using XRL;
using XRL.World;

namespace Strategineer.RaceAgainstTime
{
  [PlayerMutator]
  public class RaceAgainstTimePlayerMutator : IPlayerMutator
  {
    public void mutate(GameObject player)
    {
      if (!StratOptions.EnableForDailyRunOnly || The.Game.gameMode == "Daily")
      {
        player.AddPart<RaceAgainstTimePart>();
      }
    }
  }
}