# ADR-0003 — Un proyecto por mod, lo compartido en `UserLibs\` y los assemblies del juego nunca se versionan

- **Estado:** aceptado (propuesto el 2026-11-07; confirmado por el usuario el 2026-11-07).
  Con [ADR-0004](0004-sin-gate-se-prueba-en-el-juego.md) ya no existe `Disfigure.Nucleo`:
  lo demás sigue igual.
- **Fecha:** 2026-11-07
- **Decide:** cómo crece el repo cuando hay más de un mod, y de dónde salen las referencias
  al juego

## Contexto

- El usuario quiere algo escalable: varios mods que comparten código, no uno solo.
- MelonLoader carga primero los DLL de `UserLibs\` como librerías, después los de `Plugins\`
  y al final los de `Mods\` (el orden está en `MelonLoader\Latest.log`: `Loading
  UserLibs...`, `Loading Plugins...`, `Loading Mods...`). Un DLL compartido en `UserLibs\`
  lo resuelven todos los mods.
- Los assemblies de `MelonLoader\Il2CppAssemblies` se generan a partir del juego, que es de
  ColdBrew Entertainment: no son nuestros para redistribuir. Además se regeneran cada vez
  que el juego se actualiza.

## Decisión

- **Un proyecto por mod**, `src/Disfigure.Mods.<Nombre>`, que produce un DLL para `Mods\`.
- Lo compartido va en librerías para `UserLibs\`: `Disfigure.Nucleo` (la lógica, ver
  ADR-0002) y, cuando un segundo mod repita pegamento del primero, `Disfigure.Comun`. No
  antes.
- Las referencias al juego y a MelonLoader se toman de la instalación local a través de la
  propiedad `RutaJuego`, con `Private=false` para que no se copien. **Ningún DLL del juego ni
  de MelonLoader entra al repo**, y el `.gitignore` corta los casos obvios.

## Consecuencias

**A favor:** agregar un mod es agregar un proyecto, un mod roto no rompe a los otros, y el
repo se puede publicar sin código del juego adentro.

**En contra:** todos los mods dependen de la versión de `UserLibs\` que esté desplegada: un
cambio que rompe la API de `Disfigure.Nucleo` obliga a recompilar y desplegar todos los mods
juntos. Y sin el juego instalado no compila ningún mod.

**Se revisa si:** los mods se van a distribuir a otros jugadores (ahí conviene empaquetar
cada mod con sus dependencias), o MelonLoader cambia cómo resuelve `UserLibs\`.

## Alternativas consideradas

- **Un solo proyecto con todos los mods adentro:** es más simple al principio, pero un error
  en uno apaga a todos y no se puede activar uno sin el otro.
- **Copiar el código compartido en cada mod, o fusionarlo con ILRepack:** evita `UserLibs\`,
  pero deja dos mods con copias distintas del mismo tipo, y en el juego ese error es difícil
  de ver.
- **Versionar los `Il2CppAssemblies` para compilar sin el juego:** redistribuye código
  derivado del juego y queda viejo con cada actualización.
