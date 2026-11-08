using System.Diagnostics;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Los sprites de los poderes, sacados de las cartas del juego
/// (<see cref="Upgrade.authoritativeUpgradeCardIconSprite"/>) y buscados por
/// <see cref="Upgrade.upgradeName"/>, que viene en mayúsculas (<c>SHIELD BARRIER</c>).
/// El juego llena el sprite recién cuando muestra la carta.
/// </summary>
internal sealed class IconosDePoderes
{
    private static readonly TimeSpan EsperaEntreBusquedas = TimeSpan.FromSeconds(5);

    private readonly ManualLogSource registro;
    private readonly Dictionary<string, Sprite> sprites = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stopwatch reloj = Stopwatch.StartNew();
    private TimeSpan proximaBusqueda = TimeSpan.Zero;

    internal IconosDePoderes(ManualLogSource registro)
    {
        this.registro = registro;
    }

    internal Sprite? Buscar(string poder)
    {
        return sprites.TryGetValue(poder, out var sprite) && sprite != null ? sprite : null;
    }

    /// <summary>
    /// Busca las cartas de los poderes que no tienen sprite, como mucho una vez cada cinco
    /// segundos: <c>FindObjectsOfTypeAll</c> recorre todos los objetos cargados y es caro.
    /// </summary>
    internal void BuscarFaltantes(IEnumerable<string> poderes)
    {
        var faltan = poderes.Where(poder => Buscar(poder) is null).ToList();
        if (faltan.Count == 0 || reloj.Elapsed < proximaBusqueda)
        {
            return;
        }

        proximaBusqueda = reloj.Elapsed + EsperaEntreBusquedas;

        var cartas = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Upgrade>())
            .Select(objeto => objeto.TryCast<Upgrade>())
            .Where(carta => carta != null)
            .ToList();

        foreach (var poder in faltan)
        {
            var sprite = cartas
                .Where(carta => string.Equals(
                    carta!.upgradeName?.Trim(), poder, StringComparison.OrdinalIgnoreCase))
                .Select(carta => carta!.authoritativeUpgradeCardIconSprite)
                .FirstOrDefault(sprite => sprite != null);
            if (sprite == null)
            {
                continue;
            }

            sprites[poder] = sprite;
            registro.LogInfo($"Icono de {poder}: sprite '{sprite.name}'.");
        }
    }
}
