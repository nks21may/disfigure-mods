# ADR-0009 — Everything in English, and the mod is published as Buff Tracker

- **Status:** accepted (the user asked for the mod to be prepared for publishing "completely
  in English" on 2026-11-11; the name Buff Tracker was chosen when preparing it)
- **Date:** 2026-11-12
- **Decides:** the language of the repo and of the mod, and the public name of the mod

## Context

- The user wants the mod published so anyone can use it, through
  [Disfigure-Mod-Manager](https://github.com/Purpledudr/Disfigure-Mod-Manager) or by hand.
  The game's modding community is international: the community translation mod ships nine
  languages.
- Until 1.0.0 the repo was in Spanish: code identifiers, comments, docs, the panel, the
  `.cfg` descriptions and the log lines. The mod was called `Temporizadores`, with plugin GUID
  `dalessandro.disfigure.temporizadores` and DLL `Disfigure.Mods.Temporizadores.dll`.
- The mod manager matches installed plugins by DLL file name and reads the version from the
  `[BepInPlugin]` attribute. The `.cfg` file name comes from the plugin GUID.
- The mod no longer only shows timers: it also shows the Shield Flare kill count and the
  bonus of stacking buffs.

## Decision

- **Everything is in English:** code identifiers, comments, the HUD, the F7 panel, the `.cfg`,
  the log lines, the README, the changelog, the ADRs, the backlog and `CLAUDE.md`. Answers to
  the user in conversation stay in Spanish.
- **The mod is Buff Tracker**: project `src/Disfigure.Mods.BuffTracker`, DLL
  `Disfigure.Mods.BuffTracker.dll`, plugin GUID `dalessandro.disfigure.bufftracker`, `.cfg`
  `BepInEx\config\dalessandro.disfigure.bufftracker.cfg`. First public version: **1.0.0**.
- Other renames: `Juego.props` is `Game.props` (`GamePath`, environment variable
  `DISFIGURE_GAME_PATH`, `-p:Deploy=true`), and `herramientas/` is `tools/`.
- The stack diagnostics stay in the code but are **off by default** (`[Debug]
  StackDiagnostics` in the `.cfg`): players do not get an extra log file.
- `tools/package.ps1` builds the release zip with the layout the mod manager requires
  (`BepInEx/plugins/Disfigure.Mods.BuffTracker/Disfigure.Mods.BuffTracker.dll`). The repo is
  MIT licensed.

## Consequences

**Pros:** anyone can read the code, the docs and the mod's texts, and the name describes
what the mod does.

**Cons:** the history before this ADR is in Spanish (commit messages included). Anyone who
had `Temporizadores` installed has to remove `BepInEx\plugins\Disfigure.Mods.Temporizadores\`
or both load; their old `.cfg` is not migrated.

**Revisit if:** the mod gets translations of its own texts (then the panel and the `.cfg`
descriptions would need a language setting).

## Alternatives considered

- **English only for what the player sees, code and docs in Spanish:** the user asked for
  everything in English.
- **Keeping the name Temporizadores:** it is Spanish and only describes part of what the mod
  shows.
- **Hidden Timers or Combat HUD:** the first only covers the shield timers; the second is too
  generic to find in a catalog.
