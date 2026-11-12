using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine;
using UiImage = UnityEngine.UI.Image;

namespace Disfigure.Mods.BuffTracker;

/// <summary>
/// The indicator icons, drawn with Unity UI (ADR-0007): an own overlay <c>Canvas</c> with no
/// <c>CanvasScaler</c> (one unit is one pixel), and one <c>Image</c> per power, anchored to
/// the top right.
/// </summary>
internal sealed class IconOverlay
{
    private const int SortingOrder = 5000;

    private readonly ManualLogSource log;
    private readonly Dictionary<string, UiImage> images = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> visible = new(StringComparer.Ordinal);
    private GameObject? root;
    private bool disabled;

    internal IconOverlay(ManualLogSource log)
    {
        this.log = log;
    }

    /// <summary>False if creating the <c>Canvas</c> or an <c>Image</c> failed: rows show the name.</summary>
    internal bool Available => !disabled;

    /// <summary>
    /// Shows the icon of every row that has a sprite, at its position, and hides the rest.
    /// </summary>
    internal void Show(IReadOnlyList<Row> rows)
    {
        if (disabled)
        {
            return;
        }

        try
        {
            var withIcon = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                if (row.Sprite == null)
                {
                    continue;
                }

                var image = ImageFor(row.Indicator.Power);
                if (image.sprite != row.Sprite)
                {
                    image.sprite = row.Sprite;
                }

                var rect = image.rectTransform;
                rect.sizeDelta = new Vector2(row.Icon.Width, row.Icon.Height);
                rect.anchoredPosition = new Vector2(
                    -(Screen.width - row.Icon.Right),
                    -row.Icon.Y);
                SetVisible(row.Indicator.Power, image, true);
                withIcon.Add(row.Indicator.Power);
            }

            foreach (var (power, image) in images)
            {
                if (!withIcon.Contains(power))
                {
                    SetVisible(power, image, false);
                }
            }
        }
        catch (Exception error)
        {
            disabled = true;
            log.LogWarning($"Icons are disabled until the next launch: {error}");
        }
    }

    private void SetVisible(string power, UiImage image, bool show)
    {
        if (visible.TryGetValue(power, out var before) && before == show)
        {
            return;
        }

        image.gameObject.SetActive(show);
        visible[power] = show;
    }

    private UiImage ImageFor(string power)
    {
        if (images.TryGetValue(power, out var existing) && existing != null)
        {
            return existing;
        }

        var obj = new GameObject($"Icon {power}");
        obj.transform.SetParent(Root().transform, false);
        var image = obj.AddComponent(Il2CppType.Of<UiImage>()).Cast<UiImage>();
        image.raycastTarget = false;
        image.preserveAspect = true;

        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);

        images[power] = image;
        visible.Remove(power);
        log.LogInfo($"Image created for {power}.");
        return image;
    }

    private GameObject Root()
    {
        if (root != null)
        {
            return root;
        }

        root = new GameObject("Buff Tracker HUD");
        UnityEngine.Object.DontDestroyOnLoad(root);
        var canvas = root.AddComponent(Il2CppType.Of<Canvas>()).Cast<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;
        log.LogInfo("Icon canvas created.");
        return root;
    }
}
