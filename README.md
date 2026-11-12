# Disfigure mods

BepInEx mods for **Disfigure** (ColdBrew Entertainment), a top-down roguelike shooter.

## Buff Tracker

Shows what the game keeps track of but does not show, at the top right of the screen, under
the score, each with its card icon:

| Indicator | What it shows | When |
|---|---|---|
| **Shield Barrier** | Time left until it comes back (`2:59`) | After it breaks |
| **Projectile Barrier** | Time left until it comes back (`1:59`) | After it breaks |
| **Shield Flare** | Kills with the Shield Barrier active (`37/100`) | While unlocked and not active |
| **Kill Spree** | Current bonus and seconds before it resets (`+35% · 2.1s`) | While you have the card |
| **Rapid Fire** | Current bonus (`+90%`) | While you have the card |
| **Ricochet Rebound** | Bonus for the next shot (`+60%`) | While you have the card |
| **Magnetic Rounds** | Bonus for the next shot (`+20%`) | While you have the card |

Press **F7** in the game to open the options panel and turn each indicator on or off (with
the panel open, keys **1** to **9** do the same). The choice is saved in
`BepInEx\config\dalessandro.disfigure.bufftracker.cfg`, which can also be edited by hand.

### Requirements

- Disfigure on Windows.
- **BepInEx 6.0.0-be.785**, Unity IL2CPP, win-x64
  (`BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.785+6abdba4.zip` from
  [builds.bepinex.dev](https://builds.bepinex.dev/projects/bepinex_be)). It is the build
  [Disfigure-Mod-Manager](https://github.com/Purpledudr/Disfigure-Mod-Manager) installs.
- MelonLoader must not be installed at the same time.

### Install

**With Disfigure-Mod-Manager:** once Buff Tracker is listed in its catalog, install it from
**Available Plugins**.

**By hand:**

1. Install BepInEx 6.0.0-be.785 in the game folder and launch the game once, so it sets
   itself up.
2. Download `BuffTracker-<version>.zip` from the
   [latest release](https://github.com/nks21may/disfigure-mods/releases/latest).
3. Extract it into the game folder. It only contains
   `BepInEx/plugins/Disfigure.Mods.BuffTracker/`.
4. Launch the game. `BepInEx\LogOutput.log` should say `Buff Tracker 1.0.0 loaded`.

To remove it, delete `BepInEx\plugins\Disfigure.Mods.BuffTracker\`.

### Compatibility

- Works alongside the community Translation Mod: the icons are looked up by the cards'
  internal names, which the translation does not change.
- Tested with the game build from November 2026 (Steam build 26116249, Unity 6000.3.16f1).
- If you had the earlier `Temporizadores` version installed, delete
  `BepInEx\plugins\Disfigure.Mods.Temporizadores\`, or both load.

### Known issues

- **Smart App Control** (Windows 11) decides file by file whether an unsigned DLL can load.
  On the development machine it always blocks four Unity modules the game does not use
  (`ShaderRuntimeModule`, `TerrainPhysicsModule`, `UnityWebRequestTextureModule` and
  `IdentifiersModule`), and the game starts anyway. Once it also blocked the game's own
  `GameAssembly.dll`, even without mods ("Failed to load il2cpp"); restarting the PC fixed
  it. Blocks are logged as event 3077 in `Microsoft-Windows-CodeIntegrity/Operational`.
- The F7 panel uses Unity's basic debug look.
- Rapid Fire and Ricochet Rebound go up to +90% in the game, although their cards say +75%.

## For developers

Stack: C# on `net6.0`, built with the .NET SDK 10 (`winget install Microsoft.DotNet.SDK.10`).
The mods reference the game and BepInEx from the local install, so building needs Disfigure
with BepInEx installed and launched once (it generates `BepInEx\interop`).

```bash
dotnet build DisfigureMods.slnx
```

Lint is part of the build: an analyzer or formatting finding does not build.

The game path comes from `Game.props`: `J:\SteamLibrary\steamapps\common\Disfigure` by
default, overridable with the `DISFIGURE_GAME_PATH` environment variable. If
`BepInEx\interop\Assembly-CSharp.dll` is not there, the build stops and says which path it
looked at.

Deploy to the game (if the game is open with the mod loaded, the DLL is in use and cannot be
replaced):

```bash
dotnet build DisfigureMods.slnx -p:Deploy=true
```

Build the release package (`dist/BuffTracker-<version>.zip` and its `.sha256`):

```powershell
tools/package.ps1
```

### Tools

- `tools/native-methods.ps1`: says whether a Unity method is in the game build or was
  rebuilt by Il2CppInterop. A rebuilt method can crash the game; check it before using a
  method the mod did not use before (see
  [ADR-0007](docs/adr/0007-icons-with-unity-ui.md)).

  ```powershell
  tools/native-methods.ps1 J:\SteamLibrary\steamapps\common\Disfigure\BepInEx\interop\UnityEngine.IMGUIModule.dll GUI Label,DrawTextureWithTexCoords
  ```

- `tools/package.ps1`: builds the release zip with the layout Disfigure-Mod-Manager expects.

### How the work is done

- **What is left:** [docs/backlog.md](docs/backlog.md), one step at a time and one commit
  per step.
- **Why it is like this:** [docs/adr/](docs/adr/).
- **What changed per version:** [CHANGELOG.md](CHANGELOG.md).

A step is done when it builds and, if it shows in the game, it was seen working. The commit
carries the `BepInEx\LogOutput.log` lines that show it.

## License

[MIT](LICENSE).
