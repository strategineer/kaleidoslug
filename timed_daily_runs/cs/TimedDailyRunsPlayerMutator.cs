using System;
using System.Diagnostics;

using XRL;
using XRL.UI;
using XRL.World;

namespace Strategineer.TimedDailyRuns
{
  [Serializable]
  public class TimeDailyRunsPart : IPart
  {
    // todo show hours if needed
    public bool hasBeenToldToWrapItUp = false;
    // todo make this all configurable
    public bool hasEverWarned = false;
    private bool hasWarnedOnce = false;
    private Stopwatch stopWatch = new Stopwatch();
    private Stopwatch lastWarnStopWatch = new Stopwatch();
    // todo add an option for this
    // todo implement optior for popups vs messages
    public TimeSpan totalTime;
    // todo might need to serialize elapsed time as we go, not sure how the stopwatch will survive serialization
    public TimeSpan elapsedTime;
    private int minutesBetweenWarnings = 15;

    public TimeDailyRunsPart()
    {
      lastWarnStopWatch.Start();
      totalTime = new TimeSpan(0, 60, 0);
      elapsedTime = new TimeSpan(0, 0, 0);
    }

    public void UpdateElapsedTime()
    {
      elapsedTime = elapsedTime + stopWatch.Elapsed;
      stopWatch.Restart();
    }

    public bool ShouldWarnAgain()
    {
      return !hasWarnedOnce || TimeLeft().Minutes % minutesBetweenWarnings == 0;
    }

    public TimeSpan TimeLeft()
    {
      return totalTime - elapsedTime;
    }

    public string FormatTimeLeft()
    {
      TimeSpan timeLeft = TimeLeft();
      if (totalTime.Hours != 0)
      {
        return $"{timeLeft.Hours}:{timeLeft.Minutes}:{timeLeft.Seconds} (hours/minutes/seconds)";
      }
      else
      {
        return $"{timeLeft.Minutes}:{timeLeft.Seconds} (minutes/seconds)";
      }
    }

    public void WarnIfNeccessary()
    {
      if (!stopWatch.IsRunning)
      {
        stopWatch.Start();
      }
      UpdateElapsedTime();
      // todo this should be in some kind of update/render method?
      TimeSpan timeLeft = totalTime - elapsedTime;
      if (!hasEverWarned)
      {
        hasEverWarned = true;
        Popup.Show($"Congrats on choosing play a real-time limited daily run!\nYou have {FormatTimeLeft()} of real time to get as far as you can.\nWhen the timer runs out your character will be sent to the shadow realm so beware.\nFeel free to save the run and come back to it later (that'll pause the timer).");
      }
      else if (timeLeft.TotalMinutes <= 0)
      {
        // todo kill the player
        Popup.Show($"Daily run over... Glad you made it this far buddy. Sending you to the shadow realm promptly.");
      }
      else if (ShouldWarnAgain())
      {
        if (timeLeft.Minutes < 5)
        {
          string msg = $"{FormatTimeLeft()} left for this daily run... Wrap it up!";
          Popup.Show(msg);
        }
        else
        {
          Popup.Show($"{FormatTimeLeft()} left for this daily run... Good Luck!");
        }
        lastWarnStopWatch.Restart();
        hasWarnedOnce = true;
      }
    }

    public override void SaveData(SerializationWriter Writer)
    {
      UpdateElapsedTime();
      base.SaveData(Writer);
    }

    public override void LoadData(SerializationReader Reader)
    {
      stopWatch.Restart();
      base.LoadData(Reader);
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
      WarnIfNeccessary();
      return base.HandleEvent(E);
    }

    public override bool HandleEvent(AfterGameLoadedEvent E)
    {
      WarnIfNeccessary();
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
        player.AddPart<TimeDailyRunsPart>();
      }
    }
  }
}