using BepInEx.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// The panel that F7 opens (ADR-0008): one button per indicator to turn it on or off, and the
/// keys 1 to 9 to do the same without the mouse. It only uses <c>GUI.Box(Rect, String)</c> and
/// <c>GUI.Button(Rect, String)</c>, which are native; <c>GUI.Toggle</c> and <c>GUILayout</c>
/// are reconstructed in this build.
/// </summary>
internal sealed class OptionsPanel
{
    private const float ReferenceHeight = 1080f;
    private const float Width = 420f;
    private const float TitleHeight = 44f;
    private const float RowHeight = 40f;
    private const float Padding = 12f;
    private const float RelativeTop = 0.20f;
    private const float FontSize = 20f;

    private static readonly Key[] NumberKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
        Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9,
    };

    private readonly Settings settings;
    private readonly IReadOnlyList<string> powers;
    private readonly ManualLogSource log;
    private bool visible;

    internal OptionsPanel(Settings settings, IReadOnlyList<string> powers, ManualLogSource log)
    {
        this.settings = settings;
        this.powers = powers;
        this.log = log;
    }

    /// <summary>Reads the keyboard: F7 opens and closes; with the panel open, 1 to 9 toggle.</summary>
    internal void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.f7Key.wasPressedThisFrame)
        {
            visible = !visible;
            if (visible)
            {
                settings.Reload();
            }

            log.LogInfo(visible ? "Panel opened." : "Panel closed.");
        }

        if (!visible)
        {
            return;
        }

        for (var i = 0; i < powers.Count && i < NumberKeys.Length; i++)
        {
            if (keyboard[NumberKeys[i]].wasPressedThisFrame)
            {
                Toggle(powers[i]);
            }
        }
    }

    internal void Draw()
    {
        if (!visible)
        {
            return;
        }

        var scale = Screen.height / ReferenceHeight;
        var width = Width * scale;
        var padding = Padding * scale;
        var rowHeight = RowHeight * scale;
        var titleHeight = TitleHeight * scale;
        var x = (Screen.width - width) / 2f;
        var y = Screen.height * RelativeTop;
        var height = titleHeight + powers.Count * rowHeight + padding;

        // The font size is changed only while the panel is drawn, and then restored.
        var box = GUI.skin.box;
        var button = GUI.skin.button;
        var boxFont = box.fontSize;
        var buttonFont = button.fontSize;
        var size = Math.Max(1, (int)Math.Round(FontSize * scale));
        box.fontSize = size;
        button.fontSize = size;

        GUI.Box(new Rect(x, y, width, height), $"{BuffTrackerPlugin.Name}  ·  F7 to close");
        for (var i = 0; i < powers.Count; i++)
        {
            var power = powers[i];
            var mark = settings.IsOn(power) ? "x" : "  ";
            var row = new Rect(
                x + padding,
                y + titleHeight + i * rowHeight,
                width - 2f * padding,
                rowHeight - 6f * scale);
            if (GUI.Button(row, $"{i + 1}.   [{mark}]   {power}"))
            {
                Toggle(power);
            }
        }

        box.fontSize = boxFont;
        button.fontSize = buttonFont;
    }

    private void Toggle(string power)
    {
        settings.Toggle(power);
        var state = settings.IsOn(power) ? "on" : "off";
        log.LogInfo($"{power}: {state}.");
    }
}
