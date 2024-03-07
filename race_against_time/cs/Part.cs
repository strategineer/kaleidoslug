using System;
using System.Diagnostics;

using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts.Skill;

namespace Strategineer.RaceAgainstTime
{
  [Serializable]
  public class RaceAgainstTimePart : IPart
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

    public RaceAgainstTimePart()
    {
      totalTime = new TimeSpan(StratOptions.TimeLimitHours, StratOptions.TimeLimitMinutes, 0);
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
      TimeLeft().Minutes % Int32.Parse(Options.GetOption("Option_Strategineer_RaceAgainstTime_MinutesBetweenWarnings")) == 0);
    }

    public TimeSpan TimeLeft()
    {
      return totalTime - elapsedTime;
    }

    public string FormatTimeLeft()
    {
      return TimeLeft().ToString(@"h\:mm");
    }

    public void Warn(string msg)
    {
      lastMinuteWarned = TimeLeft().Minutes;
      lastWarnStopWatch.Restart();
      hasEverWarned = true;
      hasWarnedOnce = true;
      stopWatch.Stop();
      if (StratOptions.EnablePopupWarnings)
      {
        Popup.Show(msg);
      }
      else
      {
        XRL.Messages.MessageQueue.AddPlayerMessage(msg);
      }
      stopWatch.Start();
    }

    public void WarnIfNeeded()
    {
      UpdateElapsedTime();
      TimeSpan timeLeft = totalTime - elapsedTime;
      if (!hasEverWarned)
      {
        Warn($"You have {FormatTimeLeft()} (hours:minutes) left on the clock.\n\nBEWARE, when the timer runs out your character will die..\n\nFeel free to save and quit and come back later (that'll pause the timer).\n\nLet me know if you find any bugs!\n\n- strategineer");
      }
      else if (timeLeft.TotalMinutes <= 0)
      {
        Warn($"Time's up...\n\nGlad you made it this far.\n\nSending you to the shadow realm ASAP.");
        // todo change the death to something else because being decapitated is an achievement
        Axe_Decapitate.Decapitate(The.Player, The.Player);
      }
      else if (ShouldWarnAgain())
      {
        if (timeLeft.Minutes < 5)
        {
          Warn($"{FormatTimeLeft()} left...\n\nWrap it up!");
        }
        else
        {
          Warn($"{FormatTimeLeft()} left...\n\nGood Luck!");
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


}