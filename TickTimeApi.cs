using StardewModdingAPI;
using StardewValley;

namespace TickTimeHelper;

public class TickTimeApi {
    public long time { get; private set; }
    public List<Action<long>> onLoad { get; } = new();
    public List<Action<long>> onUpdate { get; } = new();
    public List<Action<long, long>> onSync { get; } = new();
    
    internal void load() {
        time = GameTime.parse(Game1.timeOfDay);
        onLoad.ForEach(action => action(time));
    }

    internal void update() {
        if (!Context.IsWorldReady || !Game1.shouldTimePass()) return;
        time++;
        onUpdate.ForEach(action => action(time));
    }

    internal void sync() {
        var syncTicks = GameTime.parse(Game1.timeOfDay);
        var delta = syncTicks - time;
        time = syncTicks;
        onSync.ForEach(action => action(syncTicks, delta));
    }
}