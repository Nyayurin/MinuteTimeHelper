# 这是什么

这是一套以分钟(0.7秒)为最小单位的星露谷时间

# 为什么要做这个 Helper

我在开发
[不要饿死在星露谷(Don't Starve in Stardew Valley)](https://www.nexusmods.com/stardewvalley/mods/27440)
时遇到这样一个需求:<br>
一个生命恢复 BUFF, 每2秒恢复2点生命值<br>
UpdateTickingEventArgs 中有一个 IsMultipleOf(int number) 方法可以实现每2秒运行一次逻辑<br>
但在我翻阅源码后发现这个方法检测的是 Ticks 是否是 number 的倍数,
而 Ticks 是从游戏启动起就会不停增加, 哪怕游戏暂停也不会停止增加<br>
很显然, 这个 BUFF 在暂停游戏时不应该回血, 所以上一位给这 MOD 写 C# 的使用了这个办法:<br>
```csharp
if (!Context.IsWorldReady || !Game1.shouldTimePass()) return;
if (e.IsMultipleOf(120)) {
    ...
}
```
他在运行逻辑前检查游戏时间是否会流逝, 从而避免了在暂停页面还会恢复血量的问题<br>
但这引入了另一个问题: 如果频繁暂停, 会导致这个回复的间隔时间不固定<br>
在生命恢复这个 BUFF 上不会造成太大影响, 但随后又有了另外一个需求:<br>
引入一个理智值系统, 在晚上每秒减少一定数量的理智值<br>
配合前面说的问题, 在 TAS 上可以通过在第 60 帧"恰好"暂停来规避这次扣理智, 从而实现一个晚上都不会扣理智的情况<br>
为了解决这个问题, 我选择使用星露谷时间来处理这一系列与时间相关的需求<br>
但在使用星露谷时间时, 我发现了另外的问题:<br>
星露谷中一个小时为 100, 十分钟为 10, 但没有更小的时间单位了<br>
也就是说星露谷时间最小单位是 10 而不是 1, 而星露谷 10 分钟对应现实的 7 秒钟<br>
7 秒的更新间隔太长了, 所以我决定阅读游戏源码后重新做一份时间, 并修复原版时间的一些问题

# 概念

这套时间使用类似时间戳的存储格式, 其中 1 分钟表示为 1, 1小时表示为 60<br>
星露谷时间只有 600~2600, 表示 6 点到 26 点(第二天早上 2 点)<br>
而在本套时间系统中, 时间与日期是存在一起的<br>
即可以通过它查询到具体时间, 也可以通过它查询到具体日期<br>
而对于一日时间, 它将 6~26 改为了现实的 0~24 小时制<br>
游戏时间中 26 点对应本时间第二天的凌晨 2 点, 游戏日期中处于前一天, 但在本套时间系统内已经是第二天了<br>

# 实现

本套时间参考原版时间, 正常情况下按现实 700 毫秒为游戏内 1 分钟(在骷髅洞穴等地方也会按照原版逻辑减慢流速)<br>
但由于原版影响时间的因素过多, 本套时间无法完全跟随原版时间, 所以引入了时间同步机制来解决时间不一致问题<br>
在时间同步中, 会提供一个 delta 表示原先时间与原版实际时间之间的差值<br>
若 delta 等于 0, 表示原先时间与实际时间完全同步, 无需做任何处理<br>
若 delta 大于 0, 表示原先时间慢于实际时间, 需要多执行 delta 次更新<br>
若 delta 小于 0, 表示原先时间快于实际时间, 需要少执行 delta 次更新

# 使用

为游戏安装本 Mod<br>
在您的 Mod 源码中声明一个接口类:
```csharp
public interface TimeApi {
    public long time { get; }
    public List<Action<long>> onLoad { get; }
    public List<Action<long>> onUpdate { get; }
    public List<Action<long, long>> onSync { get; }
}
```
在游戏启动后获得接口实例:
```csharp
helper.Events.GameLoop.GameLaunched += (_, _) => {
    var timeApi = helper.ModRegistry.GetApi<TimeApi>("Yurin.MinuteTimeHelper")!;
};
```
注册 update, sync 监听器:
```csharp
timeApi.onUpdate.Add(update);
timeApi.onSync.Add(sync);
```
编写 update 与 sync 代码体:
```csharp
// 用于在 delta 小于 0 时少执行
private static long wait;

// update 在正常情况下会每 0.7 秒调用一次
private static void update(long time) {
    if (wait > 0) {
        wait--;
        return;
    }
    
    ...
}

// sync 在正常情况下会每 7 秒调用一次
private static void sync(long time, long delta) {
    if (delta < 0) {
        wait += -delta;
    } else {
        // 在 delta 大于等于 0 时调用 update
        for (var i = 0; i <= delta; i++) {
            update(time);
        }
    }
}
```
请注意:<br>
内部在发生时间同步时, 不会调用 update, 这导致哪怕时间完全一致, update 也会每 7 秒少走 1 次<br>
所以你需要在 sync 内再多调用一次来弥补, 所以哪怕 delta 为 0, 你也需要调用一次 update<br>
如果你有每 x 秒执行一次的需求:
```csharp
private static long lastTime;
private static void update(long time) {
    if (wait > 0) {
        wait--;
        return;
    }
    
    // 每 3 分钟(现实 2.1 秒)执行一次
    if (time - lastTime >= 3) {
        ...

        lastTime = time;
    }
}
```