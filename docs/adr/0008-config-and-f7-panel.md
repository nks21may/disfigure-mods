# ADR-0008 — Each indicator is turned on and off from the BepInEx `.cfg` and from an F7 panel

- **Status:** accepted (the user chose the config file and the in-game panel on 2026-11-08)
- **Date:** 2026-11-08
- **Decides:** where each HUD indicator is turned on and off, and how that choice is saved

## Context

- The user wants to add indicators for other stacking skills and to be able to turn each
  one on and off.
- Disfigure-Mod-Manager does not help with that: according to its README and files, it
  installs, updates, enables and disables whole plugins, and has no screen for each plugin's
  options.
- BepInEx ships `ConfigFile` (`BasePlugin.Config`): one `.cfg` file per plugin in
  `BepInEx\config\`, editable with a text editor and saved automatically when a value
  changes. It is BepInEx code, with no Unity methods.
- For an in-game panel, `tools/native-methods.ps1` says `GUI.Box(Rect, String)`,
  `GUI.Button(Rect, String)` and `GUI.Window` are in the build. `GUI.Toggle` and all of
  `GUILayout`, on the other hand, are **reconstructed**, like `GUI.DrawTextureWithTexCoords`
  was when it crashed (ADR-0005).
- The game reads the keyboard with the Input System. `Keyboard.current`, `Keyboard[Key]`,
  the `f7Key` and `digit1Key`…`digit9Key` keys and `ButtonControl.wasPressedThisFrame` are
  native.
- During a run the game confines the cursor and clicking shoots.

## Decision

- **Each indicator has a `bool` entry in the `[Indicators]` section** of the plugin's `.cfg`
  file in `BepInEx\config\`, on by default. An indicator that is off is not shown and does
  not look up its icon.
- **F7 opens and closes a panel** drawn with `GUI.Box` and one `GUI.Button` per indicator
  (`[x] Shield Barrier`). With the panel open, keys `1` to `9` toggle the indicator with that
  number, so the mouse is not needed. Every change is written to the same `.cfg`, so it is
  saved.
- **Opening the panel reloads the `.cfg`**, so a change made with a text editor shows up
  without restarting the game.
- The panel uses neither `GUI.Toggle` nor `GUILayout`.

## Consequences

**Pros:** each new indicator is one line in the `.cfg` and one in the panel. The `.cfg` works
even if the panel fails, and it has the standard BepInEx format.

**Cons:** the panel is basic IMGUI: it does not look like the game menus. F7 is taken; if
the game or another mod uses it, it has to change in code.

**Revisit if:** the key needs to change (move it to the `.cfg`), or the panel has so many
options that it needs something more structured than a list of buttons.

## Alternatives considered

- **Only the `.cfg`:** enough, but changing something means leaving the game, or editing the
  file with the game open and waiting for the next launch.
- **A third-party Configuration Manager for IL2CPP:** would give a menu for every plugin,
  but nobody tested it with Unity 6000.3 and it uses many Unity APIs that may be
  reconstructed in this build.
- **A Unity UI panel:** would look better, but it is many more pieces (buttons, events, font)
  that are not needed today.
