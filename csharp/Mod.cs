using StardewModdingAPI;

namespace MinuteTimeHelper;

internal class Mod : StardewModdingAPI.Mod {
    private readonly TimeApi timeApi = new();
    public override void Entry(IModHelper helper) {
        helper.Events.GameLoop.SaveLoaded += (_, _) => timeApi.load();
        helper.Events.GameLoop.UpdateTicking += (_, _) => timeApi.update();
        helper.Events.GameLoop.TimeChanged += (_, _) => timeApi.sync();
    }

    public override TimeApi GetApi() {
        return timeApi;
    }
}