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
using JetBrains.Annotations;
using LethalGym;

public class Equipment : NetworkBehaviour
{
    public PlayerActions playerActions;
    public InteractTrigger trigger;
    public Animator animator;
    public TMP_Text nameText;
    public TMP_Text repsText;
    public GameObject[] weights;
    public string EquipmentName;
    public int playerStrengthLevel;

    public AnimatorOverrideController overrideController;
    public AnimationClip equipmentEnter;
    public AnimationClip equipmentRep;
    public static AnimationClip term1;
    public static AnimationClip term2;

    // ANIMATIONS
    public static AnimationClip benchEnter;
    public static AnimationClip benchRep;
    public static AnimationClip squatEnter;
    public static AnimationClip squatRep;

    public int reps;
    public bool inUse;
    public bool isRepping;

    public PlayerControllerB playerController;
    public PlayerStrengthLevel psl;

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
        nameText.text = "Enter Bench";
        repsText.text = "To Start Count";

        if (EquipmentName == "Bench")
        {
            equipmentEnter = benchEnter;
            equipmentRep = benchRep;
        }
        else if (EquipmentName == "Squat")
        {
            equipmentEnter = squatEnter;
            equipmentRep = squatRep;
        }
        else
        {
            Debug.LogError("No Name?");
        }
    }
    
    public void Update()
    {
        if (nameText != null && repsText != null && psl != null)
        {
            nameText.text = playerController.playerUsername + "'s";
            repsText.text = "Reps:" + psl.currentRepsInLevel;
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
        playerController.gameObject.GetComponent<PlayerStrengthLevel>().addRep(this);
    }

    public IEnumerator playRep()
    {
        playerController.playerBodyAnimator.Play("SpecialAnimations.TypeOnTerminal2", -1, 0f);
        animator.Play("Base Layer.EquipmentRepAnimation", -1, 0f);
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
                overrideController = (AnimatorOverrideController)player.playerBodyAnimator.runtimeAnimatorController;
                psl = player.GetComponent<PlayerStrengthLevel>();
                playerStrengthLevel = player.GetComponent<PlayerStrengthLevel>().playerStrength;

                break;
            }
        }

        if (equipmentEnter == null)
        {
            Debug.LogError("equipmentEnter is null");
        }
        else
        {
            Debug.LogError(equipmentEnter.name.ToString());
        }
        
        if (equipmentRep == null)
        {
            Debug.LogError("equipmentRep is null");
        }
        else
        {
            Debug.LogError(equipmentRep.name.ToString());
        }

        Debug.LogError(overrideController.name.ToString());

        overrideController["TypeOnTerminal"] = equipmentEnter;
        overrideController["TypeOnTermina2"] = equipmentRep;
        SetWeights(playerStrengthLevel);
        Debug.LogError(playerStrengthLevel);
        animator.ResetTrigger("EquipmentRep");
        animator.SetTrigger("EquipmentRep");
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
                overrideController = (AnimatorOverrideController)player.playerBodyAnimator.runtimeAnimatorController;
                psl = player.GetComponent<PlayerStrengthLevel>();
                if (psl == null)
                {
                    Debug.LogWarning("no psl");
                }

                playerStrengthLevel = player.GetComponent<PlayerStrengthLevel>().playerStrength;
                break;
            }
        }

        if (equipmentEnter == null)
        {
            Debug.LogError("equipmentEnter is null");
        }
        else
        {
            Debug.LogError(equipmentEnter.name.ToString());
        }

        if (equipmentRep == null)
        {
            Debug.LogError("equipmentRep is null");
        }
        else
        {
            Debug.LogError(equipmentRep.name.ToString());
        }
        
        if (overrideController == null)
        {
            Debug.LogError("no overridecontroller");
        }

        overrideController["TypeOnTerminal"] = equipmentEnter;
        overrideController["TypeOnTerminal2"] = equipmentRep;
        SetWeights(playerStrengthLevel);
        Debug.LogError(playerStrengthLevel);
        animator.ResetTrigger("EquipmentRep");
        animator.SetTrigger("EquipmentRep");
        reps = 1;
        playerController.gameObject.GetComponent<PlayerStrengthLevel>().addRep(this);
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
        psl = null;
    }

    public void SetWeights(int levelNumber)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            if (i == 0)
            {
                weights[i].SetActive(true);
            }
            else
            {
                weights[i].SetActive(false);
            }
        }

        if (levelNumber == 1)
        {
            weights[0].SetActive(true);
        }
        if (levelNumber >= 2)
        {
            weights[1].SetActive(true);
        }
        if (levelNumber >= 3)
        {
            weights[2].SetActive(true);
        }
        if (levelNumber >= 4)
        {
            weights[3].SetActive(true);
        }
        if (levelNumber >= 5)
        {
            weights[4].SetActive(true);
        }
        if (levelNumber >= 6)
        {
            weights[5].SetActive(true);
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

