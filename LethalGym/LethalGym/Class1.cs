using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using LethalLib.Modules;
using LethalGym.Scripts;

namespace LethalGym
{

    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        //netcode-patch "A:\Mods\CustomModding\Games\LethalCompany\LethalGym\LethalGym\LethalGym\bin\Debug\netstandard2.1\LethalGym.dll" "A:\Mods\CustomModding\Games\LethalCompany\LethalGym\LethalGym\LethalGym\Dependencies"

        private const string modGUID = "Dryedcoolbro.LethalGym";
        private const string modName = "LethalGym";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        internal static ManualLogSource Logger;

        public static new Config Config { get; private set; }

        public static AssetBundle assetBundle;

        public static AnimationClip pushupAnimation;
        public static UnlockablesList unlockablesList;

        public static bool IsModLoaded(string guid) => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(guid);

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Config = new(base.Config);

            //Netcode Patching
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalequipment"));

            LoadAnimations();

            if (pushupAnimation == null)
            {
                Logger.LogError("pushupAnim not exist!");
            }
            else
            {
                Logger.LogInfo(pushupAnimation.name);
            }

            if (assetBundle == null)
            {
                Logger.LogError("Assetbundle not exist!");
            }
            else
            {
                Logger.LogInfo(assetBundle.ToString());
                Logger.LogInfo(assetBundle.name);
            }

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(LethalGymPatches));
            harmony.PatchAll(typeof(NetworkObjectManager));
            harmony.PatchAll(typeof(StrengthValuesSaveAndLoad));
            harmony.PatchAll(typeof(StrengthPatches));
            harmony.PatchAll(typeof(ConfigApply));
            harmony.PatchAll(typeof(BenchPress));
            harmony.PatchAll(typeof(MoreEmotesPatcher));
            harmony.PatchAll(typeof(PlayerStrengthLevel));
            harmony.PatchAll(typeof(Config));

            NetworkObjectManager.assetBundle = assetBundle;

            unlockablesList = assetBundle.LoadAsset<UnlockablesList>("Assets/MyAssets/Unlockables.asset");

            unlockablesList = unlockablesList;
            LoadNetworkPrefabs();
            RegisterUnlockables();

        }

        public static void LoadAnimations()
        {
            //Bench
            BenchPress.benchEnter = assetBundle.LoadAsset<AnimationClip>("Assets/MyAssets/Bench/BenchPressStart.anim");
            BenchPress.benchRep = assetBundle.LoadAsset<AnimationClip>("Assets/MyAssets/Bench/BenchRep.anim");
        }

        public static void LoadNetworkPrefabs()
        {
            //Bench
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(unlockablesList.unlockables[0].prefabObject);
        }

        public static void RegisterUnlockables()
        {
            //Bench
            Unlockables.RegisterUnlockable(unlockablesList.unlockables[0], StoreType.Decor, null, null, null, 60);
            ConfigApply.unlockablesList = unlockablesList;
        }
    }
}