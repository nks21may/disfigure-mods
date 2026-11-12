namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// How long until each shield comes back, based on how their timers count in the game
/// (backlog step 3), and how many kills the Shield Flare still needs (step 8). <c>null</c>
/// means there is nothing to show: the power is not owned, it is already active, or nothing
/// is left.
/// </summary>
internal static class Shields
{
    /// <summary>
    /// Kills with the Shield Barrier active that trigger the Shield Flare. Taken from the
    /// card description ("100 kills while Shield Barrier is active") and from the diagnostics:
    /// the game sets <c>shieldFlareActive</c> at 100. It is not stored in any field.
    /// </summary>
    internal const int ShieldFlareKillsNeeded = 100;

    /// <summary>
    /// Shield Barrier: <c>shieldBarrierTimer</c> counts up from the moment it breaks, against
    /// <c>shieldBarrierCooldown</c> (180), which is 0 while the power is not owned.
    /// </summary>
    internal static float? BarrierRemaining(PlayerStats stats, WeaponManager weapons)
    {
        if (stats.shieldBarrierCooldown <= 0f || weapons.shieldBarrierActive)
        {
            return null;
        }

        var remaining = stats.shieldBarrierCooldown - stats.shieldBarrierTimer;
        return remaining > 0f ? remaining : null;
    }

    /// <summary>
    /// Projectile Barrier: <c>projectileShieldRechargeTimer</c> jumps to
    /// <c>ProjectileShieldRechargeDuration</c> (120) when it breaks and counts down.
    /// </summary>
    internal static float? ProjectileBarrierRemaining(PlayerStats stats, WeaponManager weapons)
    {
        if (!stats.projectileShield || weapons.projectileShieldIsActive)
        {
            return null;
        }

        var remaining = weapons.projectileShieldRechargeTimer;
        return remaining > 0f ? remaining : null;
    }

    /// <summary>
    /// Shield Flare: <c>shieldActiveKills</c> counts kills while the Shield Barrier is active
    /// and goes back to 0 when it breaks; at <see cref="ShieldFlareKillsNeeded"/> the game
    /// sets <c>shieldFlareActive</c>, which turns off when the barrier breaks. Returns the
    /// current kills while the power is unlocked and not active.
    /// </summary>
    internal static int? ShieldFlareKills(PlayerStats stats, WeaponManager weapons)
    {
        if (!stats.shieldFlare || weapons.shieldFlareActive)
        {
            return null;
        }

        return Math.Clamp(weapons.shieldActiveKills, 0, ShieldFlareKillsNeeded);
    }
}
