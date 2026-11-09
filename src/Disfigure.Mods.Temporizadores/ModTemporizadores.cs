using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Plugin de BepInEx (ADR-0006): muestra en el HUD de la partida cuánto falta para que
/// vuelvan los dos escudos después de que los rompen, y las kills de la Shield Flare. Cada
/// indicador se prende y apaga desde el <c>.cfg</c> o desde el panel de F7 (ADR-0008). El
/// trabajo de cada frame lo hace <see cref="ComportamientoTemporizadores"/>.
/// </summary>
[BepInPlugin(Identificador, "Temporizadores", Version)]
public sealed class ModTemporizadores : BasePlugin
{
    public const string Identificador = "dalessandro.disfigure.temporizadores";

    public const string Version = "0.8.0";

    /// <summary>El log del plugin, para el comportamiento que vive en la escena.</summary>
    internal static ManualLogSource Registro { get; private set; } = null!;

    /// <summary>Qué indicadores se muestran, guardado en el <c>.cfg</c> del plugin.</summary>
    internal static Configuracion Opciones { get; private set; } = null!;

    public override void Load()
    {
        Registro = Log;
        Opciones = new Configuracion(Config, HudDeTemporizadores.Poderes);
        AddComponent<ComportamientoTemporizadores>();
        Log.LogInfo("Temporizadores cargado. F7 abre el panel de opciones.");
    }
}
