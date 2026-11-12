# ADR-0004 — No gate and no tests: a step is done when it builds and was seen working in the game

- **Status:** accepted (decided by the user on 2026-11-07)
- **Date:** 2026-11-07
- **Decides:** what says a step is done, now that the gate cannot pass on this machine
- **Supersedes:** the gate part of [ADR-0001](0001-melonloader-net6-and-host-gate.md) and
  all of [ADR-0002](0002-pure-logic-separate-from-the-game.md)

## Context

- The ADR-0001 gate (`dotnet test DisfigureMods.slnx`) never passed on this machine: Smart
  App Control blocks the core DLL when the tests load it (`0x800711C7`, event 3077 in
  `Microsoft-Windows-CodeIntegrity/Operational`). Since it decides file by file, a pass
  depended on how Windows judged each rebuild.
- The user decided not to turn Smart App Control off, and then to ignore it and drop the
  gate to keep the process simple.
- ADR-0002 put the logic in a core project to test it on the host. Without tests, that
  project only adds another DLL to deploy and another file Smart App Control can block.

## Decision

- No gate, no tests, no `/tdd` loop. `tests/`, the core project, `.claude/commands/tdd.md`
  and the run log are deleted; they stay in the git history.
- A [backlog](../backlog.md) step is done when `dotnet build DisfigureMods.slnx` exits 0
  and, if the step shows in the game, it was seen working. In that case the commit message
  carries the loader log lines that show it.
- Lint stays in the build (`Directory.Build.props` and `.editorconfig`, from ADR-0001). It is
  not a separate command, so removing it would not simplify anything.
- Each mod is a single project with its logic inside. If two mods end up sharing code, it
  goes next to them as [ADR-0003](0003-one-project-per-mod.md) says.

## Consequences

**Pros:** each step is build, deploy and try in the game, without depending on Smart App
Control letting test DLLs load.

**Cons:**

- Nothing checks the logic before opening the game: a math bug in a timer only shows up
  while playing.
- Smart App Control can still block the mod DLL in the game. If it does, the mod does not
  load, the loader log does not list it and the event viewer shows the block (event 3077 in
  `Microsoft-Windows-CodeIntegrity/Operational`).

**Revisit if:** a mod's logic grows to the point where testing it only in the game becomes
slow or breaks often, or Smart App Control is turned off.

## Alternatives considered

- **Keep the core project and its tests to run by hand:** the user chose to simplify all
  the way. Also, with Smart App Control on, those tests cannot run reliably.
- **Turn Smart App Control off or sign the DLLs:** the user did not want to turn it off, and
  signing with a recognized certificate costs money.
