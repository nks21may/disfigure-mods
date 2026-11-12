# Disfigure mods ADRs

Architecture Decision Records. One file per decision, numbered, append-only: a decision that
changes is **not edited**; a new one supersedes it and the old one moves to `superseded by`.
The history is the point.

Template: [0000-template.md](0000-template.md).

| # | Decision | Status |
|---|---|---|
| [0001](0001-melonloader-net6-and-host-gate.md) | MelonLoader 0.7.3, `net6.0` for what the game loads, and a host gate with lint in the build | accepted; gate superseded by 0004, MelonLoader by 0006 |
| [0002](0002-pure-logic-separate-from-the-game.md) | Logic goes in a core project with no game references, and it is the only thing with unit tests | superseded by 0004 |
| [0003](0003-one-project-per-mod.md) | One project per mod, shared code deployed alongside, and game assemblies never committed | accepted |
| [0004](0004-no-gate-test-in-the-game.md) | No gate and no tests: a step is done when it builds and was seen working in the game | accepted |
| [0005](0005-timers-with-imgui.md) | HUD timers are drawn with IMGUI from `OnGUI` | accepted; images moved to 0007 |
| [0006](0006-bepinex-6-instead-of-melonloader.md) | BepInEx 6 (be.785) instead of MelonLoader | accepted |
| [0007](0007-icons-with-unity-ui.md) | Icons are drawn with a Unity UI `Image`; text stays on IMGUI | accepted |
| [0008](0008-config-and-f7-panel.md) | Each indicator is turned on and off from the BepInEx `.cfg` and from an F7 panel | accepted |
| [0009](0009-english-and-buff-tracker-name.md) | Everything in English, and the mod is published as Buff Tracker | accepted |

## When to write one

When the decision is expensive to reverse: a piece of infrastructure, a boundary between
components, a format that is persisted, a security stance.

It is **not** needed to pick a utility library, name a module or arrange a folder layout.

If a backlog step runs into a decision of that size, the ADR is written **before** the code
that relies on it.
