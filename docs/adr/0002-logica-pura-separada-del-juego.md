# ADR-0002 — La lógica va en `Disfigure.Nucleo`, sin referencias al juego, y es lo único con tests unitarios

- **Estado:** aceptado (propuesto el 2026-11-07; confirmado por el usuario el 2026-11-07)
- **Fecha:** 2026-11-07
- **Decide:** qué parte de un mod se construye con TDD en el host y qué parte se verifica en
  el juego

## Contexto

- Los tipos de `MelonLoader\Il2CppAssemblies` no son el código del juego: son proxies que
  Il2CppInterop genera para llamar al runtime de IL2CPP. `Il2Cpp.PlayerStats` tiene 884
  campos `NativeFieldInfoPtr_*` y 355 `NativeMethodInfoPtr_*`: cada propiedad lee memoria
  nativa a través de punteros que sólo existen con el juego corriendo y MelonLoader adentro.
  En el host no hay runtime de IL2CPP abajo, así que un test que toque uno de esos tipos no
  se puede ejecutar.
- El loop TDD necesita un gate que corra sin abrir el juego. Cada vuelta que abre Disfigure
  paga el arranque y necesita a alguien jugando para llegar al estado que se quiere probar.

## Decisión

- Todo lo que se puede expresar sin el juego —cálculos, reglas, formatos, configuración— va
  en **`Disfigure.Nucleo`** (`net6.0`), que **no referencia** a MelonLoader, Harmony,
  Il2CppInterop ni los `Il2CppAssemblies`. Es lo único que tiene tests unitarios.
- Cada mod es **pegamento fino**: lee del juego, llama al núcleo y escribe en el juego o en
  el log. No tiene lógica propia que merezca un test.
- El pegamento se verifica **en el juego**, y el ítem del backlog dice cómo: qué se hace en
  el juego y qué tiene que aparecer en `MelonLoader\Latest.log`. La bitácora pega el
  extracto del log como evidencia, igual que la salida del gate.

## Consecuencias

**A favor:** el gate corre en segundos y sin el juego, y lo que tiene tests es justo lo que
tiene lógica.

**En contra:** los errores del pegamento (un campo del juego mal leído, un parche de Harmony
sobre un método que IL2CPP inlineó) recién aparecen en el juego. Los ítems de pegamento
necesitan al usuario con el juego abierto, y el loop no los puede cerrar solo.

**Se revisa si:** el pegamento de un mod empieza a tomar decisiones propias (si hay un `if`
que importa, es lógica y va al núcleo), o aparece una forma de ejecutar tipos de
Il2CppInterop fuera del juego.

## Alternativas consideradas

- **Tests sobre los tipos del juego con dobles:** los tipos de Il2CppInterop son proxies
  sobre punteros nativos, no clases que se puedan instanciar o simular en el host.
- **Un mod de tests que corre adentro del juego y escribe los resultados en un archivo:** es
  viable, pero cada vuelta del gate tendría que abrir el juego y esperar a que cargue. Vale
  la pena recién cuando haya bastante pegamento.
- **Sin tests para los mods:** contradice cómo se construyen los repos de `P:\`, y la lógica
  que más se rompe (cálculos, formatos) sí se puede probar.
