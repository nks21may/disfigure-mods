using System.Globalization;
using BepInEx.Logging;
using UnityEngine;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Un rectángulo en píxeles de pantalla, con el origen arriba a la izquierda. Es propio y no
/// <c>UnityEngine.Rect</c>, así las cuentas no pasan por métodos de Unity.
/// </summary>
internal readonly record struct Caja(float X, float Y, float Ancho, float Alto)
{
    internal float Derecha => X + Ancho;
}

/// <summary>
/// Una fila del HUD en este frame: el timer, su sprite si lo hay, y dónde van el icono y el
/// texto.
/// </summary>
internal sealed record Fila(TimerEnPantalla Timer, Sprite? Sprite, Caja Icono, Caja Texto);

/// <summary>
/// Los timers de los dos escudos, arriba a la derecha y debajo de la puntuación. Lee el
/// juego, arma las filas una sola vez por frame, y de ahí salen el icono (Unity UI,
/// ADR-0007) y el texto (IMGUI, ADR-0005). Es una clase común y no un
/// <c>MonoBehaviour</c>, así Il2CppInterop no intenta registrar sus métodos en IL2CPP.
/// </summary>
internal sealed class HudDeTemporizadores
{
    // Medidas pensadas para 1080 de alto; se escalan con la resolución.
    private const float AltoDeReferencia = 1080f;
    private const float AlturaRelativa = 0.40f;
    private const float Margen = 24f;
    private const float TamanoDeIcono = 40f;
    private const float Separacion = 8f;
    private const float AnchoDelTiempo = 90f;
    private const float AnchoConNombre = 360f;
    private const float AltoDeLinea = 48f;
    private const float TamanoDeLetra = 26f;

    private readonly ManualLogSource registro;
    private readonly TimerEnPantalla barrera = new("Shield Barrier");
    private readonly TimerEnPantalla proyectiles = new("Projectile Barrier");
    private readonly IconosDePoderes sprites;
    private readonly IconosEnPantalla iconos;
    private List<Fila> filas = new();

    internal HudDeTemporizadores(ManualLogSource registro)
    {
        this.registro = registro;
        sprites = new IconosDePoderes(registro);
        iconos = new IconosEnPantalla(registro);
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

        var enPantalla = new[] { barrera, proyectiles }.Where(timer => timer.Tiempo is not null);
        if (iconos.Disponibles)
        {
            // Sólo se buscan los sprites de los timers que se están mostrando.
            sprites.BuscarFaltantes(enPantalla.Select(timer => timer.Poder));
        }

        filas = ArmarFilas(enPantalla);
        iconos.Mostrar(filas);
    }

    internal void Dibujar()
    {
        foreach (var fila in filas)
        {
            var texto = fila.Sprite == null
                ? $"{fila.Timer.Poder}  {fila.Timer.Tiempo}"
                : fila.Timer.Tiempo!;
            DibujarTexto(texto, fila.Texto);
        }
    }

    private List<Fila> ArmarFilas(IEnumerable<TimerEnPantalla> enPantalla)
    {
        var escala = Screen.height / AltoDeReferencia;
        var y = Screen.height * AlturaRelativa;
        var resultado = new List<Fila>();

        foreach (var timer in enPantalla)
        {
            var sprite = iconos.Disponibles ? sprites.Buscar(timer.Poder) : null;
            var altoDeFila = AltoDeLinea * escala;
            var lado = TamanoDeIcono * escala;
            var xDelTiempo = Screen.width - (Margen + AnchoDelTiempo) * escala;
            var icono = new Caja(xDelTiempo - Separacion * escala - lado, y, lado, lado);
            var texto = sprite == null
                ? new Caja(
                    Screen.width - (Margen + AnchoConNombre) * escala,
                    y,
                    AnchoConNombre * escala,
                    altoDeFila)
                : new Caja(xDelTiempo, y, AnchoDelTiempo * escala, altoDeFila);

            resultado.Add(new Fila(timer, sprite, icono, texto));
            y += altoDeFila;
        }

        return resultado;
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
    /// Texto con sombra, centrado en el alto del icono. Sólo usa <c>GUI.Label</c>:
    /// <c>GUI.DrawTextureWithTexCoords</c> crasheó el juego (ver ADR-0005).
    /// </summary>
    private static void DibujarTexto(string texto, Caja donde)
    {
        var escala = Screen.height / AltoDeReferencia;
        var tamano = Math.Max(1, (int)Math.Round(TamanoDeLetra * escala));
        var y = donde.Y + (TamanoDeIcono * escala - tamano * 1.3f) / 2f;
        var etiqueta = tamano.ToString(CultureInfo.InvariantCulture);

        GUI.Label(
            new Rect(donde.X + 2f, y + 2f, donde.Ancho, donde.Alto),
            $"<size={etiqueta}><b><color=#000000>{texto}</color></b></size>");
        GUI.Label(
            new Rect(donde.X, y, donde.Ancho, donde.Alto),
            $"<size={etiqueta}><b><color=#FFFFFF>{texto}</color></b></size>");
    }
}
