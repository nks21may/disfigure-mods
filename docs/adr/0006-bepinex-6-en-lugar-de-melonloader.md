# ADR-0006 — BepInEx 6 (be.785) en lugar de MelonLoader

- **Estado:** aceptado (decidido por el usuario el 2026-11-07)
- **Fecha:** 2026-11-07
- **Decide:** con qué loader se cargan los mods
- **Reemplaza:** la parte de MelonLoader de
  [ADR-0001](0001-melonloader-net6-y-gate-en-el-host.md). Siguen `net6.0` y el lint en el
  build.

## Contexto

- En el Discord oficial del juego circula
  [Disfigure-Mod-Manager](https://github.com/Purpledudr/Disfigure-Mod-Manager): un programa
  que instala **BepInEx 6.0.0-be.785** (Unity IL2CPP, win-x64) y administra plugins desde
  un `plugins.json`. Ahí se publican la traducción a otros idiomas, el modo sandbox y un
  mod de optimización. La comunidad de mods del juego usa BepInEx.
- Su `DisfigureTranslationMod.csproj` está armado igual que nuestros mods: `net6.0`,
  referencias a la instalación local con `Private=false` (`BepInEx\core` y
  `BepInEx\interop`), Harmony y `UnityEngine.IMGUIModule`. Su paquete con BepInEx incluido
  pasó una prueba de arranque limpio en Windows x64, así que be.785 funciona con esta
  versión del juego (Unity 6000.3.16f1, metadata v39).
- MelonLoader y BepInEx se enganchan al juego por el mismo mecanismo (un DLL proxy en la
  carpeta del juego: `version.dll` uno, `winhttp.dll` el otro). Instalados juntos, cargan
  los dos runtimes en el mismo proceso. Para que el mod conviva con los plugins de la
  comunidad, tiene que ser un plugin de BepInEx.
- El código del mod que lee el juego es Il2CppInterop puro, igual en los dos loaders. Lo
  que depende de MelonLoader es poco: la clase base (`MelonMod`), los callbacks `OnUpdate`
  y `OnGUI`, y el logger.

## Decisión

- Los mods se cargan con **BepInEx 6.0.0-be.785, Unity IL2CPP, win-x64**: la misma build que
  instala Disfigure-Mod-Manager. MelonLoader se saca del juego.
- Cada mod es un `BasePlugin` con `[BepInPlugin]`. `Update` y `OnGUI` van en un
  `MonoBehaviour` propio que el plugin registra en IL2CPP con `AddComponent<T>()`.
- Las referencias salen de `BepInEx\core` y `BepInEx\interop` de la instalación local, con
  `Private=false`, como antes.
- Cada mod se despliega en `BepInEx\plugins\<nombre del mod>\`, y su log va a
  `BepInEx\LogOutput.log`.
- No dependemos de Disfigure-Mod-Manager para desarrollar. Si después queremos que lo
  liste, se le propone al autor una entrada en su `plugins.json`.

## Consecuencias

**A favor:** el mod puede convivir con los plugins que usa la comunidad, y se puede
distribuir por el mismo camino.

**En contra:**

- BepInEx 6 sigue en builds "bleeding edge", sin versión estable.
- Los assemblies interop se regeneran con BepInEx, y hay que volver a verificar en el juego
  todo lo que ya funcionaba con MelonLoader.
- Los nombres de los tipos del juego pueden cambiar: el prefijo `Il2Cpp.` del namespace lo
  agregaba MelonLoader.

**Se revisa si:** be.785 deja de funcionar con una actualización del juego, o la comunidad
se pasa a otro loader.

## Alternativas consideradas

- **Seguir con MelonLoader:** funciona, pero el mod no podría convivir con los plugins de
  BepInEx que usa la comunidad.
- **Soportar los dos loaders desde el mismo código:** duplica la clase de entrada, el
  despliegue y la verificación en el juego, para un público que ya está en BepInEx.
- **Una build de BepInEx más nueva que be.785:** sería otra combinación sin probar con el
  juego. be.785 es la que ya usa la comunidad.
