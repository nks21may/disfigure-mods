# Mods de Disfigure

Mods para **Disfigure** (ColdBrew Entertainment), un roguelike de disparos con vista
cenital hecho en Unity 6000.3.16f1 con IL2CPP, que no tiene soporte oficial de mods. Los
mods se cargan con **MelonLoader 0.7.3**, que genera los assemblies interop del juego en
la instalación local (`J:\SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies`).

Stack: C# con `net6.0` para todo lo que se carga en el juego, xUnit en `net10.0` para los
tests y el .NET SDK 10 fijado en `global.json`. No habla con otros repos.

## Arranque rápido

Requisitos:

- Disfigure instalado con MelonLoader 0.7.3 y arrancado al menos una vez, para que
  existan `MelonLoader\Il2CppAssemblies`.
- El .NET SDK 10: `winget install Microsoft.DotNet.SDK.10`.
- **Smart App Control desactivado.** Con Smart App Control activado, Windows decide
  archivo por archivo si deja cargar un DLL sin firmar, y cada recompilación es un archivo
  nuevo que vuelve a evaluar. El 2026-11-07 bloqueó `Disfigure.Nucleo.dll` en el gate
  (`FileLoadException ... Una directiva de Control de aplicaciones bloqueó este archivo
  (0x800711C7)`) y 3 de los 154 assemblies que genera MelonLoader en el juego, mientras
  dejaba pasar el resto. Así, el gate y los mods dan rojos al azar.

Todavía no hay mods que desplegar: el ítem que arma el despliegue (D4) escribe el resto de
esta sección.

## El gate

```bash
dotnet test DisfigureMods.slnx
```

Corre en el host con el .NET SDK 10 de `global.json`: compila con los analizadores y el formato como errores, y después corre los tests, todo en un comando.

Una sola clase de tests, para iterar más rápido:

```bash
dotnet test DisfigureMods.slnx --filter "FullyQualifiedName~Disfigure.Nucleo.Tests.HarnessTests"
```

> **Ojo con los pipes.** `comando | tail -20` devuelve el exit code de `tail`,
> que es siempre 0: te oculta el rojo. Corré sin pipe, o cerrá con
> `; echo "EXIT=$?"`.

## Cómo se construye esto

Con TDD, un ítem por iteración:

- **Qué falta:** [docs/backlog.md](docs/backlog.md)
- **Qué se hizo y con qué evidencia:** [docs/bitacora.md](docs/bitacora.md)
- **Por qué está así:** [docs/adr/](docs/adr/)

Una iteración se corre con `/tdd` (el protocolo está en
[.claude/commands/tdd.md](.claude/commands/tdd.md)). El branch de trabajo es
`loop/construccion`.

Para dejarlo iterando solo:

```
/loop /tdd
```
