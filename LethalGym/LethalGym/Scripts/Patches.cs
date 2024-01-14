using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine.Windows;
using BepInEx.Logging;
using LethalLib;
using Unity.Netcode;
using System.Collections;
using LethalLib.Modules;

namespace LethalGym.Scripts
{

    [HarmonyPatch]
    internal class LethalGymPatches
    {
        public static AnimatorOverrideController overrideController;

        //Animations
        public static AnimationClip benchEnter;
        public static AnimationClip benchRep;
        public static AnimationClip term1;
        public static AnimationClip term2;

        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        public static void AddEmote(StartOfRound __instance)
        {
            __instance.localClientAnimatorController = overrideController;
            __instance.otherClientsAnimatorController = overrideController;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void AddEmoteToPlayer(PlayerControllerB __instance)
        {
/*            if (!__instance.inTerminalMenu)
            {
                AnimatorOverrideController controller = (AnimatorOverrideController)__instance.playerBodyAnimator.runtimeAnimatorController;
                if (controller != null)
                {
                    if ((__instance.playerBodyAnimator.runtimeAnimatorController as AnimatorOverrideController)["TypeOnTerminal"] != benchEnter && (__instance.playerBodyAnimator.runtimeAnimatorController as AnimatorOverrideController)["TypeOnTerminal2"] != benchRep)
                    {
                        (__instance.playerBodyAnimator.runtimeAnimatorController as AnimatorOverrideController)["TypeOnTerminal"] = benchEnter;
                        (__instance.playerBodyAnimator.runtimeAnimatorController as AnimatorOverrideController)["TypeOnTerminal2"] = benchRep;
                    }
                }

            }*/
        }

        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        [HarmonyPostfix]
        public static void ResetAnimation(Terminal __instance)
        {
            EquipmentNetworkHandler equipmentNetworkHandler = UnityEngine.GameObject.FindObjectOfType<EquipmentNetworkHandler>();
            if (equipmentNetworkHandler != null)
            {
                equipmentNetworkHandler.BeginTerminalServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
            }
        }

        [HarmonyPatch(typeof(ShipBuildModeManager), "StoreShipObjectClientRpc")]
        [HarmonyPrefix]
        public static void KickPlayerOutClientRpc()
        {
            BenchPress benchPress = GameObject.FindObjectOfType<BenchPress>();

            if (benchPress != null)
            {
                benchPress.LeaveEquipment();
                benchPress.StopSpecialAnimation();
            }
        }

        [HarmonyPatch(typeof(ShipBuildModeManager), "StoreObjectServerRpc")]
        [HarmonyPrefix]
        public static void KickPlayerOutServerRpc()
        {
            BenchPress benchPress = GameObject.FindObjectOfType<BenchPress>();

            if (benchPress != null)
            {
                benchPress.LeaveEquipment();
            }
        }
    }

    [HarmonyPatch]
    internal class NetworkObjectManager
    {
        public static AssetBundle assetBundle;
        static GameObject networkPrefab;

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            if (networkPrefab != null)
                return;

            networkPrefab = (GameObject)assetBundle.LoadAsset("Assets/MyAssets/EquipmentNetworkHandlerPrefab.prefab");
            networkPrefab.AddComponent<EquipmentNetworkHandler>();

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = GameObject.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }

    }

    [HarmonyPatch]
    internal class StrengthValuesSaveAndLoad : NetworkBehaviour
    {
        private static StrengthValuesSaveAndLoad Instance;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        [HarmonyPatch(typeof(GameNetworkManager), "SaveGameValues")]
        [HarmonyPostfix]
        private static void SaveStrengthValues(GameNetworkManager __instance)
        {
            PlayerControllerB[] players = GameObject.FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB player in players)
            {
                PlayerStrengthLevel psl = player.GetComponent<PlayerStrengthLevel>();
                if (psl != null)
                {
                    ES3.Save("PlayerStrength" + player.playerSteamId, psl.playerStrength, __instance.currentSaveFileName);
                    ES3.Save("PlayerReps" + player.playerSteamId, psl.currentRepsInLevel, __instance.currentSaveFileName);
                    Debug.LogWarning(player.playerSteamId.ToString());
                    Debug.LogWarning(psl.playerStrength + " " + psl.currentRepsInLevel);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SendNewPlayerValuesClientRpc")]
        [HarmonyPostfix]
        private static void LoadStrengthValues(PlayerControllerB __instance)
        {
            PlayerStrengthLevel psl = __instance.gameObject.AddComponent<PlayerStrengthLevel>();

            if (psl != null)
            {
                Debug.LogWarning("psl not null");
                string saveFileName = GameObject.FindObjectOfType<GameNetworkManager>().currentSaveFileName;
                Debug.LogWarning(saveFileName);
                //GameObject.FindObjectOfType<GameNetworkManager>().gameObject.AddComponent<StrengthValuesSaveAndLoad>().StartCoroutine(LoadValues(psl, __instance, saveFileName));
                while (__instance.playerSteamId == 0)
                {
                    
                }
                psl.playerStrength = ES3.Load<int>("PlayerStrength" + __instance.playerSteamId, saveFileName, 1);
                psl.currentRepsInLevel = ES3.Load<int>("PlayerReps" + __instance.playerSteamId, saveFileName, 0);
                Debug.LogWarning(__instance.playerSteamId.ToString());
                Debug.LogWarning(psl.playerStrength + " " + psl.currentRepsInLevel);
            }
        }
    }

    [HarmonyPatch]
    internal class ConfigApply : NetworkBehaviour
    {
        public static UnlockablesList unlockablesList;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPostfix]
        private static void UpdateBenchPriceController()
        {
            FindObjectOfType<EquipmentNetworkHandler>().UpdateBenchPriceStart(unlockablesList);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void log()
        {
            Plugin.Logger.LogWarning(Config.Instance.strongerBody);
        }
    }
}
