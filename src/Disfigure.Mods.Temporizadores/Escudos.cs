namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Cuánto falta para que vuelva cada escudo, según cómo cuentan sus timers en el juego
/// (resultado del paso 3 del backlog), y cuánto falta para activar la Shield Flare (paso 8).
/// <c>null</c> significa que no hay nada que mostrar: no se tiene el poder, ya está activo o
/// ya no falta nada.
/// </summary>
internal static class Escudos
{
    /// <summary>
    /// Kills con la Shield Barrier activa que activan la Shield Flare. Sale de la descripción
    /// de la carta ("100 kills while Shield Barrier is active") y del diagnóstico: el juego
    /// prende <c>shieldFlareActive</c> al llegar a 100. No está en ningún campo.
    /// </summary>
    internal const int KillsParaShieldFlare = 100;

    /// <summary>
    /// Shield Barrier: <c>shieldBarrierTimer</c> cuenta para arriba desde que se rompe, contra
    /// <c>shieldBarrierCooldown</c> (180), que vale 0 mientras no se tiene el poder.
    /// </summary>
    internal static float? FaltaBarrera(PlayerStats stats, WeaponManager armas)
    {
        if (stats.shieldBarrierCooldown <= 0f || armas.shieldBarrierActive)
        {
            return null;
        }

        var falta = stats.shieldBarrierCooldown - stats.shieldBarrierTimer;
        return falta > 0f ? falta : null;
    }

    /// <summary>
    /// Projectile Barrier: <c>projectileShieldRechargeTimer</c> salta a
    /// <c>ProjectileShieldRechargeDuration</c> (120) cuando se rompe y cuenta para abajo.
    /// </summary>
    internal static float? FaltaProyectiles(PlayerStats stats, WeaponManager armas)
    {
        if (!stats.projectileShield || armas.projectileShieldIsActive)
        {
            return null;
        }

        var falta = armas.projectileShieldRechargeTimer;
        return falta > 0f ? falta : null;
    }

    /// <summary>
    /// Shield Flare: <c>shieldActiveKills</c> cuenta las kills mientras la Shield Barrier está
    /// activa y vuelve a 0 cuando la rompen; al llegar a <see cref="KillsParaShieldFlare"/>
    /// el juego prende <c>shieldFlareActive</c>, que se apaga cuando se rompe la barrera.
    /// Devuelve las kills que lleva mientras esté desbloqueada y sin activar.
    /// </summary>
    internal static int? KillsDeShieldFlare(PlayerStats stats, WeaponManager armas)
    {
        if (!stats.shieldFlare || armas.shieldFlareActive)
        {
            return null;
        }

        return Math.Clamp(armas.shieldActiveKills, 0, KillsParaShieldFlare);
    }
}
