namespace Disfigure.Mods.BuffTracker;

/// <summary>Whether an indicator appeared on or disappeared from the HUD this frame.</summary>
internal enum VisibilityChange
{
    None,
    Appeared,
    Disappeared,
}

/// <summary>
/// One HUD line: the power and what is shown next to its icon (a time like <c>2:59</c>, a
/// progress like <c>37/100</c> or a bonus like <c>+35%</c>). Updated in <c>Update</c> and
/// read in <c>OnGUI</c>.
/// </summary>
internal sealed class Indicator
{
    internal Indicator(string power)
    {
        Power = power;
    }

    /// <summary>The power's name in the game, as on its card.</summary>
    internal string Power { get; }

    /// <summary>What is shown, or <c>null</c> when the indicator is hidden.</summary>
    internal string? Text { get; private set; }

    internal VisibilityChange Update(string? text)
    {
        var before = Text is not null;
        Text = text;
        var now = Text is not null;

        if (before == now)
        {
            return VisibilityChange.None;
        }

        return now ? VisibilityChange.Appeared : VisibilityChange.Disappeared;
    }
}
