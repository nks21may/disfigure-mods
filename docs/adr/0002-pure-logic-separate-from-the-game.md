# ADR-0002 — Logic goes in a core project with no game references, and it is the only thing with unit tests

- **Status:** superseded by [ADR-0004](0004-no-gate-test-in-the-game.md) (accepted on
  2026-11-07; superseded the same day)
- **Date:** 2026-11-07
- **Decides:** which part of a mod is built with TDD on the host and which part is checked
  in the game

## Context

- The types in `MelonLoader\Il2CppAssemblies` are not the game's code: they are proxies
  Il2CppInterop generates to call into the IL2CPP runtime. `PlayerStats` has 884
  `NativeFieldInfoPtr_*` fields and 355 `NativeMethodInfoPtr_*`: every property reads native
  memory through pointers that only exist with the game running and the loader inside. On
  the host there is no IL2CPP runtime underneath, so a test that touches one of those types
  cannot run.
- The TDD loop needs a gate that runs without opening the game. Every run that opens
  Disfigure pays for the startup and needs someone playing to reach the state under test.

## Decision

- Everything that can be expressed without the game —math, rules, formats, settings— goes
  in a **core project** (`net6.0`) that **does not reference** MelonLoader, Harmony,
  Il2CppInterop or the game assemblies. It is the only thing with unit tests.
- Each mod is **thin glue**: it reads from the game, calls the core and writes to the game
  or to the log. It has no logic of its own worth a test.
- The glue is checked **in the game**, and the backlog item says how: what is done in the
  game and what has to show up in the loader log. The log excerpt is the evidence, like the
  gate output.

## Consequences

**Pros:** the gate runs in seconds and without the game, and what has tests is exactly what
has logic.

**Cons:** glue bugs (a game field read wrong, a Harmony patch on a method IL2CPP inlined)
only show up in the game. Glue items need the user with the game open, and the loop cannot
close them alone.

**Revisit if:** a mod's glue starts making its own decisions (an `if` that matters is logic
and goes to the core), or a way to run Il2CppInterop types outside the game appears.

## Alternatives considered

- **Tests on the game types with doubles:** Il2CppInterop types are proxies over native
  pointers, not classes that can be instantiated or faked on the host.
- **A test mod that runs inside the game and writes results to a file:** viable, but every
  gate run would have to open the game and wait for it to load. Only worth it once there is
  a lot of glue.
- **No tests for the mods:** goes against how the user's repos are built, and the logic that
  breaks the most (math, formats) can be tested.
