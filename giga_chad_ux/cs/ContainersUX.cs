using System;
using System.Collections.Generic;

using HarmonyLib;

using XRL;
using XRL.World;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Capabilities;

namespace Strategineer.GigaChadUX.ContainersUX
{
  class Helpers
  {
    public static bool EnableSetDefaultPouringVolume => Options.GetOption("Option_Strategineer_GigaChadUX_EnableSetDefaultPouringVolume", "No").EqualsNoCase("Yes");
    public static int NumbersOfDramsToPourByDefault => Int32.Parse(Options.GetOption("Option_Strategineer_GigaChadUX_NumbersOfDramsToPourByDefault", "64"));
  }


  [HarmonyPatch(typeof(XRL.UI.Popup))]
  class SetDefaultVolumePercentageWhenPouring
  {
    [HarmonyPrefix]
    [HarmonyPatch("AskNumber")]
    static void Prefix(string Message, ref int Start, int Max)
    {
      if (Helpers.EnableSetDefaultPouringVolume && Message.Contains("How many drams?"))
      {
        int old = Start;
        Start = Math.Min(Helpers.NumbersOfDramsToPourByDefault, Max);
      }
    }
  }
}