using UnityEngine;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// The <c>MonoBehaviour</c> the plugin adds to the scene (ADR-0006). It only forwards
/// <c>Update</c> and <c>OnGUI</c> to the HUD and the options panel: Il2CppInterop registers
/// every method of this class in IL2CPP, so the fewer it has, the better.
/// </summary>
public sealed class BuffTrackerBehaviour : MonoBehaviour
{
    private readonly TrackerHud hud = new(BuffTrackerPlugin.SharedLog, BuffTrackerPlugin.Options);

    private readonly OptionsPanel panel = new(
        BuffTrackerPlugin.Options,
        TrackerHud.Powers,
        BuffTrackerPlugin.SharedLog);

    /// <summary>Used by Il2CppInterop to create the managed side of the object.</summary>
    public BuffTrackerBehaviour(IntPtr handle)
        : base(handle)
    {
    }

    public void Update()
    {
        hud.Update();
        panel.Update();
    }

    public void OnGUI()
    {
        hud.Draw();
        panel.Draw();
    }
}
