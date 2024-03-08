using System;
using System.Diagnostics;

using XRL;
using XRL.UI;
using XRL.World;
using XRL.Liquids;
using XRL.World.Parts;

namespace Strategineer.RaceAgainstTime
{
  [Serializable]
  public class RaceAgainstTimePart : IPart
  {
    public bool hasAttemptedToKillThePlayerCharacter = false;
    public bool hasEverWarned = false;
    private bool hasWarnedOnce = false;
    private Stopwatch stopwatch = new Stopwatch();
    private Stopwatch lastWarnStopWatch = new Stopwatch();
    public TimeSpan totalTime;
    public TimeSpan elapsedTime;
    // todo add option to choose between popups or text messages in thee log
    // todo pause the timer when the game is paused
    private int lastMinuteWarned = int.MaxValue;

    public void StartStopwatch()
    {
      stopwatch.Start();
    }
    public void StopStopwatch()
    {
      stopwatch.Stop();
    }

    public RaceAgainstTimePart()
    {
      totalTime = new TimeSpan(StratOptions.TimeLimitHours, StratOptions.TimeLimitMinutes, 0);
      elapsedTime = new TimeSpan(0, 0, 0);
    }

    public void UpdateElapsedTime()
    {
      elapsedTime += stopwatch.Elapsed;
      stopwatch.Restart();
    }

    public bool ShouldWarnAgain()
    {
      return !hasWarnedOnce ||
      (!hasAttemptedToKillThePlayerCharacter &&
      lastMinuteWarned != TimeLeft().Minutes &&
      TimeLeft().Minutes % Int32.Parse(Options.GetOption("Option_Strategineer_RaceAgainstTime_MinutesBetweenWarnings")) == 0);
    }

    public TimeSpan TimeLeft()
    {
      return totalTime - elapsedTime;
    }

    public string FormatTimeLeft(bool friendly = true)
    {
      TimeSpan timeLeft = TimeLeft();
      if (!friendly)
      {
        if (timeLeft < TimeSpan.Zero)
        {
          return "BEEP BEEP BEEP";
        }
        if (timeLeft.Hours == 0)
        {
          if (timeLeft.Minutes == 0)
          {
            return $"{timeLeft.Seconds}s";
          }
          else
          {
            return $"{timeLeft.Minutes}min";
          }
        }
        return timeLeft.ToString(@"h\:mm");
      }
      string hour_noun = timeLeft.Hours == 1 ? "hour" : "hours";
      string minute_noun = timeLeft.Minutes == 1 ? "minute" : "minutes";
      string second_noun = timeLeft.Seconds == 1 ? "second" : "seconds";
      if (timeLeft.Minutes == 0 && timeLeft.Seconds == 0)
      {
        return $"{timeLeft.Hours} {hour_noun}";
      }
      if (timeLeft.Hours == 0 && timeLeft.Minutes == 0)
      {
        return $"{timeLeft.Seconds} {second_noun}";
      }
      if (timeLeft.Hours == 0 && timeLeft.Seconds == 0)
      {
        return $"{timeLeft.Minutes} {minute_noun}";
      }
      else
      {
        return $"{timeLeft.Hours} {hour_noun} and {timeLeft.Minutes} {minute_noun}";
      }
    }

    public void Warn(string msg)
    {
      lastMinuteWarned = TimeLeft().Minutes;
      lastWarnStopWatch.Restart();
      hasEverWarned = true;
      hasWarnedOnce = true;
      stopwatch.Stop();
      if (StratOptions.EnablePopupWarnings)
      {
        Popup.Show(msg);
      }
      else
      {
        XRL.Messages.MessageQueue.AddPlayerMessage(msg);
      }
      stopwatch.Start();
    }

    public void WarnIfNeeded()
    {
      UpdateElapsedTime();
      TimeSpan timeLeft = totalTime - elapsedTime;
      if (!hasEverWarned)
      {
        Warn($"Tick Tock... Tick Tock...\n\nA sturdy collar tugs at your neck.\n\n\"You have {FormatTimeLeft()} left on the clock, don't disappoint us, we're watching you.\"");
      }
      else if (!hasAttemptedToKillThePlayerCharacter &&
      timeLeft.TotalHours <= 0)
      {
        hasAttemptedToKillThePlayerCharacter = true;
        // todo change the death to something else because being decapitated is an achievement
        if (StratOptions.EnableKillPlayerCharacterWhenTimerRunsOut)
        {
          Warn($"The beeping from the collar stops, only to be replaced by a loud electrical humming. \n\n\"Thank you for your service.\"");
          The.Player.Die(The.Player, "race against time collar", "You exploded.", The.Player.It + " @@exploded.");
        }
        else
        {
          Warn($"The collar at your neck loosens and falls to the ground.\n\n\"Thank you for your service, that was a pleasant surprise. We'll be in touch.\"");
        }
      }
      else if (ShouldWarnAgain())
      {
        Warn($"{FormatTimeLeft()} left...\n\n");
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
      stopwatch.Stop();
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