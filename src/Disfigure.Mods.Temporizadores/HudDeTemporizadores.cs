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
/// Una fila del HUD en este frame: el indicador, su sprite si lo hay, y dónde van el icono y
/// el texto.
/// </summary>
internal sealed record Fila(Indicador Indicador, Sprite? Sprite, Caja Icono, Caja Texto);

/// <summary>
/// Los indicadores de los escudos, arriba a la derecha y debajo de la puntuación: cuánto
/// falta para que vuelvan la Shield Barrier y la Projectile Barrier, y cuántas kills lleva la
/// Shield Flare. Lee el juego, arma las filas una sola vez por frame, y de ahí salen el icono
/// (Unity UI, ADR-0007) y el texto (IMGUI, ADR-0005). Es una clase común y no un
/// <c>MonoBehaviour</c>, así Il2CppInterop no intenta registrar sus métodos en IL2CPP.
/// </summary>
internal sealed class HudDeTemporizadores
{
    internal const string ShieldBarrier = "Shield Barrier";
    internal const string ProjectileBarrier = "Projectile Barrier";
    internal const string ShieldFlare = "Shield Flare";
    internal const string KillSpree = "Kill Spree";
    internal const string RapidFire = "Rapid Fire";
    internal const string RicochetRebound = "Ricochet Rebound";
    internal const string MagneticRounds = "Magnetic Rounds";

    /// <summary>Los poderes con indicador, en el orden del HUD, del panel y del <c>.cfg</c>.</summary>
    internal static readonly IReadOnlyList<string> Poderes = new[]
    {
        ShieldBarrier, ProjectileBarrier, ShieldFlare,
        KillSpree, RapidFire, RicochetRebound, MagneticRounds,
    };

    // Medidas pensadas para 1080 de alto; se escalan con la resolución.
    private const float AltoDeReferencia = 1080f;
    private const float AlturaRelativa = 0.40f;
    private const float Margen = 24f;
    private const float TamanoDeIcono = 40f;
    private const float Separacion = 8f;
    private const float AnchoDelValor = 170f;
    private const float AnchoConNombre = 360f;
    private const float AltoDeLinea = 48f;
    private const float TamanoDeLetra = 26f;

    private readonly ManualLogSource registro;
    private readonly Configuracion configuracion;
    private readonly Indicador barrera = new(ShieldBarrier);
    private readonly Indicador proyectiles = new(ProjectileBarrier);
    private readonly Indicador flare = new(ShieldFlare);
    private readonly Indicador killSpree = new(KillSpree);
    private readonly Indicador rapidFire = new(RapidFire);
    private readonly Indicador ricochetRebound = new(RicochetRebound);
    private readonly Indicador magneticRounds = new(MagneticRounds);
    private readonly Indicador[] indicadores;
    private readonly IconosDePoderes sprites;
    private readonly IconosEnPantalla iconos;
    private readonly DiagnosticoDeStacks diagnostico;
    private List<Fila> filas = new();

    internal HudDeTemporizadores(ManualLogSource registro, Configuracion configuracion)
    {
        this.registro = registro;
        this.configuracion = configuracion;
        diagnostico = new DiagnosticoDeStacks(registro);
        indicadores = new[]
        {
            barrera, proyectiles, flare, killSpree, rapidFire, ricochetRebound, magneticRounds,
        };
        sprites = new IconosDePoderes(registro);
        iconos = new IconosEnPantalla(registro);
    }

    internal void Actualizar()
    {
        var refs = GameRefs.Instance;
        var stats = refs == null ? null : refs.playerStats;
        var armas = refs == null ? null : refs.weaponManager;
        var hayPartida = stats != null && armas != null;

        if (hayPartida)
        {
            diagnostico.Actualizar(stats!, armas!);
        }

        var faltaBarrera = hayPartida ? Escudos.FaltaBarrera(stats!, armas!) : null;
        Actualizar(barrera, faltaBarrera is { } fb ? FormatoDeTiempo.Formatear(fb) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"timer={stats!.shieldBarrierTimer:F2} "
                + $"cooldown={stats.shieldBarrierCooldown:F2} "
                + $"activa={armas!.shieldBarrierActive}"));

        var faltaProyectiles = hayPartida ? Escudos.FaltaProyectiles(stats!, armas!) : null;
        Actualizar(proyectiles, faltaProyectiles is { } fp ? FormatoDeTiempo.Formatear(fp) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"timer={armas!.projectileShieldRechargeTimer:F2} "
                + $"duracion={armas.ProjectileShieldRechargeDuration:F2} "
                + $"activo={armas.projectileShieldIsActive}"));

        var killsFlare = hayPartida ? Escudos.KillsDeShieldFlare(stats!, armas!) : null;
        Actualizar(
            flare,
            killsFlare is { } kf
                ? string.Create(CultureInfo.InvariantCulture, $"{kf}/{Escudos.KillsParaShieldFlare}")
                : null,
            () => string.Create(
                CultureInfo.InvariantCulture,
                $"kills={armas!.shieldActiveKills} desbloqueada={stats!.shieldFlare} "
                + $"activa={armas.shieldFlareActive} barrera={armas.shieldBarrierActive}"));

        Actualizar(killSpree, hayPartida ? Stacks.KillSpree(stats!) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"buff={stats!.killSpreeBuff:F2} timer={stats.killSpreeTimer:F2}"));
        Actualizar(rapidFire, hayPartida ? Stacks.RapidFire(stats!) : null, () =>
            string.Create(CultureInfo.InvariantCulture, $"multiplicador={stats!.rapidFireMultiplier:F2}"));
        Actualizar(ricochetRebound, hayPartida ? Stacks.RicochetRebound(stats!) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"multiplicador={stats!.ricochetReboundMultiplier:F2}"));
        Actualizar(magneticRounds, hayPartida ? Stacks.MagneticRounds(stats!, armas!) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"multiplicador={armas!.magneticRoundMultiplier:F2}"));

        var enPantalla = indicadores.Where(indicador => indicador.Texto is not null).ToList();
        if (iconos.Disponibles)
        {
            // Sólo se buscan los sprites de los indicadores que se están mostrando.
            sprites.BuscarFaltantes(enPantalla.Select(indicador => indicador.Poder));
        }

        filas = ArmarFilas(enPantalla);
        iconos.Mostrar(filas);

        void Actualizar(Indicador indicador, string? texto, Func<string> valores)
        {
            // Un indicador apagado en el .cfg o en el panel no se muestra (ADR-0008).
            var cambio = indicador.Actualizar(
                configuracion.EstaPrendido(indicador.Poder) ? texto : null);
            if (cambio != Cambio.Ninguno)
            {
                Avisar(indicador, cambio, hayPartida ? valores() : "sin partida");
            }
        }
    }

    internal void Dibujar()
    {
        foreach (var fila in filas)
        {
            var texto = fila.Sprite == null
                ? $"{fila.Indicador.Poder}  {fila.Indicador.Texto}"
                : fila.Indicador.Texto!;
            DibujarTexto(texto, fila.Texto);
        }
    }

    private List<Fila> ArmarFilas(IEnumerable<Indicador> enPantalla)
    {
        var escala = Screen.height / AltoDeReferencia;
        var y = Screen.height * AlturaRelativa;
        var resultado = new List<Fila>();

        foreach (var indicador in enPantalla)
        {
            var sprite = iconos.Disponibles ? sprites.Buscar(indicador.Poder) : null;
            var altoDeFila = AltoDeLinea * escala;
            var lado = TamanoDeIcono * escala;
            var xDelValor = Screen.width - (Margen + AnchoDelValor) * escala;
            var icono = new Caja(xDelValor - Separacion * escala - lado, y, lado, lado);
            var texto = sprite == null
                ? new Caja(
                    Screen.width - (Margen + AnchoConNombre) * escala,
                    y,
                    AnchoConNombre * escala,
                    altoDeFila)
                : new Caja(xDelValor, y, AnchoDelValor * escala, altoDeFila);

            resultado.Add(new Fila(indicador, sprite, icono, texto));
            y += altoDeFila;
        }

        return resultado;
    }

    /// <summary>
    /// Deja en <c>BepInEx\LogOutput.log</c> cuándo aparece y desaparece cada indicador, con los
    /// valores del juego en ese momento, para poder verificar en qué valor se va cada uno.
    /// </summary>
    private void Avisar(Indicador indicador, Cambio cambio, string valores)
    {
        var que = cambio == Cambio.Aparecio ? $"aparece en {indicador.Texto}" : "desaparece";
        registro.LogInfo($"{indicador.Poder}: {que} ({valores})");
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
