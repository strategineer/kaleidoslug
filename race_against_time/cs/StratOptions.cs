using XRL.UI;

namespace Strategineer.RaceAgainstTime
{
  public class StratOptions
  {
    public static bool EnableForDailyRunOnly => Options.GetOption("Option_Strategineer_RaceAgainstTime_EnableForDailyRunOnly").EqualsNoCase("Yes");
    public static bool EnablePopupWarnings => Options.GetOption("Option_Strategineer_RaceAgainstTime_EnablePopupWarnings").EqualsNoCase("Yes");
    public static int TimeLimitHours => int.Parse(Options.GetOption("Option_Strategineer_RaceAgainstTime_TimeLimitHours"));
    public static int TimeLimitMinutes => int.Parse(Options.GetOption("Option_Strategineer_RaceAgainstTime_TimeLimitMinutes"));
  }
}