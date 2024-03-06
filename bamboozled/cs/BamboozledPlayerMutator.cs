using System;

using XRL; // to abbreviate XRL.PlayerMutator and XRL.IPlayerMutator
using XRL.World; // to abbreviate XRL.World.GameObject
using XRL.World.Effects;

namespace Strategineer.BamboozleMode
{
  [Serializable]
  public class Bamboozled : Confused
  {
    public Bamboozled()
      : base(Int32.MaxValue, 1, 0)
    {
      this.DisplayName = "{{K-C-m-M-r-c-CK|bamboozled}}";
    }
    public override int GetEffectType()
    {
      // same as confused but not removable (I think this does that?)
      return 83886082;
    }
  }
  [PlayerMutator]
  public class BamboozledPlayerMutator : IPlayerMutator
  {
    public void mutate(GameObject player)
    {
      player.ApplyEffect(new Bamboozled());
    }
  }
}