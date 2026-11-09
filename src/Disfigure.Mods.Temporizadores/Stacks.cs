using System.Globalization;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// El texto de los indicadores de las cartas que stackean, según cómo se vieron en el juego
/// (paso 11 del backlog). Se muestran mientras la carta está desbloqueada, también en
/// <c>+0%</c>: los valores cambian varias veces por segundo y una fila que aparece y
/// desaparece a ese ritmo no se puede leer. <c>null</c> significa que no se tiene la carta.
/// </summary>
internal static class Stacks
{
    /// <summary>Kill Spree se reinicia cuando <c>killSpreeTimer</c> llega a este valor.</summary>
    internal const float SegundosDeKillSpree = 3f;

    /// <summary>
    /// Kill Spree: <c>killSpreeBuff</c> sube 0.05 por kill hasta 1.35; <c>killSpreeTimer</c>
    /// cuenta para arriba desde la última kill y en 3 el bonus vuelve a 1.
    /// </summary>
    internal static string? KillSpree(PlayerStats stats)
    {
        if (!stats.killSpree)
        {
            return null;
        }

        var bonus = Porcentaje(stats.killSpreeBuff);
        if (bonus == 0)
        {
            return "+0%";
        }

        var falta = Math.Max(0f, SegundosDeKillSpree - stats.killSpreeTimer);
        return string.Create(CultureInfo.InvariantCulture, $"+{bonus}% · {falta:0.0}s");
    }

    /// <summary>
    /// Rapid Fire: <c>rapidFireMultiplier</c> sube 0.15 por disparo que pega, hasta 1.90, y
    /// vuelve a 1 cerca de un segundo después. No hay campo con ese tiempo.
    /// </summary>
    internal static string? RapidFire(PlayerStats stats)
    {
        return stats.rapidFire ? Bonus(stats.rapidFireMultiplier) : null;
    }

    /// <summary>
    /// Ricochet Rebound: <c>ricochetReboundMultiplier</c> sube 0.15 por kill de rebote, hasta
    /// 1.90, y se gasta con el próximo disparo.
    /// </summary>
    internal static string? RicochetRebound(PlayerStats stats)
    {
        return stats.ricochetRebound ? Bonus(stats.ricochetReboundMultiplier) : null;
    }

    /// <summary>
    /// Magnetic Rounds: <c>WeaponManager.magneticRoundMultiplier</c> sube 0.05 por EXP
    /// juntada y se gasta con el próximo disparo.
    /// </summary>
    internal static string? MagneticRounds(PlayerStats stats, WeaponManager armas)
    {
        return stats.magneticRounds ? Bonus(armas.magneticRoundMultiplier) : null;
    }

    private static string Bonus(float multiplicador)
    {
        return string.Create(CultureInfo.InvariantCulture, $"+{Porcentaje(multiplicador)}%");
    }

    private static int Porcentaje(float multiplicador)
    {
        return Math.Max(0, (int)Math.Round((multiplicador - 1f) * 100f));
    }
}
