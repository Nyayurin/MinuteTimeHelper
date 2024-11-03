using StardewModdingAPI;

namespace TickTimeHelper;

internal class Mod : StardewModdingAPI.Mod {
    private readonly TickTimeApi tickTimeApi = new();
    public override void Entry(IModHelper helper) {
        helper.Events.GameLoop.SaveLoaded += (_, _) => tickTimeApi.load();
        helper.Events.GameLoop.UpdateTicking += (_, _) => tickTimeApi.update();
        helper.Events.GameLoop.TimeChanged += (_, _) => tickTimeApi.sync();
    }

    public override TickTimeApi GetApi() {
        return tickTimeApi;
    }
}