# ADR-0008 — Cada indicador se prende y apaga desde el `.cfg` de BepInEx y desde un panel con F7

- **Estado:** aceptado (el usuario eligió el archivo de configuración y el panel en el juego
  el 2026-11-08)
- **Fecha:** 2026-11-08
- **Decide:** dónde se prende y se apaga cada indicador del HUD, y cómo se guarda esa
  elección

## Contexto

- El usuario quiere sumar indicadores de otras habilidades stackeables y poder prender y
  apagar cada uno.
- Disfigure-Mod-Manager no sirve para eso: según su README y sus archivos, instala,
  actualiza, prende y apaga plugins enteros, y no tiene una pantalla para las opciones de
  cada plugin.
- BepInEx trae `ConfigFile` (`BasePlugin.Config`): un archivo `.cfg` por plugin en
  `BepInEx\config\`, que se edita con un editor de texto y se guarda solo cuando cambia un
  valor. Es código de BepInEx, sin métodos de Unity.
- Para un panel en el juego, `herramientas/metodos-nativos.ps1` dice que en el build están
  `GUI.Box(Rect, String)`, `GUI.Button(Rect, String)` y `GUI.Window`. En cambio
  `GUI.Toggle` y todo `GUILayout` están **reconstruidos**, como estaba
  `GUI.DrawTextureWithTexCoords` cuando crasheaba (ADR-0005).
- El juego lee el teclado con el Input System. `Keyboard.current`, `Keyboard[Key]`, las
  teclas `f7Key` y `digit1Key`…`digit9Key` y `ButtonControl.wasPressedThisFrame` son nativos.
- En plena partida el juego confina el cursor y el clic dispara.

## Decisión

- **Cada indicador tiene una entrada `bool` en la sección `[Indicadores]`** de
  `BepInEx\config\dalessandro.disfigure.temporizadores.cfg`, prendida por defecto. Un
  indicador apagado no se muestra ni busca su icono.
- **F7 abre y cierra un panel** dibujado con `GUI.Box` y un `GUI.Button` por indicador
  (`[x] Shield Barrier`). Con el panel abierto, las teclas `1` a `9` prenden y apagan el
  indicador de ese número, para no depender del mouse. Cada cambio se escribe en el mismo
  `.cfg`, así queda guardado.
- **Al abrir el panel se vuelve a leer el `.cfg`**, así un cambio hecho con el editor de
  texto aparece sin reiniciar el juego.
- El panel no usa `GUI.Toggle` ni `GUILayout`.

## Consecuencias

**A favor:** cada indicador nuevo se suma con una línea en el `.cfg` y una en el panel. El
`.cfg` sirve aunque el panel falle, y tiene el formato estándar de BepInEx.

**En contra:** el panel es IMGUI básico: no se ve como los menús del juego. F7 queda
tomada; si el juego o otro mod la usa, hay que cambiarla en el código.

**Se revisa si:** hay que cambiar la tecla (se la pasa al `.cfg`), o el panel tiene tantas
opciones que pide algo con más estructura que una lista de botones.

## Alternativas consideradas

- **Solo el `.cfg`:** alcanza, pero para cambiar algo hay que salir del juego o editar el
  archivo con el juego abierto y esperar al próximo arranque.
- **Un Configuration Manager de terceros para IL2CPP:** daría un menú para todos los
  plugins, pero nadie lo probó con Unity 6000.3 y usa muchas APIs de Unity que pueden estar
  reconstruidas en este build.
- **Un panel con Unity UI:** se vería mejor, pero son muchas más piezas (botones, eventos,
  fuente) que hoy no hacen falta.
