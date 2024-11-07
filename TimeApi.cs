using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MinuteTimeHelper;

public class TimeApi {
    public long time { get; private set; }
    public List<Action<long>> onLoad { get; } = new();
    public List<Action<long>> onUpdate { get; } = new();
    public List<Action<long, long>> onSync { get; } = new();
    private long interval;

    internal void load() {
        time = parse(Game1.timeOfDay);
        interval = 0;
        onLoad.ForEach(action => action(time));
    }

    internal void update() {
        if (!Context.IsWorldReady || !Game1.shouldTimePass()) return;
        var gameTime = Game1.currentGameTime;
        interval += gameTime.ElapsedGameTime.Milliseconds;
        var location = Game1.currentLocation;
        if (interval > Game1.realMilliSecondsPerGameMinute + location.ExtraMillisecondsPerInGameMinute) {
            time++;
            interval = 0;
            onUpdate.ForEach(action => action(time));
        }
    }

    internal void sync() {
        var syncTicks = parse(Game1.timeOfDay);
        var delta = syncTicks - time;
        time = syncTicks;
        interval = 0;
        onSync.ForEach(action => action(syncTicks, delta));
    }
    
    private static long parse(int time) {
        // 1230 -> 30 -> 3
        var timescale = time % 100 / 10;
        // 1230 -> 12 -> 12
        // 2400 -> 24 -> 0
        var hours = time / 100 % 24;
        var hoursTimescale = hours * 6;
        var days = SDate.Now().DaysSinceStart - 1;
        if (hours < 6) days++;
        var daysTimescale = days * 24 * 6;
        return (timescale + hoursTimescale + daysTimescale) * 10L;
    }
}