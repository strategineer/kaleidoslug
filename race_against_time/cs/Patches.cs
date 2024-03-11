using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
      try
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
      catch { }
    }
    [HarmonyPostfix]
    [HarmonyPatch("ShowOptionList")]
    public static void Postfix(IList<string> Options)
    {
      try
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
      catch { }
    }
    [HarmonyPatch(typeof(PlayerStatusBar), "Update")]
    class AddPatch
    {

      static void UpdateTimerUI(PlayerStatusBar instance)
      {
        try
        {
          if (The.Player != null &&
          The.Player.GetPart("RaceAgainstTimePart") is RaceAgainstTimePart part)
          {
            // todo only do this if the time has changed
            instance.PlayerNameText.SetText(part.FormatTimeLeft(false));
          }
        }
        catch { }
      }
      [HarmonyPrefix]
      static void Prefix(PlayerStatusBar __instance)
      {
        UpdateTimerUI(__instance);
      }
      [HarmonyPostfix]
      static void Postfix(PlayerStatusBar __instance)
      {
        UpdateTimerUI(__instance);
      }
    }

  }
}