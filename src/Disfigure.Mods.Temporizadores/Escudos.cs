namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Cuánto falta para que vuelva cada escudo, según cómo cuentan sus timers en el juego
/// (resultado del paso 3 del backlog). <c>null</c> significa que no hay nada que mostrar:
/// no se tiene el poder, el escudo está activo o ya no falta nada.
/// </summary>
internal static class Escudos
{
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
}
