using UnityEngine;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// El <c>MonoBehaviour</c> que el plugin agrega a la escena (ADR-0006). Sólo pasa
/// <c>Update</c> y <c>OnGUI</c> al HUD y al panel de opciones:
/// Il2CppInterop registra en IL2CPP cada método de esta clase, así que cuantos menos tenga,
/// mejor.
/// </summary>
public sealed class ComportamientoTemporizadores : MonoBehaviour
{
    private readonly HudDeTemporizadores hud =
        new(ModTemporizadores.Registro, ModTemporizadores.Opciones);

    private readonly PanelDeOpciones panel = new(
        ModTemporizadores.Opciones,
        HudDeTemporizadores.Poderes,
        ModTemporizadores.Registro);

    /// <summary>Lo usa Il2CppInterop para crear la parte administrada del objeto.</summary>
    public ComportamientoTemporizadores(IntPtr puntero)
        : base(puntero)
    {
    }

    public void Update()
    {
        hud.Actualizar();
        panel.Actualizar();
    }

    public void OnGUI()
    {
        hud.Dibujar();
        panel.Dibujar();
    }
}
