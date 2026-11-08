using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Plugin de BepInEx (ADR-0006): muestra en el HUD de la partida cuánto falta para que
/// vuelvan los dos escudos después de que los rompen. El trabajo de cada frame lo hace
/// <see cref="ComportamientoTemporizadores"/>.
/// </summary>
[BepInPlugin(Identificador, "Temporizadores", "0.4.1")]
public sealed class ModTemporizadores : BasePlugin
{
    public const string Identificador = "dalessandro.disfigure.temporizadores";

    /// <summary>El log del plugin, para el comportamiento que vive en la escena.</summary>
    internal static ManualLogSource Registro { get; private set; } = null!;

    public override void Load()
    {
        Registro = Log;
        AddComponent<ComportamientoTemporizadores>();
        Log.LogInfo("Temporizadores cargado.");
    }
}
