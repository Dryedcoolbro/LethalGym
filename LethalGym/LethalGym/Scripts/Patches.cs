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
            Equipment benchPress = GameObject.FindObjectOfType<Equipment>();

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
            Equipment benchPress = GameObject.FindObjectOfType<Equipment>();

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
                psl.playerController = __instance;
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
    internal class StrengthPatches : NetworkBehaviour
    {
        [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectClientRpc")]
        [HarmonyPostfix]
        private static void BeginGrab(PlayerControllerB __instance)
        {
            PlayerStrengthLevel psl = __instance.GetComponent<PlayerStrengthLevel>();

            GrabbableObject grabbedObject = (GrabbableObject)Traverse.Create(__instance).Field("currentlyGrabbingObject").GetValue();

            if (psl.canGrab && PlayerStrengthLevel.strongerBodyStatus)
            {
                /*psl.originalCarryWeight += Mathf.Clamp(grabbedObject.itemProperties.weight - 1f, 0f, 10f);

                Debug.LogError(grabbedObject.itemProperties.itemName + " " + grabbedObject.itemProperties.weight.ToString());

                Debug.LogError("(Grab) First original weight: " + psl.originalCarryWeight.ToString());

                switch (psl.playerStrength)
                {
                    case 1:
                        __instance.carryWeight = psl.originalCarryWeight / 1f;
                        break;
                    case 2:
                        __instance.carryWeight = psl.originalCarryWeight / 1.05f;
                        break;
                    case 3:
                        __instance.carryWeight = psl.originalCarryWeight / 1.1f;
                        break;
                    case 4:
                        __instance.carryWeight = psl.originalCarryWeight / 1.3f;
                        break;
                    case 5:
                        __instance.carryWeight = psl.originalCarryWeight / 1.5f;
                        break;
                }

                Debug.LogError("(Grab) Middle instance: " + __instance.carryWeight.ToString());

                if (__instance.carryWeight < 1)
                {
                    __instance.carryWeight = 1;
                    psl.originalCarryWeight = 1;
                    Debug.LogWarning("Carry weight less than 1.0");
                }

                Debug.LogError("(grab) Last original: " + psl.originalCarryWeight.ToString());

                Debug.LogError("(Grab) Last instance: " + __instance.carryWeight.ToString());
*/
                FindObjectOfType<EquipmentNetworkHandler>().DoubleCheckGrabFunction(psl, true);
                FindObjectOfType<EquipmentNetworkHandler>().GrabWeightServerRpc(__instance.playerClientId, grabbedObject.itemProperties.weight);
                psl.canGrab = false;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "DiscardHeldObject")]
        [HarmonyPrefix]
        private static void BeginDrop(PlayerControllerB __instance, bool placeObject = false, NetworkObject parentObjectTo = null, Vector3 placePosition = default(Vector3), bool matchRotationOfParent = true)
        {
            if (!placeObject)
            {
                PlayerStrengthLevel psl = __instance.GetComponent<PlayerStrengthLevel>();

                if (psl.canDrop && PlayerStrengthLevel.strongerBodyStatus)
                {
/*                    psl.originalCarryWeight -= Mathf.Clamp(__instance.currentlyHeldObjectServer.itemProperties.weight - 1f, 0f, 10f);

                    Debug.LogError(__instance.currentlyHeldObjectServer.itemProperties.itemName);

                    Debug.LogError("(Grab) First original weight: " + psl.originalCarryWeight.ToString());*/

                    FindObjectOfType<EquipmentNetworkHandler>().ChangeWeightsFunction(__instance, psl);
                    FindObjectOfType<EquipmentNetworkHandler>().DropWeightServerRpc(__instance.playerClientId, __instance.currentlyHeldObjectServer.itemProperties.weight);
                    psl.canDrop = false;
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        private static void ChangeHUDWeight(HUDManager __instance)
        {
            PlayerStrengthLevel psl = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<PlayerStrengthLevel>();
            float weightNum = (float)Mathf.RoundToInt(Mathf.Clamp(psl.originalCarryWeight - 1f, 0f, 100f) * 105f);
            __instance.weightCounter.text = string.Format("{0} lb", weightNum);
            __instance.weightCounterAnimator.SetFloat("weight", weightNum / 130f);
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
            FindObjectOfType<EquipmentNetworkHandler>().UpdateDecorPriceStart(unlockablesList);
        }
    }
}
