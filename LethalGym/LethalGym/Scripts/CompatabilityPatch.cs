using BepInEx.Bootstrap;
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
        public static bool loadedMoreEmotes = false;

        public static AnimationClip terminal1;
        public static AnimationClip terminal2;

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
                        Type internalClassType = assembly.GetType("MoreEmotes.Patch.EmotePatch");
                        FieldInfo animatorControllerFieldLocal = internalClassType.GetField("local", BindingFlags.Public | BindingFlags.Static);
                        RuntimeAnimatorController animatorControllerLocal = (RuntimeAnimatorController)animatorControllerFieldLocal.GetValue(null);
                        if (animatorControllerLocal != null)
                        {
                            foreach (AnimationClip clip in animatorControllerLocal.animationClips)
                            {
                                if (clip.name == "TypeOnTerminal")
                                {
                                    terminal1 = clip;
                                    BenchPress.term1 = terminal1;
                                }
                            }

                            foreach (AnimationClip clip in animatorControllerLocal.animationClips)
                            {
                                if (clip.name == "TypeOnTerminal2")
                                {
                                    terminal2 = clip;
                                    BenchPress.term2 = terminal2;
                                }
                            }

                            if (!(animatorControllerLocal is AnimatorOverrideController))
                            {
                                animatorControllerFieldLocal.SetValue(null, new AnimatorOverrideController(animatorControllerLocal));
                            }
                        }

                        FieldInfo animatorControllerFieldOther = internalClassType.GetField("others", BindingFlags.Public | BindingFlags.Static);
                        RuntimeAnimatorController animatorControllerOther = (RuntimeAnimatorController)animatorControllerFieldOther.GetValue(null);
                        if (animatorControllerOther != null)
                        {
                            if (!(animatorControllerOther is AnimatorOverrideController))
                                animatorControllerFieldOther.SetValue(null, new AnimatorOverrideController(animatorControllerOther));
                        }
                    }
                }
            }
        }
    }

}
