using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Un campo candidato de una carta: en qué clase está (<see cref="PlayerStats"/> o
/// <see cref="WeaponManager"/>) y cómo se llama.
/// </summary>
internal readonly record struct Campo(bool EnArmas, string Nombre);

/// <summary>
/// Lo que hay que averiguar de una carta que stackea: qué campo dice que está desbloqueada,
/// cuál guarda el bonus o los stacks, y cuál el tiempo, si tiene.
/// </summary>
internal sealed record Sonda(string Carta, Campo? Desbloqueo, Campo Valor, Campo? Tiempo);

/// <summary>
/// Diagnóstico del paso 11 del backlog: sigue los campos candidatos de las cartas que
/// stackean y escribe una línea cada vez que cambia el desbloqueo o el valor de una, con el
/// tiempo en ese momento. Como BepInEx pisa <c>LogOutput.log</c> en cada arranque, también
/// escribe en <c>BepInEx\Temporizadores-diagnostico.log</c>, que se acumula entre partidas.
/// Se saca cuando cada carta tenga su indicador.
/// </summary>
internal sealed class DiagnosticoDeStacks
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Los candidatos salen de los nombres de los campos; el diagnóstico los confirma. Kill
    /// Spree, Rapid Fire, Ricochet Rebound y Magnetic Rounds ya se confirmaron (paso 11) y
    /// tienen indicador en <see cref="Stacks"/>.
    /// </summary>
    internal static readonly IReadOnlyList<Sonda> Sondas = new[]
    {
        Sonda("BERSERK", "berserk", "berserkMultiplier", "berserkTimer"),
        Sonda("RAMPAGE", "rampage", "rampageMultiplier", null),
        new Sonda(
            "ACCELERATING BARREL",
            new Campo(false, "acceleratingBarrel"),
            new Campo(true, "acceleratingBarrelBuff"),
            new Campo(true, "acceleratingBarrelTimer")),
        Sonda("OVERDRIVE", "overdrive", "overdriveMultiplier", "overdriveTimer"),
        Sonda("FRAGMENT SURGE", "splintersurge", "splintersurgeMultiplier", "splintersurgeTimer"),
        Sonda("IGNITION", "ignition", "ignitionFireRateBuff", "ignitionTimer"),
        new Sonda(
            "SAW BURST",
            new Campo(false, "sawBurst"),
            new Campo(true, "sawBurstBuff"),
            new Campo(true, "sawBurstTimer")),
        Sonda("RADIANT MISSILES", null, "radiantMissilesMultiplier", "radiantMissilesTimer"),
        Sonda("SHROUDED FROST", "concealedSpiral", "concealedSpiralBuff", "concealedSpiralBuffTimer"),
        Sonda("SHROUDED FROST (nivel)", "concealedSpiral", "concealedSpiralBuffLevel", null),
        Sonda("MOLTEN ROUNDS", "moltenRounds", "moltenRoundsDamageMultiplier", null),
        Sonda("ONE IN THE CHAMBER", "oneInTheChamber", "oneInTheChamberMultiplier", null),
        Sonda("PLAGUE", "plague", "plagueStackCount", null),
        new Sonda(
            "PSYCHIC FORTIFICATION",
            new Campo(false, "noxiousFortification"),
            new Campo(true, "noxiousRemnantBuff"),
            null),
    };

    private readonly ManualLogSource registro;
    private readonly string archivo = Path.Combine(Paths.BepInExRootPath, "Temporizadores-diagnostico.log");
    private readonly Dictionary<string, string> anteriores = new(StringComparer.Ordinal);
    private readonly HashSet<string> faltantes = new(StringComparer.Ordinal);
    private readonly Stopwatch reloj = Stopwatch.StartNew();
    private TimeSpan proximaLectura = TimeSpan.Zero;
    private bool encabezadoEscrito;

    internal DiagnosticoDeStacks(ManualLogSource registro)
    {
        this.registro = registro;
    }

    internal void Actualizar(PlayerStats stats, WeaponManager armas)
    {
        if (reloj.Elapsed < proximaLectura)
        {
            return;
        }

        proximaLectura = reloj.Elapsed + Intervalo;
        if (!encabezadoEscrito)
        {
            Escribir($"--- arranque del juego, Temporizadores {ModTemporizadores.Version}");
            encabezadoEscrito = true;
        }

        foreach (var sonda in Sondas)
        {
            var desbloqueo = sonda.Desbloqueo is { } d ? Leer(d, stats, armas) : "?";
            var valor = Leer(sonda.Valor, stats, armas);
            var clave = $"{desbloqueo}|{valor}";
            if (anteriores.TryGetValue(sonda.Carta, out var anterior) && anterior == clave)
            {
                continue;
            }

            anteriores[sonda.Carta] = clave;
            var tiempo = sonda.Tiempo is { } t ? $" tiempo={Leer(t, stats, armas)}" : string.Empty;
            Escribir($"stack | {sonda.Carta} | desbloqueada={desbloqueo} valor={valor}{tiempo}");
        }
    }

    private static Sonda Sonda(string carta, string? desbloqueo, string valor, string? tiempo)
    {
        return new Sonda(
            carta,
            desbloqueo is null ? null : new Campo(false, desbloqueo),
            new Campo(false, valor),
            tiempo is null ? null : new Campo(false, tiempo));
    }

    private string Leer(Campo campo, PlayerStats stats, WeaponManager armas)
    {
        object objeto = campo.EnArmas ? armas : stats;
        var propiedad = objeto.GetType().GetProperty(campo.Nombre, BindingFlags.Public | BindingFlags.Instance);
        if (propiedad is null)
        {
            if (faltantes.Add(campo.Nombre))
            {
                registro.LogWarning($"stack | no existe el campo {campo.Nombre}");
            }

            return "falta";
        }

        return propiedad.GetValue(objeto) switch
        {
            float f => f.ToString("F2", CultureInfo.InvariantCulture),
            double x => x.ToString("F2", CultureInfo.InvariantCulture),
            IFormattable otro => otro.ToString(null, CultureInfo.InvariantCulture),
            var otro => otro?.ToString() ?? "null",
        };
    }

    private void Escribir(string linea)
    {
        registro.LogInfo(linea);
        try
        {
            var hora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture);
            File.AppendAllText(archivo, $"[{hora}] {linea}{Environment.NewLine}");
        }
        catch (IOException error)
        {
            registro.LogWarning($"No pude escribir {archivo}: {error.Message}");
        }
    }
}
