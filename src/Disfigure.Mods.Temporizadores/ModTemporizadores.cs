using System.Globalization;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(
    typeof(Disfigure.Mods.Temporizadores.ModTemporizadores),
    "Temporizadores",
    "0.2.0",
    "Nicolás D'Alessandro")]
[assembly: MelonGame("ColdBrew Entertainment", "Disfigure")]

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Muestra en el HUD de la partida cuánto falta para que vuelvan los dos escudos después
/// de que los rompen: la Shield Barrier y la Projectile Barrier. Se dibuja con IMGUI
/// (ADR-0005).
/// </summary>
public sealed class ModTemporizadores : MelonMod
{
    private const float MargenIzquierdo = 24f;
    private const float AlturaRelativa = 0.30f;
    private const float Ancho = 420f;
    private const float AltoDeLinea = 34f;
    private const int TamanoDeLetra = 24;

    private readonly TimerEnPantalla barrera = new("Shield Barrier");
    private readonly TimerEnPantalla proyectiles = new("Projectile Barrier");

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Temporizadores cargado.");
    }

    public override void OnUpdate()
    {
        var refs = GameRefs.Instance;
        var stats = refs == null ? null : refs.playerStats;
        var armas = refs == null ? null : refs.weaponManager;
        var hayPartida = stats != null && armas != null;

        var cambioBarrera = barrera.Actualizar(
            hayPartida ? Escudos.FaltaBarrera(stats!, armas!) : null);
        if (cambioBarrera != Cambio.Ninguno)
        {
            var valores = hayPartida
                ? string.Create(
                    CultureInfo.InvariantCulture,
                    $"timer={stats!.shieldBarrierTimer:F2} "
                    + $"cooldown={stats.shieldBarrierCooldown:F2} "
                    + $"activa={armas!.shieldBarrierActive}")
                : "sin partida";
            Avisar(barrera, cambioBarrera, valores);
        }

        var cambioProyectiles = proyectiles.Actualizar(
            hayPartida ? Escudos.FaltaProyectiles(stats!, armas!) : null);
        if (cambioProyectiles != Cambio.Ninguno)
        {
            var valores = hayPartida
                ? string.Create(
                    CultureInfo.InvariantCulture,
                    $"timer={armas!.projectileShieldRechargeTimer:F2} "
                    + $"duracion={armas.ProjectileShieldRechargeDuration:F2} "
                    + $"activo={armas.projectileShieldIsActive}")
                : "sin partida";
            Avisar(proyectiles, cambioProyectiles, valores);
        }
    }

    public override void OnGUI()
    {
        var y = Screen.height * AlturaRelativa;
        y = DibujarSiHay(barrera.Texto, y);
        DibujarSiHay(proyectiles.Texto, y);
    }

    /// <summary>
    /// Deja en <c>Latest.log</c> cuándo aparece y desaparece cada timer, con los valores del
    /// juego en ese momento, para poder verificar en qué valor vuelve cada escudo.
    /// </summary>
    private void Avisar(TimerEnPantalla timer, Cambio cambio, string valores)
    {
        var que = cambio == Cambio.Aparecio ? $"aparece {timer.Texto}" : "desaparece";
        LoggerInstance.Msg($"{timer.Nombre}: {que} ({valores})");
    }

    /// <summary>Dibuja una línea con sombra y devuelve dónde va la siguiente.</summary>
    private static float DibujarSiHay(string? texto, float y)
    {
        if (texto is null)
        {
            return y;
        }

        GUI.Label(
            new Rect(MargenIzquierdo + 2f, y + 2f, Ancho, AltoDeLinea),
            $"<size={TamanoDeLetra}><b><color=#000000>{texto}</color></b></size>");
        GUI.Label(
            new Rect(MargenIzquierdo, y, Ancho, AltoDeLinea),
            $"<size={TamanoDeLetra}><b><color=#FFFFFF>{texto}</color></b></size>");
        return y + AltoDeLinea;
    }
}
