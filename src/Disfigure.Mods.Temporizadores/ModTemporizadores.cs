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
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Temporizadores cargado.");
    }
}
