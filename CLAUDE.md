# Notas para trabajar en este repo

## Qué es

Mods para **Disfigure** (ColdBrew Entertainment), un roguelike de disparos con vista
cenital hecho en Unity 6000.3.16f1 con IL2CPP, que no tiene soporte oficial de mods. Los
mods se cargan con **MelonLoader 0.7.3**, que genera los assemblies interop del juego en
la instalación local (`J:\SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies`).

Stack: C# con `net6.0` y el .NET SDK 10 fijado en `global.json`. No habla con otros repos.

Antes de proponer un diseño, leé el [índice de ADR](docs/adr/README.md). Casi
todo lo que parece una decisión abierta ya está decidido y con motivo.

## Reglas que no se negocian desde el código

Cada una tiene un ADR detrás.

1. **Lo que se carga en el juego apunta a `net6.0`, y el lint vive en el build** (los
   analizadores y el formato son errores). MelonLoader 0.7.3 carga los mods en .NET 6.
   [ADR-0001](docs/adr/0001-melonloader-net6-y-gate-en-el-host.md)
2. **Un proyecto por mod, y lo compartido va a `UserLibs\`.** Las referencias al juego y a
   MelonLoader salen de la instalación local con `Private=false`: ningún DLL del juego ni
   de MelonLoader entra al repo. [ADR-0003](docs/adr/0003-un-proyecto-por-mod.md)
3. **No hay gate ni tests: un paso está hecho cuando `dotnet build DisfigureMods.slnx`
   da exit 0 y, si se ve en el juego, se lo vio funcionar.** El commit de ese paso lleva las
   líneas de `MelonLoader\Latest.log` que lo muestran.
   [ADR-0004](docs/adr/0004-sin-gate-se-prueba-en-el-juego.md)

## Convenciones

- **Código y documentación en español**, incluidos los nombres de clases, métodos
  y variables.
- Los identificadores no llevan tildes ni `ñ` (`TamanoBytes`, no `TamañoBytes`);
  los comentarios y la documentación XML sí, con ortografía correcta.
- Los tipos y miembros del juego quedan como los nombró el desarrollador
  (`Il2Cpp.PlayerStats.maxHealth`): son del juego, no nuestros.
- El lint y el formato los corre el build, así que no hay que acordarse: un hallazgo no
  compila.

## Cómo se trabaja

Un paso del [backlog](docs/backlog.md) a la vez, un commit por paso. Si un paso necesita una
decisión cara de revertir, el ADR va antes que el código.

```bash
dotnet build DisfigureMods.slnx
```

No reportes un paso como hecho en base a tu lectura del diff: compila, y si toca el juego,
se probó en el juego. Cuidado con los pipes: `comando | tail` devuelve el exit code de
`tail` y te oculta el error.

Smart App Control está activado en esta máquina y se decidió ignorarlo (ADR-0004): puede
bloquear el DLL de un mod. Si un mod no aparece en `Latest.log`, buscá el evento 3077 en
`Microsoft-Windows-CodeIntegrity/Operational` antes de buscar el error en el código.

## De dónde salen las decisiones

De la instalación local del juego y del primer arranque de MelonLoader, el 2026-11-07.
La versión de Unity sale de `UnityPlayer.dll`; la de la metadata, del encabezado de
`Disfigure_Data\il2cpp_data\Metadata\global-metadata.dat`; lo que hace MelonLoader, de
`J:\SteamLibrary\steamapps\common\Disfigure\MelonLoader\Latest.log`. No hay un sistema
anterior ni repos hermanos.
