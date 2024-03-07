using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

using Qud.UI;
using XRL;
using XRL.UI;

namespace Strategineer.RaceAgainstTime
{
  [HarmonyPatch(typeof(Popup))]
  class PauseTimerWhenPauseMenuOpen
  {
    [HarmonyPrefix]
    [HarmonyPatch("ShowOptionList")]
    public static void Prefix(IList<string> Options)
    {
      string[] optionsArray = (string[])Options;
      if (The.Player != null &&
The.Player.GetPart("RaceAgainstTimePart") is RaceAgainstTimePart part)
      {
        if (optionsArray.Contains("Key Mapping"))
        {
          part.StopStopwatch();
        }
      }
    }
    [HarmonyPostfix]
    [HarmonyPatch("ShowOptionList")]
    public static void Postfix(IList<string> Options)
    {
      string[] optionsArray = (string[])Options;
      if (The.Player != null &&
The.Player.GetPart("RaceAgainstTimePart") is RaceAgainstTimePart part)
      {
        if (optionsArray.Contains("Key Mapping"))
        {
          part.StartStopwatch();
        }
      }
    }
    [HarmonyPatch(typeof(PlayerStatusBar), "Update")]
    class AddPatch
    {
      [HarmonyPostfix]
      static void Postfix(PlayerStatusBar __instance)
      {
        if (The.Player != null &&
        The.Player.GetPart("RaceAgainstTimePart") is RaceAgainstTimePart part)
        {
          // todo only do this if the time has changed
          __instance.PlayerNameText.SetText(part.FormatTimeLeft());
        }
      }
    }

  }
}