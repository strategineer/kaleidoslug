using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using HarmonyLib;

using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts.Skill;

namespace Strategineer.TimedDailyRuns
{
  [HarmonyPatch(typeof(PlayerStatusBar), "Update")]
  class AddPatch
  {
    [HarmonyPostfix]
    static void Postfix(PlayerStatusBar __instance)
    {
      if (The.Player != null &&
      The.Player.GetPart("TimedDailyRunsPart") is TimedDailyRunsPart part)
      {
        // todo only do this if the time has changed
        string txt = part.FormatTimeLeft();
        __instance.PlayerNameText.SetText(txt);
      }
    }
  }

  [Serializable]
  public class TimedDailyRunsPart : IPart
  {
    public bool hasEverWarned = false;
    private bool hasWarnedOnce = false;
    private Stopwatch stopWatch = new Stopwatch();
    private Stopwatch lastWarnStopWatch = new Stopwatch();
    public TimeSpan totalTime;
    public TimeSpan elapsedTime;
    // todo add option to choose between popups or text messages in thee log
    // todo pause the timer when the game is paused
    private int lastMinuteWarned = int.MaxValue;

    public TimedDailyRunsPart()
    {
      lastWarnStopWatch.Start();
      int hours = Int32.Parse(Options.GetOption("Option_Strategineer_TimedDailyRuns_TimeLimitHours"));
      int minutes = Int32.Parse(Options.GetOption("Option_Strategineer_TimedDailyRuns_TimeLimitMinutes"));
      int seconds = 0;
      totalTime = new TimeSpan(hours, minutes, seconds);

      elapsedTime = new TimeSpan(0, 0, 0);
    }

    public void UpdateElapsedTime()
    {
      elapsedTime = elapsedTime + stopWatch.Elapsed;
      stopWatch.Restart();
    }

    public bool ShouldWarnAgain()
    {
      return !hasWarnedOnce ||
      (lastMinuteWarned != TimeLeft().Minutes &&
      TimeLeft().Minutes % Int32.Parse(Options.GetOption("Option_Strategineer_TimedDailyRuns_MinutesBetweenWarnings")) == 0);
    }

    public TimeSpan TimeLeft()
    {
      return totalTime - elapsedTime;
    }

    public string FormatTimeLeft()
    {
      return TimeLeft().ToString(@"h\:mm");
    }

    public void WarnWithPopup(string msg)
    {
      lastMinuteWarned = TimeLeft().Minutes;
      lastWarnStopWatch.Restart();
      hasEverWarned = true;
      hasWarnedOnce = true;
      stopWatch.Stop();
      Popup.Show(msg);
      stopWatch.Start();
    }

    public void WarnIfNeeded()
    {
      UpdateElapsedTime();
      // todo this should be in some kind of update/render method?
      TimeSpan timeLeft = totalTime - elapsedTime;
      if (!hasEverWarned)
      {
        WarnWithPopup($"Congrats on choosing play a real-time limited daily run!\n\nYou have {FormatTimeLeft()} of real time to get as far as you can.\nWhen the timer runs out your character will be sent to the shadow realm so beware.\n\nFeel free to save the run and come back to it later (that'll pause the timer).");
      }
      else if (timeLeft.TotalMinutes <= 0)
      {
        WarnWithPopup($"Daily run over...\n\nGlad you made it this far buddy.\n\nSending you to the shadow realm promptly.");
        // todo change the death to something else because being decapitated is an achievement
        Axe_Decapitate.Decapitate(The.Player, The.Player);
      }
      else if (ShouldWarnAgain())
      {
        if (timeLeft.Minutes < 5)
        {
          WarnWithPopup($"{FormatTimeLeft()} left for this daily run...\n\nWrap it up!");
        }
        else
        {
          WarnWithPopup($"{FormatTimeLeft()} left for this daily run...\n\nGood Luck!");
        }
      }
    }

    public override void SaveData(SerializationWriter Writer)
    {
      UpdateElapsedTime();
      base.SaveData(Writer);
    }

    public override bool WantEvent(int ID, int cascade)
    {
      if (!base.WantEvent(ID, cascade) && ID != BeforeDieEvent.ID && ID != AfterGameLoadedEvent.ID)
      {
        return ID == EndTurnEvent.ID;
      }
      return true;
    }

    public override bool HandleEvent(BeforeDieEvent E)
    {
      stopWatch.Stop();
      return base.HandleEvent(E);
    }

    public override bool HandleEvent(EndTurnEvent E)
    {
      WarnIfNeeded();
      return base.HandleEvent(E);
    }

    public override bool HandleEvent(AfterGameLoadedEvent E)
    {
      WarnIfNeeded();
      return base.HandleEvent(E);
    }
  }
  [PlayerMutator]
  public class TimedDailyRunsPlayerMutator : IPlayerMutator
  {
    public void mutate(GameObject player)
    {
      if (The.Game.gameMode == "Daily")
      {
        player.AddPart<TimedDailyRunsPart>();
      }
    }
  }
}