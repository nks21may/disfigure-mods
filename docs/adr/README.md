# ADR de Mods de Disfigure

Architecture Decision Records. Un archivo por decisión, numerado, append-only:
una decisión que cambia **no se edita**, se escribe una nueva que la reemplaza y
la vieja pasa a estado `reemplazado por`. El historial es el punto.

Plantilla: [0000-plantilla.md](0000-plantilla.md).

| # | Decisión | Estado |
|---|---|---|
| [0001](0001-melonloader-net6-y-gate-en-el-host.md) | MelonLoader 0.7.3, `net6.0` para lo que carga el juego y el gate en el host con el lint en el build | aceptado; el gate, reemplazado por 0004 |
| [0002](0002-logica-pura-separada-del-juego.md) | La lógica va en `Disfigure.Nucleo`, sin referencias al juego, y es lo único con tests unitarios | reemplazado por 0004 |
| [0003](0003-un-proyecto-por-mod.md) | Un proyecto por mod, lo compartido en `UserLibs\` y los assemblies del juego nunca se versionan | aceptado |
| [0004](0004-sin-gate-se-prueba-en-el-juego.md) | Sin gate ni tests: un paso está hecho cuando compila y se vio funcionar en el juego | aceptado |
| [0005](0005-timers-con-imgui.md) | Los timers del HUD se dibujan con IMGUI desde el `OnGUI` de MelonLoader | aceptado |

## Cuándo escribir uno

Cuando la decisión sea cara de revertir: una pieza de infraestructura, un límite
entre componentes, un formato que se persiste, una postura de seguridad.

**No** hace falta para elegir una librería de utilidad, nombrar un módulo o
acomodar un layout de carpetas.

Si en un paso del backlog aparece una decisión de este calibre, el ADR se escribe
**antes** del código que la asume.
