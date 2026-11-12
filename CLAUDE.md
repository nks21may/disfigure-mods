# Notes for working in this repo

## What it is

Mods for **Disfigure** (ColdBrew Entertainment), a top-down roguelike shooter made in Unity
6000.3.16f1 with IL2CPP, which has no official mod support. The mods are **BepInEx
6.0.0-be.785** plugins (Unity IL2CPP, win-x64), the same build the community uses through
[Disfigure-Mod-Manager](https://github.com/Purpledudr/Disfigure-Mod-Manager). BepInEx
generates the game's interop assemblies in the local install
(`J:\SteamLibrary\steamapps\common\Disfigure\BepInEx\interop`).

There is one mod so far, **Buff Tracker** (`src/Disfigure.Mods.BuffTracker`).

Stack: C# on `net6.0` and the .NET SDK 10 pinned in `global.json`. It talks to no other repo.

Before proposing a design, read the [ADR index](docs/adr/README.md). Almost everything that
looks like an open decision is already decided, with a reason.

## Rules that are not negotiated from the code

Each one has an ADR behind it.

1. **Mods are BepInEx 6 be.785 plugins (`BasePlugin`), target `net6.0`, and lint lives in
   the build** (analyzers and formatting are errors). `Update` and `OnGUI` go in an own
   `MonoBehaviour` the plugin adds with `AddComponent<T>()`.
   [ADR-0001](docs/adr/0001-melonloader-net6-and-host-gate.md),
   [ADR-0006](docs/adr/0006-bepinex-6-instead-of-melonloader.md)
2. **One project per mod, deployed to `BepInEx\plugins\<mod>\`.** References to the game
   and to BepInEx come from the local install with `Private=false`: no game or BepInEx DLL is
   ever committed. [ADR-0003](docs/adr/0003-one-project-per-mod.md)
3. **No gate and no tests: a step is done when `dotnet build DisfigureMods.slnx` exits 0
   and, if it shows in the game, it was seen working.** The commit for that step carries the
   `BepInEx\LogOutput.log` lines that show it.
   [ADR-0004](docs/adr/0004-no-gate-test-in-the-game.md)
4. **Everything in the repo is in English** (code, comments, mod texts, docs, commit
   messages). Talk to the user in Spanish.
   [ADR-0009](docs/adr/0009-english-and-buff-tracker-name.md)

## Conventions

- Game types and members keep the developer's names (`PlayerStats.maxHealth`): they belong
  to the game, not to us. With BepInEx they live in the global namespace, without the
  `Il2Cpp.` prefix MelonLoader added.
- Lint and formatting are run by the build, so there is nothing to remember: a finding does
  not build.
- The plugin version lives in `BuffTrackerPlugin.Version` and in the `.csproj` `<Version>`;
  keep them in sync, because the mod manager reads it from the DLL. Each release gets a
  `CHANGELOG.md` entry.

## How to work

One [backlog](docs/backlog.md) step at a time, one commit per step. If a step needs a
decision that is expensive to reverse, the ADR comes before the code.

```bash
dotnet build DisfigureMods.slnx
```

Deploy to the game (with the game closed if the mod is loaded):

```bash
dotnet build DisfigureMods.slnx -p:Deploy=true
```

Release package: `tools/package.ps1` writes `dist/BuffTracker-<version>.zip` and its
`.sha256`.

Do not report a step as done based on reading the diff: it builds, and if it touches the
game, it was tried in the game. Mind pipes: `command | tail` returns `tail`'s exit code and
hides the error.

**Before using a Unity method the mod did not use before, check that it exists in the game
build:** `tools/native-methods.ps1 <interop dll> <type> <methods>` says whether it has a
`NativeMethodInfoPtr_*` field (`NATIVE`) or Il2CppInterop rebuilt it (`RECONSTRUCTED`). A
reconstructed method can crash the game without an exception: that is what happened with
`GUI.DrawTextureWithTexCoords` (ADR-0005, ADR-0007).

Smart App Control is on on this machine and it was decided to ignore it (ADR-0004): it can
block a mod's DLL. If a mod does not show up in `BepInEx\LogOutput.log`, look for event 3077
in `Microsoft-Windows-CodeIntegrity/Operational` before looking for the bug in the code. If
the whole game does not start ("Failed to load il2cpp") and `LogOutput.log` was not
updated, check whether the 3077 is for `GameAssembly.dll`: on 2026-11-09 it happened without
mods and went away after restarting the PC (README).

## Where the decisions come from

From the local game install and the first launches with MelonLoader and with BepInEx, on
2026-11-07. The Unity version comes from `UnityPlayer.dll`; the metadata version from the
header of `Disfigure_Data\il2cpp_data\Metadata\global-metadata.dat`; what the loader does
from `J:\SteamLibrary\steamapps\common\Disfigure\BepInEx\LogOutput.log`. MelonLoader was
backed up to `J:\SteamLibrary\Disfigure-MelonLoader-respaldo-2026-11-07`. There is no
previous system and no sibling repo. History before ADR-0009 (2026-11-12) is in Spanish.
