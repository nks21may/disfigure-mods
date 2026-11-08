using BepInEx.Configuration;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Qué indicadores se muestran (ADR-0008): una entrada <c>bool</c> por indicador en la
/// sección <c>[Indicadores]</c> de <c>BepInEx\config\dalessandro.disfigure.temporizadores.cfg</c>,
/// prendida por defecto. BepInEx guarda el archivo cada vez que cambia un valor.
/// </summary>
internal sealed class Configuracion
{
    private const string Seccion = "Indicadores";

    private readonly ConfigFile archivo;
    private readonly Dictionary<string, ConfigEntry<bool>> entradas = new(StringComparer.Ordinal);

    internal Configuracion(ConfigFile archivo, IEnumerable<string> poderes)
    {
        this.archivo = archivo;
        foreach (var poder in poderes)
        {
            // La clave va sin espacios: "Shield Barrier" queda "ShieldBarrier".
            var clave = poder.Replace(" ", string.Empty, StringComparison.Ordinal);
            entradas[poder] = archivo.Bind(
                Seccion,
                clave,
                true,
                $"Muestra el indicador de {poder} en el HUD.");
        }
    }

    internal bool EstaPrendido(string poder)
    {
        return !entradas.TryGetValue(poder, out var entrada) || entrada.Value;
    }

    internal void Alternar(string poder)
    {
        if (entradas.TryGetValue(poder, out var entrada))
        {
            entrada.Value = !entrada.Value;
        }
    }

    /// <summary>Vuelve a leer el archivo, por si se editó a mano con el juego abierto.</summary>
    internal void Releer()
    {
        archivo.Reload();
    }
}
