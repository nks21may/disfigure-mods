using System.Globalization;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// The text of the stacking buff indicators, based on how they behaved in the game (backlog
/// step 11). They are shown while the card is unlocked, including at <c>+0%</c>: the values
/// change several times per second, and a line that appears and disappears that fast cannot
/// be read. <c>null</c> means the card is not owned.
/// </summary>
internal static class StackBuffs
{
    /// <summary>Kill Spree resets when <c>killSpreeTimer</c> reaches this value.</summary>
    internal const float KillSpreeSeconds = 3f;

    /// <summary>
    /// Kill Spree: <c>killSpreeBuff</c> goes up 0.05 per kill up to 1.35; <c>killSpreeTimer</c>
    /// counts up from the last kill and at 3 the bonus goes back to 1.
    /// </summary>
    internal static string? KillSpree(PlayerStats stats)
    {
        if (!stats.killSpree)
        {
            return null;
        }

        var bonus = Percentage(stats.killSpreeBuff);
        if (bonus == 0)
        {
            return "+0%";
        }

        var left = Math.Max(0f, KillSpreeSeconds - stats.killSpreeTimer);
        return string.Create(CultureInfo.InvariantCulture, $"+{bonus}% · {left:0.0}s");
    }

    /// <summary>
    /// Rapid Fire: <c>rapidFireMultiplier</c> goes up 0.15 per shot that hits, up to 1.90, and
    /// goes back to 1 about a second later. There is no field with that time.
    /// </summary>
    internal static string? RapidFire(PlayerStats stats)
    {
        return stats.rapidFire ? Bonus(stats.rapidFireMultiplier) : null;
    }

    /// <summary>
    /// Ricochet Rebound: <c>ricochetReboundMultiplier</c> goes up 0.15 per ricochet kill, up
    /// to 1.90, and is spent on the next shot.
    /// </summary>
    internal static string? RicochetRebound(PlayerStats stats)
    {
        return stats.ricochetRebound ? Bonus(stats.ricochetReboundMultiplier) : null;
    }

    /// <summary>
    /// Magnetic Rounds: <c>WeaponManager.magneticRoundMultiplier</c> goes up 0.05 per EXP
    /// picked up and is spent on the next shot.
    /// </summary>
    internal static string? MagneticRounds(PlayerStats stats, WeaponManager weapons)
    {
        return stats.magneticRounds ? Bonus(weapons.magneticRoundMultiplier) : null;
    }

    private static string Bonus(float multiplier)
    {
        return string.Create(CultureInfo.InvariantCulture, $"+{Percentage(multiplier)}%");
    }

    private static int Percentage(float multiplier)
    {
        return Math.Max(0, (int)Math.Round((multiplier - 1f) * 100f));
    }
}
