using System;
using System.Collections.Generic;

using HarmonyLib;

using XRL.UI;

namespace Strategineer.GigaChadUX.ContainersUX
{
  class Helpers
  {
    public static bool EnableSetDefaultPouringVolume => Options.GetOption("Option_Strategineer_GigaChadUX_EnableSetDefaultPouringVolume", "No").EqualsNoCase("Yes");
    public static int NumbersOfDramsToPourByDefault => Int32.Parse(Options.GetOption("Option_Strategineer_GigaChadUX_NumbersOfDramsToPourByDefault", "64"));
  }


  [HarmonyPatch(typeof(Popup))]
  class SetDefaultVolumePercentageWhenPouring
  {
    [HarmonyPrefix]
    [HarmonyPatch("AskNumber")]
    static void Prefix(string Message, ref int Start, int Max)
    {
      if (Helpers.EnableSetDefaultPouringVolume &&
      Message.Contains("How many drams?") &&
      Start == 64)
      {
        int old = Start;
        Start = Math.Min(Helpers.NumbersOfDramsToPourByDefault, Max);
      }
    }
  }
}