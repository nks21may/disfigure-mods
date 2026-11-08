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
si deja cargar un DLL sin firmar: el 2026-11-07 bloqueó 3 de los 152 assemblies que genera
MelonLoader. Si un mod no aparece en `MelonLoader\Latest.log`, puede ser eso: el bloqueo
queda como evento 3077 en `Microsoft-Windows-CodeIntegrity/Operational`.

## Compilar

```bash
dotnet build DisfigureMods.slnx
```

El lint está adentro del build: un hallazgo de los analizadores o del formato no compila.

La ruta del juego sale de `Juego.props`: por defecto
`J:\SteamLibrary\steamapps\common\Disfigure`, y se pisa con la variable de entorno
`DISFIGURE_RUTA_JUEGO`. Si ahí no está `MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll`,
el build corta y dice qué ruta buscó.

## Desplegar

Con el juego cerrado (si está abierto, el DLL del mod está en uso y no se puede pisar):

```bash
dotnet build DisfigureMods.slnx -p:Desplegar=true
```

Copia cada mod a `Mods\` dentro de la carpeta del juego. Sin `-p:Desplegar=true`, el build
no toca el juego. Al abrir el juego, `MelonLoader\Latest.log` lista el mod:

```
[21:04:33.994] Temporizadores v0.1.0
[21:04:33.994] by Nicolás D'Alessandro
[21:04:33.995] Assembly: Disfigure.Mods.Temporizadores.dll
[21:04:33.995] 1 Mod loaded.
[21:04:35.098] [Temporizadores] Temporizadores cargado.
```

Para sacar el mod, se borra su DLL de `Mods\`.

## Cómo se trabaja

- **Qué falta:** [docs/backlog.md](docs/backlog.md), un paso a la vez y un commit por paso.
- **Por qué está así:** [docs/adr/](docs/adr/).

Un paso está hecho cuando compila y, si se ve en el juego, se lo vio funcionar. El commit
lleva las líneas de `Latest.log` que lo muestran.
