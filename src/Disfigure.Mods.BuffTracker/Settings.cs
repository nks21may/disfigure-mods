using BepInEx.Configuration;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// Which indicators are shown (ADR-0008): one <c>bool</c> entry per indicator in the
/// <c>[Indicators]</c> section of <c>BepInEx\config\dalessandro.disfigure.bufftracker.cfg</c>,
/// on by default. BepInEx saves the file every time a value changes. The <c>[Debug]</c>
/// section has the stack diagnostics, off by default.
/// </summary>
internal sealed class Settings
{
    private const string IndicatorsSection = "Indicators";

    private readonly ConfigFile file;
    private readonly Dictionary<string, ConfigEntry<bool>> entries = new(StringComparer.Ordinal);
    private readonly ConfigEntry<bool> stackDiagnostics;

    internal Settings(ConfigFile file, IEnumerable<string> powers)
    {
        this.file = file;
        foreach (var power in powers)
        {
            // Keys have no spaces: "Shield Barrier" becomes "ShieldBarrier".
            var key = power.Replace(" ", string.Empty, StringComparison.Ordinal);
            entries[power] = file.Bind(
                IndicatorsSection,
                key,
                true,
                $"Show the {power} indicator on the HUD.");
        }

        stackDiagnostics = file.Bind(
            "Debug",
            "StackDiagnostics",
            false,
            "Log the fields of stacking cards that have no indicator yet to "
            + "BepInEx\\BuffTracker-diagnostics.log. Only useful for development.");
    }

    internal bool StackDiagnostics => stackDiagnostics.Value;

    internal bool IsOn(string power)
    {
        return !entries.TryGetValue(power, out var entry) || entry.Value;
    }

    internal void Toggle(string power)
    {
        if (entries.TryGetValue(power, out var entry))
        {
            entry.Value = !entry.Value;
        }
    }

    /// <summary>Reads the file again, in case it was edited by hand while the game is open.</summary>
    internal void Reload()
    {
        file.Reload();
    }
}
