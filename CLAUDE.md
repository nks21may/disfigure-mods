# Notas para trabajar en este repo

## Qué es

Mods para **Disfigure** (ColdBrew Entertainment), un roguelike de disparos con vista
cenital hecho en Unity 6000.3.16f1 con IL2CPP, que no tiene soporte oficial de mods. Los
mods se cargan con **MelonLoader 0.7.3**, que genera los assemblies interop del juego en
la instalación local (`J:\SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies`).

Stack: C# con `net6.0` para todo lo que se carga en el juego, xUnit en `net10.0` para los
tests y el .NET SDK 10 fijado en `global.json`. No habla con otros repos.

Antes de proponer un diseño, leé el [índice de ADR](docs/adr/README.md). Casi
todo lo que parece una decisión abierta ya está decidido y con motivo.

## Reglas que no se negocian desde el código

Cada una tiene un ADR detrás. Un cambio que las viole se rechaza en review,
aunque los tests pasen.

1. **Lo que se carga en el juego apunta a `net6.0`, y el gate es `dotnet test
   DisfigureMods.slnx` en el host con los analizadores como errores.** MelonLoader 0.7.3
   carga los mods en .NET 6, y el lint vive en el build, no en un comando aparte.
   [ADR-0001](docs/adr/0001-melonloader-net6-y-gate-en-el-host.md)

## Convenciones

- **Código y documentación en español**, incluidos los nombres de clases, métodos
  y variables.
- Los identificadores no llevan tildes ni `ñ` (`TamanoBytes`, no `TamañoBytes`);
  los comentarios y la documentación XML sí, con ortografía correcta.
- Los tipos y miembros del juego quedan como los nombró el desarrollador
  (`Il2Cpp.PlayerStats.maxHealth`): son del juego, no nuestros.
- El lint y el formato **los corre el gate**, así que no hay que acordarse: un
  hallazgo es un rojo como cualquier otro.
- Los tests van en `tests/`, nombrados por comportamiento y no por función:
  `Una_edicion_es_un_evento_distinto`, no `Normalizar2`.

## Cómo se construye

Con TDD, un ítem del [backlog](docs/backlog.md) por iteración, con el protocolo
de [.claude/commands/tdd.md](.claude/commands/tdd.md). Branch: `loop/construccion`.

El ciclo es rojo → verde → refactor, en un solo commit. **El test se escribe
antes que el código**, y se verifica que falle con el error esperado: un test
que nunca falló es indistinguible de no tener test.

## El gate

```bash
dotnet test DisfigureMods.slnx
```

Corre en el host con el .NET SDK 10 de `global.json`: compila con los analizadores y el formato como errores, y después corre los tests, todo en un comando. **Nada está hecho si no lo dice un comando con exit 0** — no
reportes un ítem como resuelto en base a tu lectura del diff.

Cuidado con los pipes: `comando | tail` devuelve el exit code de `tail` y te
oculta el rojo.

## De dónde salen las decisiones

De la instalación local del juego y del primer arranque de MelonLoader, el 2026-11-07.
La versión de Unity sale de `UnityPlayer.dll`; la de la metadata, del encabezado de
`Disfigure_Data\il2cpp_data\Metadata\global-metadata.dat`; lo que hace MelonLoader, de
`J:\SteamLibrary\steamapps\common\Disfigure\MelonLoader\Latest.log`. No hay un sistema
anterior ni repos hermanos.
