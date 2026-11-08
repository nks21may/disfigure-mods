using System.Globalization;
using BepInEx.Logging;
using UnityEngine;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Los timers de los dos escudos: los lee del juego y los dibuja con IMGUI (ADR-0005),
/// arriba a la derecha, debajo de la puntuación. Es una clase común y no un
/// <c>MonoBehaviour</c>, así Il2CppInterop no intenta registrar sus métodos en IL2CPP.
/// </summary>
internal sealed class HudDeTemporizadores
{
    // Medidas pensadas para 1080 de alto; se escalan con la resolución.
    private const float AltoDeReferencia = 1080f;
    private const float AlturaRelativa = 0.40f;
    private const float Margen = 24f;
    private const float AnchoDeLinea = 360f;
    private const float AltoDeLinea = 40f;
    private const float TamanoDeLetra = 26f;

    private readonly ManualLogSource registro;
    private readonly TimerEnPantalla barrera = new("Shield Barrier");
    private readonly TimerEnPantalla proyectiles = new("Projectile Barrier");

    internal HudDeTemporizadores(ManualLogSource registro)
    {
        this.registro = registro;
    }

    internal void Actualizar()
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

    internal void Dibujar()
    {
        var escala = Screen.height / AltoDeReferencia;
        var y = Screen.height * AlturaRelativa;
        y = DibujarSiHay(barrera, y, escala);
        DibujarSiHay(proyectiles, y, escala);
    }

    /// <summary>
    /// Deja en <c>BepInEx\LogOutput.log</c> cuándo aparece y desaparece cada timer, con los
    /// valores del juego en ese momento, para poder verificar en qué valor vuelve cada escudo.
    /// </summary>
    private void Avisar(TimerEnPantalla timer, Cambio cambio, string valores)
    {
        var que = cambio == Cambio.Aparecio ? $"aparece en {timer.Tiempo}" : "desaparece";
        registro.LogInfo($"{timer.Poder}: {que} ({valores})");
    }

    /// <summary>
    /// Dibuja una línea contra el borde derecho, con sombra, y devuelve dónde va la siguiente.
    /// Sólo usa <c>GUI.Label</c>: <c>GUI.DrawTextureWithTexCoords</c> crasheó el juego (ver
    /// ADR-0005).
    /// </summary>
    private static float DibujarSiHay(TimerEnPantalla timer, float y, float escala)
    {
        if (timer.Tiempo is null)
        {
            return y;
        }

        var tamano = Math.Max(1, (int)Math.Round(TamanoDeLetra * escala))
            .ToString(CultureInfo.InvariantCulture);
        var ancho = AnchoDeLinea * escala;
        var alto = AltoDeLinea * escala;
        var x = Screen.width - Margen * escala - ancho;
        var texto = $"{timer.Poder}  {timer.Tiempo}";

        GUI.Label(
            new Rect(x + 2f, y + 2f, ancho, alto),
            $"<size={tamano}><b><color=#000000>{texto}</color></b></size>");
        GUI.Label(
            new Rect(x, y, ancho, alto),
            $"<size={tamano}><b><color=#FFFFFF>{texto}</color></b></size>");
        return y + alto;
    }
}
