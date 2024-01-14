using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LethalGym.Scripts
{
    public class EquipmentNetworkHandler : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        public static EquipmentNetworkHandler Instance { get; private set; }

        public void UpdateBenchPriceStart(UnlockablesList unlockablesList)
        {
            StartCoroutine(UpdateBenchPrice(unlockablesList));
        }

        public IEnumerator UpdateBenchPrice(UnlockablesList unlockablesList)
        {
            yield return new WaitForSeconds(5f);

            if (Config.Instance.strongerBody)
            {
                for (int i = 0; i < Unlockables.registeredUnlockables.Count; i++)
                {
                    if (Unlockables.registeredUnlockables[i].unlockable == unlockablesList.unlockables[0])
                    {
                        Unlockables.UpdateUnlockablePrice(Unlockables.registeredUnlockables[i].unlockable, 299);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Unlockables.registeredUnlockables.Count; i++)
                {
                    if (Unlockables.registeredUnlockables[i].unlockable == unlockablesList.unlockables[0])
                    {
                        Unlockables.UpdateUnlockablePrice(Unlockables.registeredUnlockables[i].unlockable, 60);
                        break;
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void BeginTerminalServerRPC(ulong playerID)
        {
            PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB player in allPlayers)
            {
                if (player.playerClientId == playerID)
                {
                    AnimatorOverrideController controller = (AnimatorOverrideController)player.playerBodyAnimator.runtimeAnimatorController;
                    Debug.LogWarning(player.playerBodyAnimator.runtimeAnimatorController.ToString());
                    resetAnimations(controller);
                    break;
                }
            }
            BeginTerminalClientRPC(playerID);
        }

        [ClientRpc]
        public void BeginTerminalClientRPC(ulong playerID)
        {
            PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB player in allPlayers)
            {
                if (player.playerClientId == playerID)
                {
                    AnimatorOverrideController controller = (AnimatorOverrideController)player.playerBodyAnimator.runtimeAnimatorController;
                    resetAnimations(controller);
                    break;
                }
            }

        }

        public static void resetAnimations(AnimatorOverrideController controller)
        {
            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
            controller.GetOverrides(overrides);
            for (int i = 0; i < overrides.Count; ++i)
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, null);
            controller.ApplyOverrides(overrides);
        }
    }
}

