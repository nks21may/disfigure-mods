# ADR-0005 — Los timers del HUD se dibujan con IMGUI desde el `OnGUI` de MelonLoader

- **Estado:** aceptado (decidido por el usuario el 2026-11-07). Con
  [ADR-0006](0006-bepinex-6-en-lugar-de-melonloader.md), `OnGUI` es de un `MonoBehaviour`
  propio que el plugin agrega a la escena; lo demás sigue igual. Las imágenes las dibuja
  Unity UI desde [ADR-0007](0007-iconos-con-unity-ui.md).
- **Fecha:** 2026-11-07
- **Decide:** con qué se dibujan en pantalla los timers que agrega el mod

## Contexto

- Había dos caminos. **IMGUI**: `MelonMod.OnGUI` se llama en cada frame y `GUI.Label`
  dibuja texto encima de todo. **Unity UI con TextMeshPro**: crear un `Canvas` y un
  `TextMeshProUGUI` propios, o colgarse del HUD del juego, que usa esa tecnología
  (`Il2Cpp.ToggleStatusHud` con sus textos de auto-fire, auto-aim y zoom).
- IMGUI está disponible en el build del juego: `MelonLoader\Il2CppAssemblies` trae
  `UnityEngine.IMGUIModule.dll`, y el juego compila scripts con `OnGUI`
  (`HS_CameraHolder`, `Hovl_DemoLasers`, `FPSDisplay`), así que el código nativo de IMGUI no
  quedó afuera del build.
- En IL2CPP, cada tipo o método que el mod usa y el juego no tiene que estar en el build.
  Con TextMeshPro hay más piezas en juego (crear el `Canvas`, elegir la fuente, ubicarlo en
  la jerarquía del HUD), y cada una puede chocar con eso.

## Decisión

- Los timers se dibujan con **IMGUI**, desde `OnGUI` del mod, con `GUI.Label`.
- El tamaño, la negrita y el color salen de rich text (`<size>`, `<b>`, `<color>`), que la
  etiqueta por defecto de IMGUI ya interpreta. Así el mod no crea `GUIStyle` propios.
- La lectura del juego se hace en `OnUpdate` y `OnGUI` sólo dibuja el texto ya armado,
  porque `OnGUI` se llama varias veces por frame.

## Consecuencias

**A favor:** es lo mínimo para ver los timers en pantalla, y usa sólo `GUI.Label`, `Rect` y
`Screen`.

**En contra:** no se ve como el HUD del juego: es texto encima de la imagen, con una
sombra para que se lea. La posición es fija y no sabe dónde están los elementos del HUD:
puede taparlos.

**Se revisa si:** se quiere que los timers se vean integrados al HUD, o IMGUI falla en el
juego (no dibuja, o tira errores por métodos que no están en el build).

> **Nota (2026-11-07): las imágenes con IMGUI crashean el juego.** Para el paso 5 del
> backlog, los iconos de los poderes se dibujaron con `GUI.DrawTextureWithTexCoords`, y el
> juego crasheó las dos veces que se dibujó un icono:
>
> - con MelonLoader, a las 21:48 (`d3d12: Device failed error (887a0007)` y
>   `Unrecoverable GPU device error!` en `Player-prev.log`);
> - con BepInEx, a las 22:09:13, en el mismo segundo en que apareció el primer timer con
>   icono. Windows registró una violación de acceso (`0xc0000005`) en `dotnet\coreclr.dll`.
>
> El texto con `GUI.Label` anduvo una partida entera sin problemas. Lo más probable es que
> el juego no use ese método, que IL2CPP lo haya sacado del build y que la versión que
> reconstruye Il2CppInterop llame mal al código nativo. **Con IMGUI sólo se dibuja texto.**
> Si se quieren imágenes, el camino es Unity UI (un `Image` con el sprite), que el juego sí
> usa, y eso pide un ADR nuevo.

## Alternativas consideradas

- **Unity UI con TextMeshPro:** es el camino para que se vea igual que el HUD del juego,
  pero tiene más piezas que pueden fallar en IL2CPP. El usuario eligió arrancar con IMGUI.
