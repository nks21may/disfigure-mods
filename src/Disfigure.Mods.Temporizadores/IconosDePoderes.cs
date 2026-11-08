using System.Diagnostics;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

namespace Disfigure.Mods.Temporizadores;

/// <summary>Un icono listo para IMGUI: la textura y la parte de ella que ocupa el sprite.</summary>
internal sealed record Icono(Texture2D Textura, Rect Coordenadas, float Proporcion);

/// <summary>
/// Los iconos de los poderes, sacados de las cartas del juego
/// (<see cref="Upgrade.authoritativeUpgradeCardIconSprite"/>), buscados por
/// <see cref="Upgrade.upgradeName"/>. Si no encuentra uno, el HUD muestra el nombre.
/// </summary>
internal sealed class IconosDePoderes
{
    private static readonly TimeSpan EsperaEntreBusquedas = TimeSpan.FromSeconds(5);

    private readonly Dictionary<string, Icono> iconos = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stopwatch reloj = Stopwatch.StartNew();
    private TimeSpan proximaBusqueda = TimeSpan.Zero;
    private bool cartasRegistradas;

    internal Icono? Buscar(string poder)
    {
        return iconos.TryGetValue(poder, out var icono) && icono.Textura != null ? icono : null;
    }

    /// <summary>
    /// Busca las cartas de los poderes que faltan, como mucho una vez cada cinco segundos:
    /// <c>FindObjectsOfTypeAll</c> recorre todos los objetos cargados y es caro.
    /// </summary>
    internal void Actualizar(IReadOnlyList<string> poderes, MelonLogger.Instance log)
    {
        if (poderes.All(poder => Buscar(poder) is not null) || reloj.Elapsed < proximaBusqueda)
        {
            return;
        }

        proximaBusqueda = reloj.Elapsed + EsperaEntreBusquedas;

        var cartas = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Upgrade>())
            .Select(objeto => objeto.TryCast<Upgrade>())
            .Where(carta => carta != null)
            .ToList();

        if (!cartasRegistradas && cartas.Count > 0)
        {
            // Una vez: los nombres reales de las cartas, por si alguno no coincide.
            var nombres = cartas
                .Select(carta => carta!.upgradeName)
                .Distinct()
                .OrderBy(nombre => nombre, StringComparer.Ordinal);
            log.Msg($"Cartas de poder cargadas ({cartas.Count}): {string.Join(", ", nombres)}");
            cartasRegistradas = true;
        }

        foreach (var poder in poderes.Where(poder => Buscar(poder) is null))
        {
            var carta = cartas.FirstOrDefault(carta => string.Equals(
                carta!.upgradeName?.Trim(), poder, StringComparison.OrdinalIgnoreCase));
            var sprite = carta?.authoritativeUpgradeCardIconSprite;
            if (sprite == null)
            {
                continue;
            }

            if (ArmarIcono(sprite) is { } icono)
            {
                iconos[poder] = icono;
                log.Msg($"Icono de {poder}: sprite '{sprite.name}'.");
            }
        }
    }

    private static Icono? ArmarIcono(Sprite sprite)
    {
        try
        {
            var textura = sprite.texture;
            var r = sprite.textureRect;
            var coordenadas = new Rect(
                r.x / textura.width,
                r.y / textura.height,
                r.width / textura.width,
                r.height / textura.height);
            return new Icono(textura, coordenadas, r.width / r.height);
        }
        catch (Exception error)
        {
            // textureRect falla si el sprite está empaquetado sin forma de rectángulo en un
            // atlas. Ahí el HUD sigue con el nombre.
            MelonLogger.Warning($"No pude usar el sprite '{sprite.name}': {error.Message}");
            return null;
        }
    }
}
