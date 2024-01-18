using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEditor;
using GameNetcodeStuff;
using LethalGym.Scripts;
using LethalLib.Modules;

public class PlayerStrengthLevel : NetworkBehaviour
{
    public int playerStrength = 1;
    public int currentRepsInLevel = 0;

    public static int repsNeededL1 = 25;
    public static int repsNeededL2 = 50;
    public static int repsNeededL3 = 75;
    public static int repsNeededL4 = 100;
    public static int repsNeededL5 = 200;

    public bool canGrab;
    public bool canDrop;

    public static bool strongerBodyStatus;

    public float originalCarryWeight;

    public PlayerControllerB playerController;

    public void Start()
    {
        originalCarryWeight = 1f;
        canGrab = true;
        canDrop = true;
    }

    public void Update()
    {
        
    }

    public void UpdateConfigs(bool nStrongerBodyStatus)
    {
        strongerBodyStatus = nStrongerBodyStatus;
    }

    public void addRep(Equipment bench)
    {
        currentRepsInLevel++;
        Debug.LogWarning(currentRepsInLevel.ToString());

        if (playerStrength == 1 && currentRepsInLevel >= repsNeededL1)
        {
            playerStrength++;
            currentRepsInLevel = 0;
            bench.SetWeights(playerStrength);
            Debug.Log(playerStrength + " PlayerUpgraded!");
        }

        if (playerStrength == 2 && currentRepsInLevel >= repsNeededL2)
        {
            playerStrength++;
            currentRepsInLevel = 0;
            bench.SetWeights(playerStrength);
            Debug.Log(playerStrength + " PlayerUpgraded!");
        }

        if (playerStrength == 3 && currentRepsInLevel >= repsNeededL3)
        {
            playerStrength++;
            currentRepsInLevel = 0;
            bench.SetWeights(playerStrength);
            Debug.Log(playerStrength + " PlayerUpgraded!");
        }

        if (playerStrength == 4 && currentRepsInLevel >= repsNeededL4)
        {
            playerStrength++;
            currentRepsInLevel = 0;
            bench.SetWeights(playerStrength);
            Debug.Log(playerStrength + " PlayerUpgraded!");
        }

        if (playerStrength == 5 && currentRepsInLevel >= repsNeededL5)
        {
            bench.repsText.text = "MAXED OUT";
            Debug.Log("Bro is maxed out");
        }
    }
}
