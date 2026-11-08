# ADR-0007 — Los iconos se dibujan con un `Image` de Unity UI; el texto sigue con IMGUI

- **Estado:** aceptado (el usuario eligió los iconos con Unity UI el 2026-11-07)
- **Fecha:** 2026-11-07
- **Decide:** con qué se dibujan los iconos de los poderes al lado de cada timer
- **Reemplaza:** la parte de imágenes de [ADR-0005](0005-timers-con-imgui.md). El texto
  sigue con IMGUI.

## Contexto

- Con IMGUI, `GUI.DrawTextureWithTexCoords` crasheó el juego las dos veces que dibujó un
  icono (nota en ADR-0005). `GUI.Label` con texto anduvo sin problemas.
- El juego arma su interfaz con Unity UI (`UnityEngine.UI`, que `Assembly-CSharp`
  referencia), así que el código nativo de `Canvas` e `Image` está en el build y se usa
  todo el tiempo. Un `Image` resuelve solo los sprites que vienen de un atlas.
- El sprite de cada poder es `Upgrade.authoritativeUpgradeCardIconSprite`, que el juego
  llena recién cuando muestra la carta.
- **Por qué crasheó IMGUI.** En el interop, los métodos que están en el build del juego
  tienen un campo `NativeMethodInfoPtr_*`; los que IL2CPP sacó y Il2CppInterop reconstruyó
  desde el código de Unity no lo tienen. `herramientas/metodos-nativos.ps1` lo revisa:
  `GUI.DrawTextureWithTexCoords` tiene **0 punteros** (reconstruido), mientras que
  `GUI.Label(Rect, String)` es nativo. Todos los métodos que usa este ADR son nativos: los
  constructores de `GameObject(String)`, `Vector2` y `Rect`, `GameObject.AddComponent(Type)`,
  `SetActive`, `Transform.SetParent`, `Object.DontDestroyOnLoad`, los setters de
  `RectTransform` (`anchorMin`, `anchorMax`, `pivot`, `sizeDelta`, `anchoredPosition`), de
  `Canvas` (`renderMode`, `sortingOrder`), de `Image` (`sprite`, `preserveAspect`) y de
  `Graphic` (`raycastTarget`, `rectTransform`).

## Decisión

- El mod crea **un `Canvas` propio** en modo `ScreenSpaceOverlay`, con un `sortingOrder`
  alto y sin `CanvasScaler`, así una unidad del `RectTransform` es un píxel de la pantalla.
  El objeto se marca con `DontDestroyOnLoad` y no tiene `GraphicRaycaster`, así no ataja
  clics.
- **Un `Image` por poder**, anclado arriba a la derecha, con `preserveAspect` y sin
  `raycastTarget`. Se prende cuando su timer se muestra y se apaga cuando no.
- **El texto sigue con IMGUI** (`GUI.Label`), que ya está probado. Las posiciones de las
  filas salen de un solo cálculo que usan las dos partes, así el icono y el tiempo quedan
  alineados.
- Si no hay sprite para un poder, la fila muestra el nombre, como hasta ahora. Si crear el
  `Canvas` o un `Image` tira una excepción, los iconos se apagan hasta el próximo arranque y
  queda el nombre.

## Consecuencias

**A favor:** usa la misma tecnología con la que el juego dibuja su HUD, en vez de un
método de IMGUI que el juego no usa.

**En contra:** el dibujo queda repartido entre dos sistemas (Unity UI para los iconos,
IMGUI para el texto). Una excepción se puede atajar, pero un crash nativo no: si el `Image`
también crashea, hay que sacarlo.

**Se revisa si:** el `Image` crashea o no se ve, o se quiere pasar también el texto a Unity
UI para que se vea como el resto del HUD.

## Alternativas consideradas

- **Pasar también el texto a Unity UI:** necesita una fuente (copiarla de un `Text` del
  juego o cargar una integrada), y son más piezas nuevas a la vez. Se puede hacer después,
  cuando el `Image` esté probado.
- **Otros métodos de IMGUI para imágenes (`GUI.DrawTexture`, `GUI.Label` con una
  textura):** tienen el mismo riesgo que `DrawTextureWithTexCoords`, y un `GUI.Label` con
  textura dibuja el atlas entero, no el sprite.
