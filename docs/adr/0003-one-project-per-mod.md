# ADR-0003 — One project per mod, shared code deployed alongside, and game assemblies never committed

- **Status:** accepted (proposed on 2026-11-07; confirmed by the user on 2026-11-07). With
  [ADR-0004](0004-no-gate-test-in-the-game.md) there is no core project any more. With
  [ADR-0006](0006-bepinex-6-instead-of-melonloader.md) there are no `Mods\` or `UserLibs\`
  folders: each mod goes to `BepInEx\plugins\<mod>\`, and a shared library would also go in
  `BepInEx\plugins\` (when needed, check that BepInEx resolves it from there). The rest still
  applies.
- **Date:** 2026-11-07
- **Decides:** how the repo grows when there is more than one mod, and where the game
  references come from

## Context

- The user wants something that scales: several mods sharing code, not just one.
- MelonLoader loads `UserLibs\` DLLs first as libraries, then `Plugins\` and last `Mods\`
  (the order is in `MelonLoader\Latest.log`: `Loading UserLibs...`, `Loading Plugins...`,
  `Loading Mods...`). A shared DLL in `UserLibs\` is resolved by every mod.
- The assemblies in `MelonLoader\Il2CppAssemblies` are generated from the game, which
  belongs to ColdBrew Entertainment: they are not ours to redistribute. They are also
  regenerated every time the game updates.

## Decision

- **One project per mod**, `src/Disfigure.Mods.<Name>`, producing one DLL.
- Shared code goes in libraries deployed next to the mods: a core project for logic (see
  ADR-0002) and, when a second mod repeats glue from the first, a common library. Not
  before.
- References to the game and to the loader come from the local install through the
  `GamePath` property, with `Private=false` so they are not copied. **No game or loader DLL
  is ever committed**, and `.gitignore` stops the obvious cases.

## Consequences

**Pros:** adding a mod is adding a project, a broken mod does not break the others, and the
repo can be published without any game code in it.

**Cons:** every mod depends on the version of the shared library that is deployed: a
breaking change in its API forces rebuilding and deploying every mod together. And no mod
builds without the game installed.

**Revisit if:** the mods are distributed to other players (then each mod should be packed
with its dependencies), or the loader changes how it resolves shared libraries.

## Alternatives considered

- **A single project with every mod inside:** simpler at first, but a bug in one turns off
  all of them and one cannot be turned on without the others.
- **Copying the shared code into each mod, or merging it with ILRepack:** avoids a shared
  library, but leaves two mods with different copies of the same type, and in the game that
  bug is hard to see.
- **Committing the game assemblies to build without the game:** redistributes code derived
  from the game and goes stale with every update.
