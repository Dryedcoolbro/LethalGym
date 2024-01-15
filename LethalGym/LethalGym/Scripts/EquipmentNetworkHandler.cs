﻿using GameNetcodeStuff;
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
        public static bool strongerBody;

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        public static EquipmentNetworkHandler Instance { get; private set; }

        // Check Weights
        public void DoubleCheckGrabFunction(PlayerStrengthLevel psl, bool GrabOrDrop)
        {
            StartCoroutine(DoubleCheckGrab(psl, GrabOrDrop));
        }

        public IEnumerator DoubleCheckGrab(PlayerStrengthLevel psl, bool GrabOrDrop)
        {
            yield return new WaitForSeconds(0.1f);
            if (GrabOrDrop)
            {
                psl.canGrab = true;
            }
            else
            {
                psl.canDrop = true;
            }
        }

        public void ChangeWeightsFunction(PlayerControllerB player, PlayerStrengthLevel psl)
        {
            StartCoroutine(ChangeWeights(player, psl));
        } 

        public IEnumerator ChangeWeights(PlayerControllerB player, PlayerStrengthLevel psl)
        {
            yield return new WaitForSeconds(0.1f);

            switch (psl.playerStrength)
            {
                case 1:
                    player.carryWeight = psl.originalCarryWeight / 1f;
                    break;
                case 2:
                    player.carryWeight = psl.originalCarryWeight / 1.05f;
                    break;
                case 3:
                    player.carryWeight = psl.originalCarryWeight / 1.1f;
                    break;
                case 4:
                    player.carryWeight = psl.originalCarryWeight / 1.3f;
                    break;
                case 5:
                    player.carryWeight = psl.originalCarryWeight / 1.5f;
                    break;
            }

            Debug.LogError("(Grab) Middle instance: " + player.carryWeight.ToString());

            if (player.carryWeight < 1)
            {
                player.carryWeight = 1;
                psl.originalCarryWeight = 1;
                Debug.LogWarning("Carry weight less than 1.0");
            }

            Debug.LogError("(grab) Last original: " + psl.originalCarryWeight.ToString());

            Debug.LogError("(Grab) Last instance: " + player.carryWeight.ToString());

            DoubleCheckGrabFunction(psl, false);
            psl.canDrop = false;
        }

        public static void setStrongerBody(bool strongerBodyValue)
        {
            strongerBody = strongerBodyValue;

            PlayerStrengthLevel[] psls = FindObjectsOfType<PlayerStrengthLevel>();
            foreach (PlayerStrengthLevel psl in psls)
            {
                psl.UpdateStrongerBodyStatus(strongerBody);
            }
        }

        // Sync Weights
        [ServerRpc]
        public void GrabWeightServerRpc(ulong playerID, float grabbedObjectWeight)
        {
            PlayerControllerB player = null;
            PlayerStrengthLevel psl = null;

            PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB ePlayer in allPlayers)
            {
                if (ePlayer.playerClientId == playerID)
                {
                    player = ePlayer;
                    psl = player.GetComponent<PlayerStrengthLevel>();
                    break;
                }
            }

            GrabWeightClientRpc(playerID, grabbedObjectWeight);
        }

        [ClientRpc]
        public void GrabWeightClientRpc(ulong playerID, float grabbedObjectWeight)
        {
            PlayerControllerB player = null;
            PlayerStrengthLevel psl = null;

            PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB ePlayer in allPlayers)
            {
                if (ePlayer.playerClientId == playerID)
                {
                    player = ePlayer;
                    psl = player.GetComponent<PlayerStrengthLevel>();
                    break;
                }
            }

            psl.originalCarryWeight += Mathf.Clamp(grabbedObjectWeight - 1f, 0f, 10f);

            switch (psl.playerStrength)
            {
                case 1:
                    player.carryWeight = psl.originalCarryWeight / 1f;
                    break;
                case 2:
                    player.carryWeight = psl.originalCarryWeight / 1.05f;
                    break;
                case 3:
                    player.carryWeight = psl.originalCarryWeight / 1.1f;
                    break;
                case 4:
                    player.carryWeight = psl.originalCarryWeight / 1.3f;
                    break;
                case 5:
                    player.carryWeight = psl.originalCarryWeight / 1.5f;
                    break;
            }

            if (player.carryWeight < 1)
            {
                player.carryWeight = 1;
                psl.originalCarryWeight = 1;
            }

        }

        [ServerRpc]
        public void DropWeightServerRpc(ulong playerID, float grabbedObjectWeight)
        {
            PlayerControllerB player = null;
            PlayerStrengthLevel psl = null;

            PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB ePlayer in allPlayers)
            {
                if (ePlayer.playerClientId == playerID)
                {
                    player = ePlayer;
                    psl = player.GetComponent<PlayerStrengthLevel>();
                    break;
                }
            }

            DropWeightClientRpc(playerID, grabbedObjectWeight);
        }

        [ClientRpc]
        public void DropWeightClientRpc(ulong playerID, float grabbedObjectWeight)
        {
            PlayerControllerB player = null;
            PlayerStrengthLevel psl = null;

            PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB ePlayer in allPlayers)
            {
                if (ePlayer.playerClientId == playerID)
                {
                    player = ePlayer;
                    psl = player.GetComponent<PlayerStrengthLevel>();
                    break;
                }
            }

            psl.originalCarryWeight -= Mathf.Clamp(grabbedObjectWeight - 1f, 0f, 10f);

            switch (psl.playerStrength)
            {
                case 1:
                    player.carryWeight = psl.originalCarryWeight / 1f;
                    break;
                case 2:
                    player.carryWeight = psl.originalCarryWeight / 1.05f;
                    break;
                case 3:
                    player.carryWeight = psl.originalCarryWeight / 1.1f;
                    break;
                case 4:
                    player.carryWeight = psl.originalCarryWeight / 1.3f;
                    break;
                case 5:
                    player.carryWeight = psl.originalCarryWeight / 1.5f;
                    break;
            }

            if (player.carryWeight < 1)
            {
                player.carryWeight = 1;
                psl.originalCarryWeight = 1;
            }
        }

        // Update Bench Price
        public void UpdateDecorPriceStart(UnlockablesList unlockablesList)
        {
            StartCoroutine(UpdateDecorPrice(unlockablesList));
        }

        public IEnumerator UpdateDecorPrice(UnlockablesList unlockablesList)
        {
            yield return new WaitForSeconds(2f);

            if (Config.Instance.strongerBody)
            {
                for (int i = 0; i < Unlockables.registeredUnlockables.Count; i++)
                {
                    if (Unlockables.registeredUnlockables[i].unlockable == unlockablesList.unlockables[0])
                    {
                        Unlockables.UpdateUnlockablePrice(Unlockables.registeredUnlockables[i].unlockable, 299);
                        continue;
                    }
                    else if (Unlockables.registeredUnlockables[i].unlockable == unlockablesList.unlockables[1])
                    {
                        Unlockables.UpdateUnlockablePrice(Unlockables.registeredUnlockables[i].unlockable, 299);
                        continue;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Unlockables.registeredUnlockables.Count; i++)
                {
                    if (Unlockables.registeredUnlockables[i].unlockable == unlockablesList.unlockables[0])
                    {
                        Unlockables.UpdateUnlockablePrice(Unlockables.registeredUnlockables[i].unlockable, 1);
                        continue;
                    }
                    else if (Unlockables.registeredUnlockables[i].unlockable == unlockablesList.unlockables[1])
                    {
                        Unlockables.UpdateUnlockablePrice(Unlockables.registeredUnlockables[i].unlockable, 1);
                        continue;
                    }
                }
            }

            setStrongerBody(Config.Instance.strongerBody);
        }

        // Terminal RPC
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

