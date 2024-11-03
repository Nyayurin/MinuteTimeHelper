using StardewModdingAPI.Utilities;

namespace TickTimeHelper;

public static class GameTime {
    // 10 minutes (real world time 7.3 seconds)
    public const long TICKS_PER_TIMESCALE = 438;
    public const long TICKS_PER_HOUR = 6 * TICKS_PER_TIMESCALE;
    public const long TICKS_PER_DAY = 24 * 6 * TICKS_PER_TIMESCALE;
    
    public static long parse(int time) {
        // 1230 -> 30 -> 3
        var timescale = time % 100 / 10;
        // 1230 -> 12 -> 12
        // 2400 -> 24 -> 0
        var hours = time / 100 % 24;
        var hoursTimescale = hours * 6;
        var days = SDate.Now().DaysSinceStart;
        if (hours < 6) days++;
        var daysTimescale = days * 24 * 6;
        return (timescale + hoursTimescale + daysTimescale) * TICKS_PER_TIMESCALE;
    }
}