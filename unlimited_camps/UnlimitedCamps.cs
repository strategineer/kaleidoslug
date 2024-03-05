using HarmonyLib;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace Strategineer.UnlimitedCamps.HarmonyPatches
{
  [HarmonyPatch(typeof(Survival_Camp))]
  class YourPatch1
  {
    [HarmonyPrefix]
    [HarmonyPatch("AttemptCamp")]
    static bool Prefix(ref GameObject Actor, ref bool __result, ref Survival_Camp __instance)
    {
      if (Actor.AreHostilesNearby())
      {
        __result = Actor.Fail("You can't cook with hostiles nearby.");
        return false;
      }
      if (Actor.OnWorldMap())
      {
        __result = Actor.Fail("You can't cook on the world map.");
        return false;
      }
      if (!Actor.CanChangeMovementMode("Camping", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
      {
        __result = false;
        return false;
      }
      PointOfInterest one = GetPointsOfInterestEvent.GetOne(Actor, "Campfire");
      if (one != null && one.GetDistanceTo(Actor) <= 24)
      {
        GameObject @object = one.Object;
        if (@object != null)
        {
          if (Actor.IsPlayer())
          {
            if (one.IsAt(Actor))
            {
              __result = Actor.Fail("There " + @object.Is + " already " + @object.an() + " here.");
              return false;
            }
          }
        }
      }
      string text = __instance.PickDirectionS("Make Camp");
      if (text == null)
      {
        __result = false;
        return false;
      }
      Cell cellFromDirection = Actor.CurrentCell.GetCellFromDirection(text);
      if (cellFromDirection == null || cellFromDirection.ParentZone != Actor.CurrentZone)
      {
        __result = Actor.Fail("You can only build a campfire in the same zone you are in.");
        return false;
      }
      if (cellFromDirection.HasObjectWithTag("ExcavatoryTerrainFeature"))
      {
        __result = Actor.Fail("There is nothing there you can build a campfire on.");
        return false;
      }
      if (!cellFromDirection.IsEmpty())
      {
        __result = Actor.Fail("Something is in the way!");
        return false;
      }
      GameObject gameObject = Campfire.FindExtinguishingPool(cellFromDirection);
      if (gameObject != null)
      {
        __result = Actor.Fail("You cannot start a campfire in " + gameObject.t() + ".");
        return false;
      }
      IComponent<GameObject>.XDidY(Actor, "make", "camp");
      GameObject gameObject2 = ((!cellFromDirection.ParentZone.ZoneID.StartsWith("ThinWorld")) ? cellFromDirection.AddObject("Campfire") : cellFromDirection.AddObject("BlueCampfire"));
      if (Actor.IsPlayer())
      {
        gameObject2.SetIntProperty("PlayerCampfire", 1);
      }
      if (!__instance.CampedZones.Contains(Actor.CurrentZone.ZoneID))
      {
        __instance.CampedZones.Add(Actor.CurrentZone.ZoneID);
      }
      __result = true;
      return false;
    }
  }
}