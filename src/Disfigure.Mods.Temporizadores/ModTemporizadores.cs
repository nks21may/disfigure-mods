using System.Diagnostics;
using System.Globalization;
using Il2Cpp;
using MelonLoader;

[assembly: MelonInfo(
    typeof(Disfigure.Mods.Temporizadores.ModTemporizadores),
    "Temporizadores",
    "0.1.0",
    "Nicolás D'Alessandro")]
[assembly: MelonGame("ColdBrew Entertainment", "Disfigure")]

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Muestra en el HUD de la partida los timers que el juego guarda pero no muestra, empezando
/// por los de los dos escudos.
/// </summary>
public sealed class ModTemporizadores : MelonMod
{
    private static readonly TimeSpan IntervaloDeRegistro = TimeSpan.FromSeconds(1);

    private readonly Stopwatch reloj = Stopwatch.StartNew();
    private TimeSpan proximoRegistro = TimeSpan.Zero;

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Temporizadores cargado.");
    }

    public override void OnUpdate()
    {
        if (reloj.Elapsed < proximoRegistro)
        {
            return;
        }

        proximoRegistro = reloj.Elapsed + IntervaloDeRegistro;
        RegistrarEscudos();
    }

    /// <summary>
    /// Diagnóstico del paso 3 del backlog: una línea por segundo con los campos de los dos
    /// escudos, para ver en el juego cuáles son los de cada uno y cómo cuentan. Fuera de una
    /// partida no hay <see cref="PlayerStats"/> y no escribe nada.
    /// </summary>
    private void RegistrarEscudos()
    {
        var refs = GameRefs.Instance;
        if (refs == null)
        {
            return;
        }

        var stats = refs.playerStats;
        var armas = refs.weaponManager;
        if (stats == null || armas == null)
        {
            return;
        }

        LoggerInstance.Msg(string.Create(
            CultureInfo.InvariantCulture,
            $"escudos | barrera: cooldown={stats.shieldBarrierCooldown:F2} "
            + $"timer={stats.shieldBarrierTimer:F2} activa={armas.shieldBarrierActive} "
            + $"| proyectiles: tiene={stats.projectileShield} "
            + $"timer={armas.projectileShieldRechargeTimer:F2} "
            + $"duracion={armas.ProjectileShieldRechargeDuration:F2} "
            + $"duracionStats={stats.ProjectileShieldRechargeDuration:F2} "
            + $"activo={armas.projectileShieldIsActive} "
            + $"| tempShieldHits={armas.tempShieldHits}"));
    }
}
