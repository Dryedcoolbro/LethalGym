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
            GameObject.FindObjectOfType<BenchPress>().LeaveEquipment();
            GameObject.FindObjectOfType<BenchPress>().StopSpecialAnimation();
        }

        [HarmonyPatch(typeof(ShipBuildModeManager), "StoreObjectServerRpc")]
        [HarmonyPrefix]
        public static void KickPlayerOutServerRpc()
        {
            GameObject.FindObjectOfType<BenchPress>().LeaveEquipment();
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
}
