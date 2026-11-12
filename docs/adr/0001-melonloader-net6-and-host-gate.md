# ADR-0001 — MelonLoader 0.7.3, `net6.0` for what the game loads, and a host gate with lint in the build

- **Status:** accepted (the user chose MelonLoader and a host gate on 2026-11-07); the gate
  part is superseded by [ADR-0004](0004-no-gate-test-in-the-game.md) and the MelonLoader
  part by [ADR-0006](0006-bepinex-6-instead-of-melonloader.md). `net6.0` and lint in the
  build still apply.
- **Date:** 2026-11-07
- **Decides:** which loader runs the mods, which runtime they are built for, and which
  command says something is done

## Context

- Disfigure is Unity 6000.3.16f1 (the `UnityPlayer.dll` version) built with IL2CPP: there
  are no C# game assemblies, the code is in `GameAssembly.dll` and the metadata in
  `Disfigure_Data\il2cpp_data\Metadata\global-metadata.dat`, unencrypted and version 39
  according to its header. It is a very new version: as of 2026-11-07, neither MelonLoader
  nor BepInEx said in their release notes that they supported it.
- MelonLoader 0.7.3 was tried on 2026-11-07 and worked on the first attempt. In
  `MelonLoader\Latest.log`, Cpp2IL `2022.1.0-pre-release.21` reports `Using actual IL2CPP
  Metadata version 39`, Il2CppInterop `1.5.1-ci.845` generates the assemblies, and the log
  ends with `Assembly Generation Successful!`. 152 assemblies ended up in
  `MelonLoader\Il2CppAssemblies`, among them `Assembly-CSharp.dll` with 1092 types.
- MelonLoader hosts mods on .NET 6: `MelonLoader\net6\MelonLoader.runtimeconfig.json` asks
  for `net6.0` with `rollForward: LatestMinor`, which does not jump major versions. A mod
  built for `net8.0` builds and passes tests on the host, but does not load in the game.
- The mod build references the DLLs of the local install, on `J:`, and deploys there. The
  IDE needs the SDK on the host anyway.

## Decision

- Mods are loaded with **MelonLoader 0.7.3** and patch with the Harmony it ships
  (`MelonLoader\net6\0Harmony.dll`).
- Everything the game loads targets **`net6.0`**. Tests target `net10.0`, because they run
  on the host and not in the game.
- The gate is **`dotnet test DisfigureMods.slnx`** on the host, with the .NET SDK 10 pinned
  in `global.json`.
- **Lint lives in the build.** `Directory.Build.props` turns on `TreatWarningsAsErrors`,
  `EnforceCodeStyleInBuild` and `AnalysisLevel` `latest-recommended`, and `.editorconfig`
  raises formatting (IDE0055), unused usings (IDE0005) and block namespaces (IDE0161) to
  warnings. A finding does not build, and what does not build does not pass the gate. A test
  guarded that nobody turned those two properties off.

## Consequences

**Pros:** a single command with exit 0 says it builds, there is no pending lint and the
tests pass. A test caught it if someone changed `net6.0` by mistake.

**Cons:**

- .NET 6 has been out of support since November 2024, and the NETSDK1138 warning has to be
  turned off (`CheckEolTargetFramework=false`).
- Once there are projects that reference the game, the gate needs Disfigure installed with
  MelonLoader: it cannot be reproduced on a machine without the game.
- With Smart App Control on, Windows does not let unsigned DLLs built here load. On
  2026-11-07 the first gate run gave `FileLoadException ... An Application Control policy
  has blocked this file (0x800711C7)` when loading the core DLL, with event 3077 in
  `Microsoft-Windows-CodeIntegrity/Operational`.

  > **Correction (2026-11-07, later):** it does not block every unsigned DLL. It decides
  > file by file: in the game it let 149 of the 152 assemblies MelonLoader generates
  > through, and in the gate it let the test DLL through. The evidence is in that day's log
  > entry (commit `16a63a1`; the log file was removed with ADR-0004). The decision does not
  > change.

**Revisit if:** MelonLoader moves to hosting mods on another .NET version (its
`runtimeconfig.json` changes), or a game update breaks assembly generation.

## Alternatives considered

- **BepInEx 6 (bleeding edge builds):** supports IL2CPP, but as of 2026-11-07 there was no
  confirmation that it handled metadata v39, and MelonLoader already worked. Trying it was a
  risk with nothing to gain.
- **The gate in Docker with the SDK image:** more reproducible, but it requires mounting the
  `J:` install into the container, Docker Desktop has to be running for every run, and the
  IDE needs the SDK on the host anyway. The user chose the host.
- **Lint as a separate command (`dotnet format --verify-no-changes`):** one more command to
  remember to run, which is exactly what failed in another of the user's repos. With the
  rules in the build it is not needed.
