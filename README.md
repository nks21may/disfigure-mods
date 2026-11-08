# Mods de Disfigure

Mods para **Disfigure** (ColdBrew Entertainment), un roguelike de disparos con vista
cenital hecho en Unity 6000.3.16f1 con IL2CPP, que no tiene soporte oficial de mods. Los
mods son plugins de **BepInEx 6.0.0-be.785** (Unity IL2CPP, win-x64), la misma build que usa
la comunidad a través de
[Disfigure-Mod-Manager](https://github.com/Purpledudr/Disfigure-Mod-Manager).

Stack: C# con `net6.0` y el .NET SDK 10 fijado en `global.json`. No habla con otros repos.

## Mods

- **Temporizadores:** muestra en el HUD, arriba a la derecha, cuánto falta para que
  vuelvan la Shield Barrier y la Projectile Barrier después de que te las rompen.

## Requisitos

- Disfigure con **BepInEx 6.0.0-be.785** instalado
  (`BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.785+6abdba4.zip`, de
  [builds.bepinex.dev](https://builds.bepinex.dev/projects/bepinex_be)) y arrancado al menos
  una vez, para que exista `BepInEx\interop`. MelonLoader no tiene que estar instalado al
  mismo tiempo.
- El .NET SDK 10: `winget install Microsoft.DotNet.SDK.10`.

**Smart App Control** está activado en esta máquina y se decidió ignorarlo
([ADR-0004](docs/adr/0004-sin-gate-se-prueba-en-el-juego.md)). Decide archivo por archivo
si deja cargar un DLL sin firmar: bloquea siempre los mismos cuatro módulos de Unity que el
juego no usa (`ShaderRuntimeModule`, `TerrainPhysicsModule`, `UnityWebRequestTextureModule`
e `IdentifiersModule`), y el juego arranca igual. Si un mod no aparece en
`BepInEx\LogOutput.log`, puede ser eso: el bloqueo queda como evento 3077 en
`Microsoft-Windows-CodeIntegrity/Operational`.

## Compilar

```bash
dotnet build DisfigureMods.slnx
```

El lint está adentro del build: un hallazgo de los analizadores o del formato no compila.

La ruta del juego sale de `Juego.props`: por defecto
`J:\SteamLibrary\steamapps\common\Disfigure`, y se pisa con la variable de entorno
`DISFIGURE_RUTA_JUEGO`. Si ahí no está `BepInEx\interop\Assembly-CSharp.dll`, el build corta
y dice qué ruta buscó.

## Desplegar

```bash
dotnet build DisfigureMods.slnx -p:Desplegar=true
```

Copia cada mod a `BepInEx\plugins\<mod>\` dentro de la carpeta del juego. Sin
`-p:Desplegar=true`, el build no toca el juego. Si el juego está abierto con el mod cargado,
el DLL está en uso y no se puede pisar: hay que cerrarlo antes. El mod carga en el próximo
arranque, y `BepInEx\LogOutput.log` lo lista.

Para sacar el mod, se borra su carpeta de `BepInEx\plugins\`.

## Cómo se trabaja

- **Qué falta:** [docs/backlog.md](docs/backlog.md), un paso a la vez y un commit por paso.
- **Por qué está así:** [docs/adr/](docs/adr/).

Un paso está hecho cuando compila y, si se ve en el juego, se lo vio funcionar. El commit
lleva las líneas de `BepInEx\LogOutput.log` que lo muestran.
