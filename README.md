# Mods de Disfigure

Mods para **Disfigure** (ColdBrew Entertainment), un roguelike de disparos con vista
cenital hecho en Unity 6000.3.16f1 con IL2CPP, que no tiene soporte oficial de mods. Los
mods se cargan con **MelonLoader 0.7.3**, que genera los assemblies interop del juego en
la instalación local (`J:\SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies`).

Stack: C# con `net6.0` y el .NET SDK 10 fijado en `global.json`. No habla con otros repos.

## Requisitos

- Disfigure instalado con MelonLoader 0.7.3 y arrancado al menos una vez, para que
  existan `MelonLoader\Il2CppAssemblies`.
- El .NET SDK 10: `winget install Microsoft.DotNet.SDK.10`.

**Smart App Control** está activado en esta máquina y se decidió ignorarlo
([ADR-0004](docs/adr/0004-sin-gate-se-prueba-en-el-juego.md)). Decide archivo por archivo
si deja cargar un DLL sin firmar: el 2026-11-07 bloqueó 3 de los 154 assemblies que genera
MelonLoader. Si un mod no aparece en `MelonLoader\Latest.log`, puede ser eso: el bloqueo
queda como evento 3077 en `Microsoft-Windows-CodeIntegrity/Operational`.

## Compilar

```bash
dotnet build DisfigureMods.slnx
```

El lint está adentro del build: un hallazgo de los analizadores o del formato no compila.

Todavía no hay mods que desplegar: el paso del backlog que arma el despliegue escribe esta
parte.

## Cómo se trabaja

- **Qué falta:** [docs/backlog.md](docs/backlog.md), un paso a la vez y un commit por paso.
- **Por qué está así:** [docs/adr/](docs/adr/).

Un paso está hecho cuando compila y, si se ve en el juego, se lo vio funcionar. El commit
lleva las líneas de `Latest.log` que lo muestran.
