# ADR-0007 — Icons are drawn with a Unity UI `Image`; text stays on IMGUI

- **Status:** accepted (the user chose icons with Unity UI on 2026-11-07)
- **Date:** 2026-11-07
- **Decides:** what draws the power icons next to each indicator
- **Supersedes:** the image part of [ADR-0005](0005-timers-with-imgui.md). Text stays on
  IMGUI.

## Context

- With IMGUI, `GUI.DrawTextureWithTexCoords` crashed the game both times it drew an icon
  (note in ADR-0005). `GUI.Label` with text worked fine.
- The game builds its interface with Unity UI (`UnityEngine.UI`, referenced by
  `Assembly-CSharp`), so the native `Canvas` and `Image` code is in the build and used all
  the time. An `Image` handles sprites that come from an atlas on its own.
- Each power's sprite is `Upgrade.authoritativeUpgradeCardIconSprite`, which the game only
  fills once it shows the card.
- **Why IMGUI crashed.** In the interop, methods that are in the game build have a
  `NativeMethodInfoPtr_*` field; those IL2CPP stripped and Il2CppInterop rebuilt from
  Unity's code do not. `tools/native-methods.ps1` checks it: `GUI.DrawTextureWithTexCoords`
  has **0 pointers** (reconstructed), while `GUI.Label(Rect, String)` is native. Every method
  this ADR uses is native: the constructors of `GameObject(String)`, `Vector2` and `Rect`,
  `GameObject.AddComponent(Type)`, `SetActive`, `Transform.SetParent`,
  `Object.DontDestroyOnLoad`, the setters of `RectTransform` (`anchorMin`, `anchorMax`,
  `pivot`, `sizeDelta`, `anchoredPosition`), of `Canvas` (`renderMode`, `sortingOrder`), of
  `Image` (`sprite`, `preserveAspect`) and of `Graphic` (`raycastTarget`, `rectTransform`).

## Decision

- The mod creates **its own `Canvas`** in `ScreenSpaceOverlay` mode, with a high
  `sortingOrder` and no `CanvasScaler`, so one `RectTransform` unit is one screen pixel. The
  object is marked `DontDestroyOnLoad` and has no `GraphicRaycaster`, so it does not catch
  clicks.
- **One `Image` per power**, anchored to the top right, with `preserveAspect` and no
  `raycastTarget`. It turns on when its indicator shows and off when it does not.
- **Text stays on IMGUI** (`GUI.Label`), which is already proven. Row positions come from a
  single calculation both parts use, so icon and value stay aligned.
- If a power has no sprite, its row shows the name, as before. If creating the `Canvas` or
  an `Image` throws, icons are turned off until the next launch and the name stays.

## Consequences

**Pros:** it uses the same technology the game draws its HUD with, instead of an IMGUI
method the game does not use.

**Cons:** drawing is split across two systems (Unity UI for icons, IMGUI for text). An
exception can be caught, a native crash cannot: if the `Image` also crashes, it has to go.

**Revisit if:** the `Image` crashes or does not show, or the text should also move to Unity
UI to look like the rest of the HUD.

## Alternatives considered

- **Moving the text to Unity UI too:** needs a font (copied from a game `Text` or a built-in
  one), and that is more new pieces at once. It can be done later, once the `Image` is
  proven.
- **Other IMGUI methods for images (`GUI.DrawTexture`, `GUI.Label` with a texture):** same
  risk as `DrawTextureWithTexCoords`, and a `GUI.Label` with a texture draws the whole atlas,
  not the sprite.
