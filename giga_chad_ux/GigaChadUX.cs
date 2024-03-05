using System;
using System.Collections.Generic;

using HarmonyLib;

using XRL.World;
using XRL.UI;

namespace Strategineer.GigaChadUX.HarmonyPatches
{
    [HarmonyPatch(typeof(XRL.UI.Popup))]
    class LastSelectedOptionShouldBeTheDefault
    {
        // todo: figure out why this option is not appearing in the options menu
        // todo: figure out the serialization of this ideally
        public static bool Unsafe => Options.GetOption("Option_Strategineer_GigaChadUX_EnableUnsafeOptionListMemory", "Yes").EqualsNoCase("Yes");
        static Dictionary<string, int> fromOptionListMenuTitleToLastSelectedOption = new Dictionary<string, int>();
        static HashSet<string> allowedTitles = new HashSet<string> { "Select Wait Style", "Go to which point of interest?", "Which item do you want to get ?" };

        [HarmonyPrefix]
        [HarmonyPatch("ShowOptionList")]
        static bool Prefix(out string __state, string Title, IList<string> Options, ref int DefaultSelected)
        {
            //XRL.Messages.MessageQueue.AddPlayerMessage($"Prefix with Title='{Title}', DefaultSelected='{DefaultSelected}'");
            if (!string.IsNullOrEmpty(Title) &&
            (Unsafe || allowedTitles.Contains(Title)) &&
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
            (Unsafe || allowedTitles.Contains(__state)) &&
            0 <= __result &&
            __result < Options.Count)
            {
                fromOptionListMenuTitleToLastSelectedOption[__state] = __result;
                //XRL.Messages.MessageQueue.AddPlayerMessage($"Saving choice indexed at {__result} for menu '{__state}'");
            }
        }
    }
}