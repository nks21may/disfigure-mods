# ADR-0004 — Sin gate ni tests: un paso está hecho cuando compila y se vio funcionar en el juego

- **Estado:** aceptado (decidido por el usuario el 2026-11-07)
- **Fecha:** 2026-11-07
- **Decide:** qué dice que un paso está hecho, ahora que el gate no puede dar verde en esta
  máquina
- **Reemplaza:** la parte del gate de [ADR-0001](0001-melonloader-net6-y-gate-en-el-host.md)
  y [ADR-0002](0002-logica-pura-separada-del-juego.md) entero

## Contexto

- El gate de ADR-0001 (`dotnet test DisfigureMods.slnx`) nunca dio verde en esta máquina:
  Smart App Control bloquea `Disfigure.Nucleo.dll` cuando los tests lo cargan (`0x800711C7`,
  evento 3077 de `Microsoft-Windows-CodeIntegrity/Operational`). Como decide archivo por
  archivo, un verde dependía de cómo evaluara Windows cada recompilación.
- El usuario decidió no desactivar Smart App Control, y después, ignorarlo y sacar el gate
  para simplificar el proceso.
- ADR-0002 separaba la lógica en `Disfigure.Nucleo` para probarla con tests unitarios en el
  host. Sin tests, ese proyecto sólo suma otro DLL para desplegar en `UserLibs\` y otro
  archivo que Smart App Control puede bloquear.

## Decisión

- No hay gate, ni tests, ni loop `/tdd`. Se borran `tests/`, `src/Disfigure.Nucleo`,
  `.claude/commands/tdd.md` y `docs/bitacora.md`; quedan en la historia de git.
- Un paso del [backlog](../backlog.md) está hecho cuando `dotnet build DisfigureMods.slnx`
  termina con exit 0 y, si el paso se ve en el juego, se lo vio funcionar. En ese caso el
  mensaje del commit lleva las líneas de `MelonLoader\Latest.log` que lo muestran.
- El lint sigue adentro del build (`Directory.Build.props` y `.editorconfig`, de ADR-0001).
  No es un comando aparte, así que sacarlo no simplifica nada.
- Cada mod es un solo proyecto, con su lógica adentro. Si dos mods llegan a compartir
  código, eso va a `UserLibs\` como dice [ADR-0003](0003-un-proyecto-por-mod.md).

## Consecuencias

**A favor:** cada paso es compilar, desplegar y probar en el juego, sin depender de que Smart
App Control deje cargar los DLL de los tests.

**En contra:**

- Nada verifica la lógica antes de abrir el juego: un error de cálculo en un timer recién
  se ve jugando.
- Smart App Control puede bloquear igual el DLL del mod en el juego. Si pasa, el mod no
  carga, `Latest.log` no lo lista y el visor de eventos muestra el bloqueo (evento 3077 de
  `Microsoft-Windows-CodeIntegrity/Operational`).

**Se revisa si:** la lógica de un mod crece hasta que probarla sólo en el juego se vuelve
lenta o se rompe seguido, o se desactiva Smart App Control.

## Alternativas consideradas

- **Mantener `Disfigure.Nucleo` y sus tests para correrlos a mano:** el usuario eligió
  simplificar del todo. Además, con Smart App Control activado esos tests no se pueden
  correr de forma confiable.
- **Desactivar Smart App Control o firmar los DLL:** el usuario no quiso desactivarlo, y
  firmar con un certificado reconocido es pago.
