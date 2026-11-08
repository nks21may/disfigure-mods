using System.Globalization;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(
    typeof(Disfigure.Mods.Temporizadores.ModTemporizadores),
    "Temporizadores",
    "0.3.0",
    "Nicolás D'Alessandro")]
[assembly: MelonGame("ColdBrew Entertainment", "Disfigure")]

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Muestra en el HUD de la partida cuánto falta para que vuelvan los dos escudos después
/// de que los rompen: la Shield Barrier y la Projectile Barrier. Se dibuja con IMGUI
/// (ADR-0005), arriba a la derecha, debajo de la puntuación.
/// </summary>
public sealed class ModTemporizadores : MelonMod
{
    // Medidas pensadas para 1080 de alto; se escalan con la resolución.
    private const float AltoDeReferencia = 1080f;
    private const float AlturaRelativa = 0.40f;
    private const float Margen = 24f;
    private const float TamanoDeIcono = 40f;
    private const float Separacion = 8f;
    private const float AnchoDelTiempo = 90f;
    private const float AnchoDelNombre = 330f;
    private const float AltoDeLinea = 48f;
    private const float TamanoDeLetra = 26f;

    private readonly TimerEnPantalla barrera = new("Shield Barrier");
    private readonly TimerEnPantalla proyectiles = new("Projectile Barrier");
    private readonly IconosDePoderes iconos = new();
    private readonly string[] poderes;

    public ModTemporizadores()
    {
        poderes = new[] { barrera.Poder, proyectiles.Poder };
    }

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

        if (hayPartida)
        {
            iconos.Actualizar(poderes, LoggerInstance);
        }

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
        var escala = Screen.height / AltoDeReferencia;
        var y = Screen.height * AlturaRelativa;
        y = DibujarSiHay(barrera, y, escala);
        DibujarSiHay(proyectiles, y, escala);
    }

    /// <summary>
    /// Deja en <c>Latest.log</c> cuándo aparece y desaparece cada timer, con los valores del
    /// juego en ese momento, para poder verificar en qué valor vuelve cada escudo.
    /// </summary>
    private void Avisar(TimerEnPantalla timer, Cambio cambio, string valores)
    {
        var que = cambio == Cambio.Aparecio ? $"aparece en {timer.Tiempo}" : "desaparece";
        LoggerInstance.Msg($"{timer.Poder}: {que} ({valores})");
    }

    /// <summary>
    /// Dibuja una línea alineada contra el borde derecho: el icono del poder y lo que falta,
    /// o el nombre si no hay icono. Devuelve dónde va la línea siguiente.
    /// </summary>
    private float DibujarSiHay(TimerEnPantalla timer, float y, float escala)
    {
        if (timer.Tiempo is null)
        {
            return y;
        }

        var icono = iconos.Buscar(timer.Poder);
        var tamanoDeLetra = Math.Max(1, (int)Math.Round(TamanoDeLetra * escala));
        var altoDelIcono = TamanoDeIcono * escala;
        var xDelTiempo = Screen.width - (Margen + AnchoDelTiempo) * escala;
        var yDelTexto = y + (altoDelIcono - tamanoDeLetra * 1.3f) / 2f;

        if (icono is not null)
        {
            var anchoDelIcono = altoDelIcono * icono.Proporcion;
            var xDelIcono = xDelTiempo - Separacion * escala - anchoDelIcono;
            GUI.DrawTextureWithTexCoords(
                new Rect(xDelIcono, y, anchoDelIcono, altoDelIcono),
                icono.Textura,
                icono.Coordenadas);
            DibujarTexto(timer.Tiempo, xDelTiempo, yDelTexto, AnchoDelTiempo * escala, tamanoDeLetra);
        }
        else
        {
            var ancho = (AnchoDelNombre + AnchoDelTiempo) * escala;
            var x = Screen.width - Margen * escala - ancho;
            DibujarTexto($"{timer.Poder}  {timer.Tiempo}", x, yDelTexto, ancho, tamanoDeLetra);
        }

        return y + AltoDeLinea * escala;
    }

    private static void DibujarTexto(string texto, float x, float y, float ancho, int tamano)
    {
        var alto = tamano * 1.5f;
        var tamanoTexto = tamano.ToString(CultureInfo.InvariantCulture);
        GUI.Label(
            new Rect(x + 2f, y + 2f, ancho, alto),
            $"<size={tamanoTexto}><b><color=#000000>{texto}</color></b></size>");
        GUI.Label(
            new Rect(x, y, ancho, alto),
            $"<size={tamanoTexto}><b><color=#FFFFFF>{texto}</color></b></size>");
    }
}
