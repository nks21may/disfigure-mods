# Backlog de construcción — Mods de Disfigure

Worklist del loop TDD. El protocolo por iteración está en
[.claude/commands/tdd.md](../.claude/commands/tdd.md) y se invoca con `/tdd`.

Cada ítem es **una iteración y un commit**, con su ciclo rojo → verde → refactor
adentro. La suite queda verde al final de cada uno.

Branch: `loop/construccion`. Alcance: sólo el repo `disfigure-mods`.

**El backlog cita símbolos por nombre, nunca por línea.** Cada commit invalida los
números de línea; los nombres sobreviven.

## Cómo se escribe un ítem

Un checkbox markdown (sin tildar, o tildado cuando se cierra) seguido de:

```
**DN — Qué hace, en una línea.** *Depende de DM.*
*Test:* el comportamiento que el test fija, en términos observables.
**Se espera rojo:** la falla concreta que tiene que dar antes del código.
Ver [ADR-NNNN](adr/NNNN-....md) si lo sostiene una decisión.
⚠️ **pérdida de datos** / ⚠️ **seguridad**, cuando corresponda.
```

Un ítem que no dice qué test lo cierra no está listo para el loop.

## Archivos intocables

Vacío. Si al empezar una iteración `git status --short` muestra cambios que **no**
son de ella, se anotan acá y **no** entran en su commit.

## El gate

```bash
dotnet test DisfigureMods.slnx
```

---

## Fase 0 — Fundaciones

- [ ] **D0 — Harness y estructura.** El gate corre, tiene el lint adentro,
      sabe ponerse rojo y da verde. Lo arma la sesión de preparación, no el loop.

---

## Fase 1 — Mod de ejemplo

- [ ] **D1 — El núcleo formatea las stats del jugador.** *Depende de D0.*
      *Test:* `FormatoDeStats.Formatear` recibe un `StatsDelJugador` (nivel, vida máxima,
      velocidad, daño de bala, velocidad de bala, daño cuerpo a cuerpo) y devuelve seis
      líneas `Nombre: valor` en ese orden, con dos decimales; con la cultura del hilo en
      `es-AR`, el separador decimal sigue siendo el punto (`1234.50`, nunca `1234,50`).
      **Se espera rojo:** no compila, porque `StatsDelJugador` no existe.
      Ver [ADR-0002](adr/0002-logica-pura-separada-del-juego.md).
- [ ] **D2 — Los proyectos de mods encuentran la instalación del juego.** *Depende de D0.*
      Un `Juego.props` define `RutaJuego` (por defecto
      `J:\SteamLibrary\steamapps\common\Disfigure`, que se pisa con la variable de entorno
      `DISFIGURE_RUTA_JUEGO`) y referencia `MelonLoader.dll`, `0Harmony.dll`,
      `Il2CppInterop.Runtime.dll` y los `Il2CppAssemblies` con `Private=false`.
      *Test:* compilar un proyecto de prueba que importa `Juego.props`, con
      `DISFIGURE_RUTA_JUEGO` apuntando a una carpeta vacía, termina con exit distinto de 0 y
      un error que nombra la ruta que buscó y la variable para pisarla.
      **Se espera rojo:** sin el chequeo, el build falla con un `CS0246` sobre un tipo del
      juego y no dice qué ruta buscó.
      Ver [ADR-0003](adr/0003-un-proyecto-por-mod.md).
- [ ] **D3 — El mod de ejemplo se identifica para MelonLoader.** *Depende de D2.*
      `src/Disfigure.Mods.Ejemplo` (`net6.0`, importa `Juego.props`) declara `MelonInfo` y
      `MelonGame("ColdBrew Entertainment", "Disfigure")`, así no se carga en otro juego.
      *Test:* leyendo el DLL compilado con `MetadataLoadContext` (sin cargar MelonLoader), el
      ensamblado tiene `MelonGame` con esos dos textos exactos y `MelonInfo` con nombre y
      versión no vacíos.
      **Se espera rojo:** el test no compila, porque el proyecto no existe.
      Ver [ADR-0003](adr/0003-un-proyecto-por-mod.md).
- [ ] **D4 — El build despliega el mod en el juego cuando se le pide.** *Depende de D3.*
      `dotnet build -p:Desplegar=true` copia `Disfigure.Mods.Ejemplo.dll` a `Mods\` y
      `Disfigure.Nucleo.dll` a `UserLibs\` dentro de `RutaJuego`; sin la propiedad no copia
      nada. Escribe la sección "Arranque rápido" del README.
      *Test:* compilar con `-p:Desplegar=true` y las carpetas de destino pisadas a una
      carpeta temporal deja los dos DLL ahí; sin `Desplegar`, la carpeta queda vacía.
      **Se espera rojo:** la carpeta temporal queda vacía.
      Ver [ADR-0003](adr/0003-un-proyecto-por-mod.md).
- [ ] **D5 — F8 en una partida loguea las stats del jugador.** *Depende de D1 y D4.*
      El mod lee `Il2Cpp.PlayerStats` (`level`, `maxHealth`, `speed`, `bulletDamage`,
      `bulletSpeed`, `meleeDamage`), arma un `StatsDelJugador` y escribe
      `FormatoDeStats.Formatear` con `MelonLogger`.
      *Test:* verificación en el juego: con el mod desplegado, F8 en una partida escribe en
      `MelonLoader\Latest.log` las seis líneas con valores reales, y la bitácora pega el
      extracto. La parte con tests unitarios es D1.
      **Se espera rojo:** antes del código, F8 no escribe nada en el log.
      **Necesita al usuario con el juego abierto.**
      Ver [ADR-0002](adr/0002-logica-pura-separada-del-juego.md).

---

## Parkeados

Ideas que todavía no son trabajo tomable: les falta una decisión del usuario o un
dato. Van **sin checkbox** para que el loop no las tome ni las cuente.

- **`Disfigure.Comun` en `UserLibs\`**, con el pegamento compartido con el juego. Recién
  cuando un segundo mod repita código del primero (ADR-0003).
