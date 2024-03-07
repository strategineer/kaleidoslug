using HarmonyLib;

using Qud.UI;
using XRL;

namespace Strategineer.RaceAgainstTime
{
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