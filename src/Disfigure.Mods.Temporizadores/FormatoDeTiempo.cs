using System.Globalization;

namespace Disfigure.Mods.Temporizadores;

internal static class FormatoDeTiempo
{
    /// <summary>
    /// Segundos como <c>m:ss</c> (<c>83</c> → <c>1:23</c>). Redondea para arriba, así el HUD
    /// no muestra <c>0:00</c> mientras todavía falta algo; un valor negativo da <c>0:00</c>.
    /// </summary>
    internal static string Formatear(float segundos)
    {
        var total = (int)Math.Ceiling(Math.Max(0f, segundos));
        return string.Create(CultureInfo.InvariantCulture, $"{total / 60}:{total % 60:00}");
    }
}
