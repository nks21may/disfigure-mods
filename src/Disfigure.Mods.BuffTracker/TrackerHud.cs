using System.Globalization;
using BepInEx.Logging;
using UnityEngine;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// A rectangle in screen pixels, with the origin at the top left. It is our own type and not
/// <c>UnityEngine.Rect</c>, so the math does not go through Unity methods.
/// </summary>
internal readonly record struct ScreenBox(float X, float Y, float Width, float Height)
{
    internal float Right => X + Width;
}

/// <summary>
/// One HUD row this frame: the indicator, its sprite if there is one, and where the icon and
/// the text go.
/// </summary>
internal sealed record Row(Indicator Indicator, Sprite? Sprite, ScreenBox Icon, ScreenBox Text);

/// <summary>
/// The indicators at the top right, under the score: how long until the Shield Barrier and
/// the Projectile Barrier come back, the Shield Flare kill count, and the bonus of the
/// stacking buffs. It reads the game, builds the rows once per frame, and both the icon
/// (Unity UI, ADR-0007) and the text (IMGUI, ADR-0005) come from them. It is a plain class
/// and not a <c>MonoBehaviour</c>, so Il2CppInterop does not try to register its methods in
/// IL2CPP.
/// </summary>
internal sealed class TrackerHud
{
    internal const string ShieldBarrier = "Shield Barrier";
    internal const string ProjectileBarrier = "Projectile Barrier";
    internal const string ShieldFlare = "Shield Flare";
    internal const string KillSpree = "Kill Spree";
    internal const string RapidFire = "Rapid Fire";
    internal const string RicochetRebound = "Ricochet Rebound";
    internal const string MagneticRounds = "Magnetic Rounds";

    /// <summary>The powers with an indicator, in HUD, panel and <c>.cfg</c> order.</summary>
    internal static readonly IReadOnlyList<string> Powers = new[]
    {
        ShieldBarrier, ProjectileBarrier, ShieldFlare,
        KillSpree, RapidFire, RicochetRebound, MagneticRounds,
    };

    // Sizes designed for a 1080-pixel-high screen; they scale with the resolution.
    private const float ReferenceHeight = 1080f;
    private const float RelativeTop = 0.40f;
    private const float Margin = 24f;
    private const float IconSize = 40f;
    private const float Gap = 8f;
    private const float ValueWidth = 170f;
    private const float WidthWithName = 360f;
    private const float LineHeight = 48f;
    private const float FontSize = 26f;

    private readonly ManualLogSource log;
    private readonly Settings settings;
    private readonly Indicator barrier = new(ShieldBarrier);
    private readonly Indicator projectileBarrier = new(ProjectileBarrier);
    private readonly Indicator flare = new(ShieldFlare);
    private readonly Indicator killSpree = new(KillSpree);
    private readonly Indicator rapidFire = new(RapidFire);
    private readonly Indicator ricochetRebound = new(RicochetRebound);
    private readonly Indicator magneticRounds = new(MagneticRounds);
    private readonly Indicator[] indicators;
    private readonly PowerSprites sprites;
    private readonly IconOverlay icons;
    private readonly StackDiagnostics diagnostics;
    private List<Row> rows = new();

    internal TrackerHud(ManualLogSource log, Settings settings)
    {
        this.log = log;
        this.settings = settings;
        diagnostics = new StackDiagnostics(log);
        indicators = new[]
        {
            barrier, projectileBarrier, flare, killSpree, rapidFire, ricochetRebound, magneticRounds,
        };
        sprites = new PowerSprites(log);
        icons = new IconOverlay(log);
    }

    internal void Update()
    {
        var refs = GameRefs.Instance;
        var stats = refs == null ? null : refs.playerStats;
        var weapons = refs == null ? null : refs.weaponManager;
        var inRun = stats != null && weapons != null;

        if (inRun && settings.StackDiagnostics)
        {
            diagnostics.Update(stats!, weapons!);
        }

        var barrierLeft = inRun ? Shields.BarrierRemaining(stats!, weapons!) : null;
        Set(barrier, barrierLeft is { } bl ? TimeFormat.Format(bl) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"timer={stats!.shieldBarrierTimer:F2} "
                + $"cooldown={stats.shieldBarrierCooldown:F2} "
                + $"active={weapons!.shieldBarrierActive}"));

        var projectileLeft = inRun ? Shields.ProjectileBarrierRemaining(stats!, weapons!) : null;
        Set(projectileBarrier, projectileLeft is { } pl ? TimeFormat.Format(pl) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"timer={weapons!.projectileShieldRechargeTimer:F2} "
                + $"duration={weapons.ProjectileShieldRechargeDuration:F2} "
                + $"active={weapons.projectileShieldIsActive}"));

        var flareKills = inRun ? Shields.ShieldFlareKills(stats!, weapons!) : null;
        Set(
            flare,
            flareKills is { } fk
                ? string.Create(CultureInfo.InvariantCulture, $"{fk}/{Shields.ShieldFlareKillsNeeded}")
                : null,
            () => string.Create(
                CultureInfo.InvariantCulture,
                $"kills={weapons!.shieldActiveKills} unlocked={stats!.shieldFlare} "
                + $"active={weapons.shieldFlareActive} barrier={weapons.shieldBarrierActive}"));

        Set(killSpree, inRun ? StackBuffs.KillSpree(stats!) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"buff={stats!.killSpreeBuff:F2} timer={stats.killSpreeTimer:F2}"));
        Set(rapidFire, inRun ? StackBuffs.RapidFire(stats!) : null, () =>
            string.Create(CultureInfo.InvariantCulture, $"multiplier={stats!.rapidFireMultiplier:F2}"));
        Set(ricochetRebound, inRun ? StackBuffs.RicochetRebound(stats!) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"multiplier={stats!.ricochetReboundMultiplier:F2}"));
        Set(magneticRounds, inRun ? StackBuffs.MagneticRounds(stats!, weapons!) : null, () =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"multiplier={weapons!.magneticRoundMultiplier:F2}"));

        var onScreen = indicators.Where(indicator => indicator.Text is not null).ToList();
        if (icons.Available)
        {
            // Only the sprites of the indicators on screen are looked up.
            sprites.FindMissing(onScreen.Select(indicator => indicator.Power));
        }

        rows = BuildRows(onScreen);
        icons.Show(rows);

        void Set(Indicator indicator, string? text, Func<string> values)
        {
            // An indicator turned off in the .cfg or in the panel is not shown (ADR-0008).
            var change = indicator.Update(settings.IsOn(indicator.Power) ? text : null);
            if (change != VisibilityChange.None)
            {
                Report(indicator, change, inRun ? values() : "not in a run");
            }
        }
    }

    internal void Draw()
    {
        foreach (var row in rows)
        {
            var text = row.Sprite == null
                ? $"{row.Indicator.Power}  {row.Indicator.Text}"
                : row.Indicator.Text!;
            DrawText(text, row.Text);
        }
    }

    private List<Row> BuildRows(IEnumerable<Indicator> onScreen)
    {
        var scale = Screen.height / ReferenceHeight;
        var y = Screen.height * RelativeTop;
        var result = new List<Row>();

        foreach (var indicator in onScreen)
        {
            var sprite = icons.Available ? sprites.Find(indicator.Power) : null;
            var rowHeight = LineHeight * scale;
            var side = IconSize * scale;
            var valueX = Screen.width - (Margin + ValueWidth) * scale;
            var icon = new ScreenBox(valueX - Gap * scale - side, y, side, side);
            var text = sprite == null
                ? new ScreenBox(
                    Screen.width - (Margin + WidthWithName) * scale,
                    y,
                    WidthWithName * scale,
                    rowHeight)
                : new ScreenBox(valueX, y, ValueWidth * scale, rowHeight);

            result.Add(new Row(indicator, sprite, icon, text));
            y += rowHeight;
        }

        return result;
    }

    /// <summary>
    /// Writes to <c>BepInEx\LogOutput.log</c> when each indicator appears and disappears, with
    /// the game values at that moment, so the value each one leaves at can be checked.
    /// </summary>
    private void Report(Indicator indicator, VisibilityChange change, string values)
    {
        var what = change == VisibilityChange.Appeared ? $"appears at {indicator.Text}" : "disappears";
        log.LogInfo($"{indicator.Power}: {what} ({values})");
    }

    /// <summary>
    /// Shadowed text, centered on the icon height. It only uses <c>GUI.Label</c>:
    /// <c>GUI.DrawTextureWithTexCoords</c> crashed the game (see ADR-0005).
    /// </summary>
    private static void DrawText(string text, ScreenBox where)
    {
        var scale = Screen.height / ReferenceHeight;
        var size = Math.Max(1, (int)Math.Round(FontSize * scale));
        var y = where.Y + (IconSize * scale - size * 1.3f) / 2f;
        var sizeText = size.ToString(CultureInfo.InvariantCulture);

        GUI.Label(
            new Rect(where.X + 2f, y + 2f, where.Width, where.Height),
            $"<size={sizeText}><b><color=#000000>{text}</color></b></size>");
        GUI.Label(
            new Rect(where.X, y, where.Width, where.Height),
            $"<size={sizeText}><b><color=#FFFFFF>{text}</color></b></size>");
    }
}
