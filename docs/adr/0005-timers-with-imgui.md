# ADR-0005 — HUD timers are drawn with IMGUI from `OnGUI`

- **Status:** accepted (decided by the user on 2026-11-07). With
  [ADR-0006](0006-bepinex-6-instead-of-melonloader.md), `OnGUI` belongs to an own
  `MonoBehaviour` the plugin adds to the scene; the rest still applies. Images are drawn by
  Unity UI since [ADR-0007](0007-icons-with-unity-ui.md).
- **Date:** 2026-11-07
- **Decides:** what draws the mod's timers on screen

## Context

- There were two ways. **IMGUI**: `OnGUI` is called every frame and `GUI.Label` draws text
  on top of everything. **Unity UI with TextMeshPro**: create an own `Canvas` and
  `TextMeshProUGUI`, or hook into the game HUD, which uses that technology
  (`ToggleStatusHud` with its auto-fire, auto-aim and zoom texts).
- IMGUI is available in the game build: the interop has `UnityEngine.IMGUIModule.dll`, and
  the game compiles scripts with `OnGUI` (`HS_CameraHolder`, `Hovl_DemoLasers`,
  `FPSDisplay`), so the native IMGUI code was not left out of the build.
- In IL2CPP, every type or method the mod uses and the game does not has to be in the
  build. With TextMeshPro there are more pieces (creating the `Canvas`, picking the font,
  placing it in the HUD hierarchy), and each one can run into that.

## Decision

- The timers are drawn with **IMGUI**, from the mod's `OnGUI`, with `GUI.Label`.
- Size, bold and color come from rich text (`<size>`, `<b>`, `<color>`), which IMGUI's
  default label already understands. That way the mod creates no `GUIStyle` of its own.
- The game is read in `Update`, and `OnGUI` only draws the prepared text, because `OnGUI`
  is called several times per frame.

## Consequences

**Pros:** it is the least needed to see the timers on screen, and only uses `GUI.Label`,
`Rect` and `Screen`.

**Cons:** it does not look like the game HUD: it is text on top of the image, with a shadow
so it can be read. The position is fixed and does not know where the HUD elements are: it
can cover them.

**Revisit if:** the timers should look integrated into the HUD, or IMGUI fails in the game
(does not draw, or throws errors for methods that are not in the build).

> **Note (2026-11-07): images with IMGUI crash the game.** For backlog step 5, the power
> icons were drawn with `GUI.DrawTextureWithTexCoords`, and the game crashed both times an
> icon was drawn:
>
> - with MelonLoader, at 21:48 (`d3d12: Device failed error (887a0007)` and
>   `Unrecoverable GPU device error!` in `Player-prev.log`);
> - with BepInEx, at 22:09:13, in the same second the first timer with an icon appeared.
>   Windows logged an access violation (`0xc0000005`) in `dotnet\coreclr.dll`.
>
> Text with `GUI.Label` worked for a whole run. `tools/native-methods.ps1` later confirmed
> the cause: `GUI.DrawTextureWithTexCoords` is not in the game build, and the version
> Il2CppInterop reconstructs calls the native code wrongly. **IMGUI only draws text.**
> Images go through Unity UI (an `Image` with the sprite), which the game does use;
> see ADR-0007.

## Alternatives considered

- **Unity UI with TextMeshPro:** the way to make it look like the game HUD, but it has more
  pieces that can fail in IL2CPP. The user chose to start with IMGUI.
