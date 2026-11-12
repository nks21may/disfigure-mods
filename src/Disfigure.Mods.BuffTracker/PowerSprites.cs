using System.Diagnostics;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// The power sprites, taken from the game's cards
/// (<see cref="Upgrade.authoritativeUpgradeCardIconSprite"/>) and looked up by
/// <see cref="Upgrade.upgradeName"/>, which is upper case (<c>SHIELD BARRIER</c>). The game
/// only fills the sprite once it shows the card.
/// </summary>
internal sealed class PowerSprites
{
    private static readonly TimeSpan SearchInterval = TimeSpan.FromSeconds(5);

    private readonly ManualLogSource log;
    private readonly Dictionary<string, Sprite> sprites = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private TimeSpan nextSearch = TimeSpan.Zero;

    internal PowerSprites(ManualLogSource log)
    {
        this.log = log;
    }

    internal Sprite? Find(string power)
    {
        return sprites.TryGetValue(power, out var sprite) && sprite != null ? sprite : null;
    }

    /// <summary>
    /// Looks for the cards of the powers that have no sprite yet, at most once every five
    /// seconds: <c>FindObjectsOfTypeAll</c> walks every loaded object and is expensive.
    /// </summary>
    internal void FindMissing(IEnumerable<string> powers)
    {
        var missing = powers.Where(power => Find(power) is null).ToList();
        if (missing.Count == 0 || clock.Elapsed < nextSearch)
        {
            return;
        }

        nextSearch = clock.Elapsed + SearchInterval;

        var cards = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Upgrade>())
            .Select(obj => obj.TryCast<Upgrade>())
            .Where(card => card != null)
            .ToList();

        foreach (var power in missing)
        {
            var sprite = cards
                .Where(card => string.Equals(
                    card!.upgradeName?.Trim(), power, StringComparison.OrdinalIgnoreCase))
                .Select(card => card!.authoritativeUpgradeCardIconSprite)
                .FirstOrDefault(sprite => sprite != null);
            if (sprite == null)
            {
                continue;
            }

            sprites[power] = sprite;
            log.LogInfo($"Icon for {power}: sprite '{sprite.name}'.");
        }
    }
}
