using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Events;
using TMPro;
using System;
using System.Numerics;
using LethalGym.Scripts;

public class BenchPress : NetworkBehaviour
{
    public PlayerActions playerActions;
    public InteractTrigger trigger;
    public Animator animator;
    public TMP_Text repCounterText;

    public static AnimatorOverrideController overrideController;
    public static AnimationClip benchEnter;
    public static AnimationClip benchRep;
    public static AnimationClip term1;
    public static AnimationClip term2;

    public int reps;
    public bool inUse;
    public bool isRepping;

    public static PlayerControllerB playerController;
    public static PlayerControllerB playerInBench;

    public void Awake()
    {
        inUse = false;

        playerActions = new PlayerActions();
        playerActions.Movement.Enable();
        trigger = GetComponent<InteractTrigger>();
    }

    private void OnEnable()
    {
        playerActions.Movement.Interact.performed += BackOut;
        playerActions.Movement.Use.performed += Rep;
    }

    private void OnDisable()
    {
        playerActions.Movement.Interact.performed -= BackOut;
        playerActions.Movement.Use.performed -= Rep;
    }

    public void Start()
    {
        Debug.LogWarning(term1);
        Debug.LogWarning(term2);
    }
    
    public void Update()
    {
        if (repCounterText != null)
        {
            repCounterText.text = "Reps:" + reps.ToString();
        }
    }

    public void Rep(InputAction.CallbackContext context)
    {
        if (!inUse)
        {
            return;
        }
        if (isRepping)
        {
            return;
        }
        if (playerController.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            return;
        }
        RepServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepServerRpc()
    {
        StartCoroutine(playRep());
        reps++;
        RepClientRpc(reps);
    }

    [ClientRpc]
    public void RepClientRpc(int serverReps)
    {
        reps = serverReps;
        StartCoroutine(playRep());
    }

    public IEnumerator playRep()
    {
        playerController.playerBodyAnimator.Play("SpecialAnimations.TypeOnTerminal2", -1, 0f);
        animator.Play("Base Layer.BarbellRep", -1, 0f);
        isRepping = true;
        yield return new WaitForSeconds(1);
        isRepping = false;
    }

    public void OnEnter(PlayerControllerB player)
    {
        Debug.LogWarning("Entered");
        //inUse = true;
        //overrideController["TypeOnTerminal"] = benchEnter;
        //overrideController["TypeOnTerminal2"] = benchRep;
        PlayerControllerB playerWhoTriggered = GameNetworkManager.Instance.localPlayerController;
        //playerWhoTriggered.playerBodyAnimator.runtimeAnimatorController = overrideController;
        //playerController = playerWhoTriggered;
        //animator.ResetTrigger("BarbellRep");
        //animator.SetTrigger("BarbellRep");
        //reps = 1;
        OnEnterServerRpc(playerWhoTriggered.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnEnterServerRpc(ulong playerInBenchID)
    {
        inUse = true;
        PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
        foreach (PlayerControllerB player in allPlayers)
        {
            if (player.playerClientId == playerInBenchID)
            {
                playerController = player;
                playerInBench = player;
                overrideController = (AnimatorOverrideController)player.playerBodyAnimator.runtimeAnimatorController;
                break;
            }
        }
        overrideController["TypeOnTerminal"] = benchEnter;
        overrideController["TypeOnTermina2"] = benchRep;
        animator.ResetTrigger("BarbellRep");
        animator.SetTrigger("BarbellRep");
        reps = 1;
        OnEnterClientRpc(playerInBenchID);
    }

    [ClientRpc]
    public void OnEnterClientRpc(ulong playerInBenchID)
    {
        inUse = true;
        PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();
        foreach (PlayerControllerB player in allPlayers)
        {
            if (player.playerClientId == playerInBenchID)
            {
                playerController = player;
                playerInBench = player;
                overrideController = (AnimatorOverrideController)player.playerBodyAnimator.runtimeAnimatorController;
                break;
            }
        }
        overrideController["TypeOnTerminal"] = benchEnter;
        overrideController["TypeOnTerminal2"] = benchRep;
        animator.ResetTrigger("BarbellRep");
        animator.SetTrigger("BarbellRep");
        reps = 1;
    }

    public void BackOut(InputAction.CallbackContext context)
    {
        if (!inUse)
        {
            return;
        }
        if (playerController.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            return;
        }
        StopSpecialAnimation();
        BackOutServerRpc();
    }

    public void StopSpecialAnimation()
    {
        trigger.StopSpecialAnimation();
    }

    [ServerRpc(RequireOwnership = false)]
    public void BackOutServerRpc()
    {
        Debug.LogWarning("BACKOUT CONFIRM");

        LeaveEquipment();
        BackOutClientRpc();
    }

    [ClientRpc]
    public void BackOutClientRpc()
    {
        Debug.LogWarning("BACKOUT CONFIRM");

        LeaveEquipment();
    }

    public void LeaveEquipment()
    {
        animator.SetTrigger("StopAnimation");
        inUse = false; 
        playerController = null;
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
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
        controller.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; ++i)
            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, null);
        controller.ApplyOverrides(overrides);
    }
}

