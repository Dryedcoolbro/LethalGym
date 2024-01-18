using BepInEx.Bootstrap;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace LethalGym.Scripts
{
    [HarmonyPatch]
    internal class MoreEmotesPatcher
    {
        public static bool moreEmotes;

        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPostfix]
        public static void ApplyPatch()
        {
            if (Plugin.IsModLoaded("MoreEmotes"))
            {
                if (Chainloader.PluginInfos.TryGetValue("MoreEmotes", out var pluginInfo))
                {
                    Assembly assembly = pluginInfo.Instance.GetType().Assembly;
                    if (assembly != null)
                    {
                        Plugin.Logger.LogWarning("Applying compatibility patch for More_Emotes");

                        Type internalClassType = assembly.GetType("MoreEmotes.Patch.EmotePatch");
                        FieldInfo animatorControllerFieldLocal = internalClassType.GetField("local", BindingFlags.Public | BindingFlags.Static);
                        RuntimeAnimatorController animatorControllerLocal = (RuntimeAnimatorController)animatorControllerFieldLocal.GetValue(null);
                        if (animatorControllerLocal != null)
                        {
                            if (!(animatorControllerLocal is AnimatorOverrideController))
                                animatorControllerFieldLocal.SetValue(null, new AnimatorOverrideController(animatorControllerLocal));
                        }

                        FieldInfo animatorControllerFieldOther = internalClassType.GetField("others", BindingFlags.Public | BindingFlags.Static);
                        RuntimeAnimatorController animatorControllerOther = (RuntimeAnimatorController)animatorControllerFieldOther.GetValue(null);
                        if (animatorControllerOther != null)
                        {
                            if (!(animatorControllerOther is AnimatorOverrideController))
                                animatorControllerFieldOther.SetValue(null, new AnimatorOverrideController(animatorControllerOther));
                        }

                        moreEmotes = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        public static void AddEmote(StartOfRound __instance)
        {

            __instance.localClientAnimatorController = new AnimatorOverrideController(__instance.localClientAnimatorController);
            __instance.otherClientsAnimatorController = new AnimatorOverrideController(__instance.otherClientsAnimatorController);
        }

    }
}