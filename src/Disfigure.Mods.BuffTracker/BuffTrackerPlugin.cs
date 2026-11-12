using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// BepInEx plugin (ADR-0006): shows on the run HUD how long until both shields come back
/// after they break, the Shield Flare kill count, and the bonus of stacking buffs. Each
/// indicator can be turned on and off from the <c>.cfg</c> file or the F7 panel (ADR-0008).
/// The per-frame work is done by <see cref="BuffTrackerBehaviour"/>.
/// </summary>
[BepInPlugin(Id, Name, Version)]
public sealed class BuffTrackerPlugin : BasePlugin
{
    public const string Id = "dalessandro.disfigure.bufftracker";

    public const string Name = "Buff Tracker";

    public const string Version = "1.0.0";

    /// <summary>The plugin log, for the behaviour that lives in the scene.</summary>
    internal static ManualLogSource SharedLog { get; private set; } = null!;

    /// <summary>Which indicators are shown, stored in the plugin's <c>.cfg</c> file.</summary>
    internal static Settings Options { get; private set; } = null!;

    public override void Load()
    {
        SharedLog = Log;
        Options = new Settings(Config, TrackerHud.Powers);
        AddComponent<BuffTrackerBehaviour>();
        Log.LogInfo($"{Name} {Version} loaded. Press F7 to open the options panel.");
    }
}
