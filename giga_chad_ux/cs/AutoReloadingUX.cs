using System;
using System.Collections.Generic;

using HarmonyLib;

using XRL;
using XRL.World;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Capabilities;

// todo might not want to reload right after firing a bow

namespace Strategineer.GigaChadUX.AutoReloadingUX
{
  class Helpers
  {
    public static bool EnableAutoReload => Options.GetOption("Option_Strategineer_GigaChadUX_EnableAutoReload", "Yes").EqualsNoCase("Yes");
    public static bool HasAmmoInInventory(GameObject obj)
    {
      List<GameObject> missileWeapons = obj.GetMissileWeapons();
      if (missileWeapons != null && missileWeapons.Count > 0)
      {
        int m = 0;
        for (int count2 = missileWeapons.Count; m < count2; m++)
        {
          if (missileWeapons[m].GetPart("MissileWeapon") is MissileWeapon missileWeapon)
          {
            if (missileWeapons[m].GetPart("MagazineAmmoLoader") is MagazineAmmoLoader magazineAmmoLoader)
            {
              foreach (GameObject item in obj.GetInventory())
              {
                if (magazineAmmoLoader.IsValidAmmo(item))
                {
                  return true;
                }
              }
            }
          }
        }
      }
      return false;
    }
    public static void ReloadMissileWeaponIfNeeded(GameObject obj)
    {
      if (!obj.IsPlayer()) { return; }
      List<GameObject> missileWeapons = obj.GetMissileWeapons();
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
        if (!anyWeaponReadyToFire && HasAmmoInInventory(obj))
        {
          CommandReloadEvent.Execute(obj);
        }
      }
    }
  }
  [HarmonyPatch(typeof(ActionManager))]
  class TryReloadBeforeAutomovingAutoAct
  {
    [HarmonyPrefix]
    [HarmonyPatch("RunSegment")]
    static void Prefix()
    {
      if (Helpers.EnableAutoReload &&
      !AutoAct.IsAnyCombat() &&
      (AutoAct.IsAnyExploration() || AutoAct.IsAnyResting()))
      {
        //XRL.Messages.MessageQueue.AddPlayerMessage($"Trying to reload due to AutoAct.GetDescription():{AutoAct.GetDescription()}, AutoAct.IsAnyExploration():{AutoAct.IsAnyExploration()}, AutoAct.IsAnyResting():{AutoAct.IsAnyResting()}, AutoAct.IsAnyGathering(){AutoAct.IsAnyGathering()}, AutoAct.IsAnyCombat(){AutoAct.IsAnyCombat()}");
        Helpers.ReloadMissileWeaponIfNeeded(The.Player);
      }
    }
  }

}