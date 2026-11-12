using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// A candidate field of a card: which class it lives in (<see cref="PlayerStats"/> or
/// <see cref="WeaponManager"/>) and its name.
/// </summary>
internal readonly record struct Field(bool OnWeapons, string Name);

/// <summary>
/// What needs to be found out about a stacking card: which field says it is unlocked, which
/// one holds the bonus or the stacks, and which one holds the time, if any.
/// </summary>
internal sealed record Probe(string Card, Field? Unlock, Field Value, Field? Time);

/// <summary>
/// Backlog step 11 diagnostics, off by default (<c>[Debug] StackDiagnostics</c> in the
/// <c>.cfg</c>): follows the candidate fields of the stacking cards that have no indicator yet
/// and writes a line every time the unlock or the value of one changes, with the time at that
/// moment. Since BepInEx overwrites <c>LogOutput.log</c> on every launch, it also appends to
/// <c>BepInEx\BuffTracker-diagnostics.log</c>, which keeps growing across runs.
/// </summary>
internal sealed class StackDiagnostics
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// The candidates come from the field names; the diagnostics confirm them. Kill Spree,
    /// Rapid Fire, Ricochet Rebound and Magnetic Rounds were already confirmed (step 11) and
    /// have an indicator in <see cref="StackBuffs"/>.
    /// </summary>
    internal static readonly IReadOnlyList<Probe> Probes = new[]
    {
        Probe("BERSERK", "berserk", "berserkMultiplier", "berserkTimer"),
        Probe("RAMPAGE", "rampage", "rampageMultiplier", null),
        new Probe(
            "ACCELERATING BARREL",
            new Field(false, "acceleratingBarrel"),
            new Field(true, "acceleratingBarrelBuff"),
            new Field(true, "acceleratingBarrelTimer")),
        Probe("OVERDRIVE", "overdrive", "overdriveMultiplier", "overdriveTimer"),
        Probe("FRAGMENT SURGE", "splintersurge", "splintersurgeMultiplier", "splintersurgeTimer"),
        Probe("IGNITION", "ignition", "ignitionFireRateBuff", "ignitionTimer"),
        new Probe(
            "SAW BURST",
            new Field(false, "sawBurst"),
            new Field(true, "sawBurstBuff"),
            new Field(true, "sawBurstTimer")),
        Probe("RADIANT MISSILES", null, "radiantMissilesMultiplier", "radiantMissilesTimer"),
        Probe("SHROUDED FROST", "concealedSpiral", "concealedSpiralBuff", "concealedSpiralBuffTimer"),
        Probe("SHROUDED FROST (level)", "concealedSpiral", "concealedSpiralBuffLevel", null),
        Probe("MOLTEN ROUNDS", "moltenRounds", "moltenRoundsDamageMultiplier", null),
        Probe("ONE IN THE CHAMBER", "oneInTheChamber", "oneInTheChamberMultiplier", null),
        Probe("PLAGUE", "plague", "plagueStackCount", null),
        new Probe(
            "PSYCHIC FORTIFICATION",
            new Field(false, "noxiousFortification"),
            new Field(true, "noxiousRemnantBuff"),
            null),
    };

    private readonly ManualLogSource log;
    private readonly string file = Path.Combine(Paths.BepInExRootPath, "BuffTracker-diagnostics.log");
    private readonly Dictionary<string, string> previous = new(StringComparer.Ordinal);
    private readonly HashSet<string> missing = new(StringComparer.Ordinal);
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private TimeSpan nextRead = TimeSpan.Zero;
    private bool headerWritten;

    internal StackDiagnostics(ManualLogSource log)
    {
        this.log = log;
    }

    internal void Update(PlayerStats stats, WeaponManager weapons)
    {
        if (clock.Elapsed < nextRead)
        {
            return;
        }

        nextRead = clock.Elapsed + Interval;
        if (!headerWritten)
        {
            Write($"--- game launch, {BuffTrackerPlugin.Name} {BuffTrackerPlugin.Version}");
            headerWritten = true;
        }

        foreach (var probe in Probes)
        {
            var unlock = probe.Unlock is { } u ? Read(u, stats, weapons) : "?";
            var value = Read(probe.Value, stats, weapons);
            var key = $"{unlock}|{value}";
            if (previous.TryGetValue(probe.Card, out var before) && before == key)
            {
                continue;
            }

            previous[probe.Card] = key;
            var time = probe.Time is { } t ? $" time={Read(t, stats, weapons)}" : string.Empty;
            Write($"stack | {probe.Card} | unlocked={unlock} value={value}{time}");
        }
    }

    private static Probe Probe(string card, string? unlock, string value, string? time)
    {
        return new Probe(
            card,
            unlock is null ? null : new Field(false, unlock),
            new Field(false, value),
            time is null ? null : new Field(false, time));
    }

    private string Read(Field field, PlayerStats stats, WeaponManager weapons)
    {
        object target = field.OnWeapons ? weapons : stats;
        var property = target.GetType().GetProperty(field.Name, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            if (missing.Add(field.Name))
            {
                log.LogWarning($"stack | field {field.Name} does not exist");
            }

            return "missing";
        }

        return property.GetValue(target) switch
        {
            float f => f.ToString("F2", CultureInfo.InvariantCulture),
            double d => d.ToString("F2", CultureInfo.InvariantCulture),
            IFormattable other => other.ToString(null, CultureInfo.InvariantCulture),
            var other => other?.ToString() ?? "null",
        };
    }

    private void Write(string line)
    {
        log.LogInfo(line);
        try
        {
            var stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture);
            File.AppendAllText(file, $"[{stamp}] {line}{Environment.NewLine}");
        }
        catch (IOException error)
        {
            log.LogWarning($"Could not write {file}: {error.Message}");
        }
    }
}
