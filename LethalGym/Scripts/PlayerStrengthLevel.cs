using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEditor;

public class PlayerStrengthLevel : NetworkBehaviour
{
    public int playerStrength = 0;
    public int currentRepsInLevel = 0;

    public static int maxRepsToUpgrade = 100;

    public void Update()
    {
        if (currentRepsInLevel >= 10)
        {
            playerStrength++;
            currentRepsInLevel = 0;
            Debug.Log(playerStrength + " PlayerUpgraded!");
        }
    }

    public void addRep()
    {
        currentRepsInLevel++;
        Debug.LogWarning(currentRepsInLevel.ToString());
    }
}
