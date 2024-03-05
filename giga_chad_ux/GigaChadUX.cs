using System;
using System.Collections.Generic;

using HarmonyLib;

using XRL.World;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Capabilities;

namespace Strategineer.GigaChadUX.HarmonyPatches
{
    class Helpers
    {
        public static void ReloadMissileWeaponIfNeeded(GameObject obj)
        {
            if (!obj.IsPlayer()) { return; }
            List<XRL.World.GameObject> missileWeapons = obj.GetMissileWeapons();
            if (missileWeapons != null && missileWeapons.Count > 0)
            {
                bool anyWeaponReadyToFire = false;
                int m = 0;
                for (int count2 = missileWeapons.Count; m < count2; m++)
                {
                    if (missileWeapons[m].GetPart("MissileWeapon") is MissileWeapon missileWeapon)
                    {
                        if (missileWeapon.ReadyToFire())
                        {
                            anyWeaponReadyToFire = true;
                            break;
                        }
                    }
                }
                if (!anyWeaponReadyToFire)
                {
                    CommandReloadEvent.Execute(obj);
                }
            }
        }
    }
    [HarmonyPatch(typeof(GameObject))]
    class TryReloadBeforeAutomoving
    {
        static bool EnableAutoReload => Options.GetOption("Option_Strategineer_GigaChadUX_EnableAutoReload", "Yes").EqualsNoCase("Yes");

        // todo make this work from autoexplore instead of this, also make it work for the move to edge mode
        [HarmonyPrefix]
        [HarmonyPatch("AutoMove")]
        static void Prefix(ref GameObject __instance)
        {
            if (EnableAutoReload)
            {
                //XRL.Messages.MessageQueue.AddPlayerMessage("Autoreloading if needed from auto move");
                Helpers.ReloadMissileWeaponIfNeeded(__instance);
            }
        }
        // [HarmonyPrefix]
        // [HarmonyPatch("Move")]
        //static void PrefixMove(ref GameObject __instance)
        //{
        //  if (EnableAutoReload && AutoAct.IsExploration())
        //{
        //XRL.Messages.MessageQueue.AddPlayerMessage("Autoreloading if needed from auto explore");
        //  Helpers.ReloadMissileWeaponIfNeeded(__instance);
        //  }
        //}
    }

    [HarmonyPatch(typeof(XRL.UI.Popup))]
    class LastSelectedOptionShouldBeTheDefault
    {
        // todo: figure out why this option is not appearing in the options menu
        // todo: figure out the serialization of this ideally
        static bool EnableUnsafeMode => Options.GetOption("Option_Strategineer_GigaChadUX_EnableUnsafeOptionListMemory", "Yes").EqualsNoCase("Yes");
        static Dictionary<string, int> fromOptionListMenuTitleToLastSelectedOption = new Dictionary<string, int>();
        static HashSet<string> allowedTitles = new HashSet<string> { "Select Wait Style", "Go to which point of interest?", "Which item do you want to get ?" };

        [HarmonyPrefix]
        [HarmonyPatch("ShowOptionList")]
        static bool Prefix(out string __state, string Title, IList<string> Options, ref int DefaultSelected)
        {
            //XRL.Messages.MessageQueue.AddPlayerMessage($"Prefix with Title='{Title}', DefaultSelected='{DefaultSelected}'");
            if (!string.IsNullOrEmpty(Title) &&
            (EnableUnsafeMode || allowedTitles.Contains(Title)) &&
            fromOptionListMenuTitleToLastSelectedOption.ContainsKey(Title))
            {
                int lastSelectedIndex = fromOptionListMenuTitleToLastSelectedOption[Title];
                if (0 <= lastSelectedIndex && lastSelectedIndex < Options.Count)
                {
                    //XRL.Messages.MessageQueue.AddPlayerMessage($"Setting default selected choice #{lastSelectedIndex} for '{Title}'");
                    DefaultSelected = lastSelectedIndex;
                }
            }
            __state = Title;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowOptionList")]
        static void Postfix(string __state, int __result, IList<string> Options)
        {
            //XRL.Messages.MessageQueue.AddPlayerMessage($"Postfix with lastTitle='{__state}' and __result='{__result}'");
            if (!string.IsNullOrEmpty(__state) &&
            (EnableUnsafeMode || allowedTitles.Contains(__state)) &&
            0 <= __result &&
            __result < Options.Count)
            {
                fromOptionListMenuTitleToLastSelectedOption[__state] = __result;
                //XRL.Messages.MessageQueue.AddPlayerMessage($"Saving choice indexed at {__result} for menu '{__state}'");
            }
        }
    }
}