using System.Globalization;

namespace Disfigure.Mods.BuffTracker;

internal static class TimeFormat
{
    /// <summary>
    /// Seconds as <c>m:ss</c> (<c>83</c> → <c>1:23</c>). Rounds up, so the HUD never shows
    /// <c>0:00</c> while something is left; a negative value gives <c>0:00</c>.
    /// </summary>
    internal static string Format(float seconds)
    {
        var total = (int)Math.Ceiling(Math.Max(0f, seconds));
        return string.Create(CultureInfo.InvariantCulture, $"{total / 60}:{total % 60:00}");
    }
}
