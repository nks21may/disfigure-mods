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

## Fase 1 — Timer del escudo en el HUD

El objetivo es que el HUD de la partida muestre cuánto falta para que vuelva el escudo
después de que lo rompen. El juego guarda ese tiempo pero no lo muestra. En
`Assembly-CSharp` hay dos escudos candidatos: la barrera (`PlayerStats.shieldBarrierCooldown`,
`PlayerStats.shieldBarrierTimer`, `WeaponManager.shieldBarrierActive`) y el escudo de
proyectiles (`WeaponManager.projectileShieldRechargeTimer`,
`WeaponManager.ProjectileShieldRechargeDuration`, `WeaponManager.projectileShieldIsActive`).
Los nombres no dicen si el timer cuenta para arriba o para abajo: eso lo responde D5.

- [ ] **D1 — El núcleo formatea un tiempo restante como `m:ss`.** *Depende de D0.*
      *Test:* `FormatoDeTiempo.Formatear` con segundos `float`: `83.0` → `1:23`, `5.0` →
      `0:05`, `720.0` → `12:00`. Redondea para arriba, así el HUD no muestra `0:00` mientras
      falta algo (`0.2` → `0:01`), y un valor negativo da `0:00`.
      **Se espera rojo:** no compila, porque `FormatoDeTiempo` no existe.
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
- [ ] **D3 — El mod de timers se identifica para MelonLoader.** *Depende de D2.*
      `src/Disfigure.Mods.Temporizadores` (`net6.0`, importa `Juego.props`) declara
      `MelonInfo` y `MelonGame("ColdBrew Entertainment", "Disfigure")`, así no se carga en
      otro juego.
      *Test:* leyendo el DLL compilado con `MetadataLoadContext` (sin cargar MelonLoader), el
      ensamblado tiene `MelonGame` con esos dos textos exactos y `MelonInfo` con nombre y
      versión no vacíos.
      **Se espera rojo:** el test no compila, porque el proyecto no existe.
      Ver [ADR-0003](adr/0003-un-proyecto-por-mod.md).
- [ ] **D4 — El build despliega el mod en el juego cuando se le pide.** *Depende de D3.*
      `dotnet build -p:Desplegar=true` copia `Disfigure.Mods.Temporizadores.dll` a `Mods\` y
      `Disfigure.Nucleo.dll` a `UserLibs\` dentro de `RutaJuego`; sin la propiedad no copia
      nada. Escribe la sección "Arranque rápido" del README.
      *Test:* compilar con `-p:Desplegar=true` y las carpetas de destino pisadas a una
      carpeta temporal deja los dos DLL ahí; sin `Desplegar`, la carpeta queda vacía.
      **Se espera rojo:** la carpeta temporal queda vacía.
      Ver [ADR-0003](adr/0003-un-proyecto-por-mod.md).
- [ ] **D5 — Diagnóstico: el mod registra los campos de los dos escudos.** *Depende de D4.*
      Mientras haya un `PlayerStats` en la escena, el mod escribe con `MelonLogger` una línea
      por segundo con `PlayerStats.shieldBarrierCooldown`, `PlayerStats.shieldBarrierTimer`,
      `PlayerStats.projectileShield`, `WeaponManager.shieldBarrierActive`,
      `WeaponManager.projectileShieldRechargeTimer`,
      `WeaponManager.ProjectileShieldRechargeDuration`,
      `WeaponManager.projectileShieldIsActive` y `WeaponManager.tempShieldHits`.
      *Test:* verificación en el juego: con el poder del escudo, el usuario deja que se lo
      rompan y espera a que vuelva. La bitácora pega el extracto de `Latest.log` y anota qué
      campo es el del poder, si cuenta para arriba o para abajo y contra qué valor. Esa
      respuesta destraba los parkeados de esta fase.
      **Se espera rojo:** antes del código, `Latest.log` no tiene ninguna línea del mod.
      **Necesita al usuario con el juego abierto,** y que diga cómo se llama el poder en el
      juego.
      Ver [ADR-0002](adr/0002-logica-pura-separada-del-juego.md).

---

## Parkeados

Ideas que todavía no son trabajo tomable: les falta una decisión del usuario o un
dato. Van **sin checkbox** para que el loop no las tome ni las cuente.

- **El núcleo calcula cuánto falta para que vuelva el escudo.** Falta el dato de D5: qué
  campo es, si cuenta para arriba o para abajo y contra qué duración. Con eso se escribe el
  test y pasa a ser un ítem.
- **El timer del escudo aparece en el HUD de la partida, y desaparece cuando el escudo está
  listo.** Antes del test va un ADR sobre cómo se dibuja: IMGUI (`OnGUI` de MelonLoader; el
  juego ya lo usa en algunos scripts) o Unity UI con TextMeshPro, como `ToggleStatusHud`
  del juego. Depende del cálculo de arriba.
- **Más timers en el HUD**, como el dash (`PlayerStats.dashRechargeTimer`), el parry
  (`WeaponManager.parryRechargeTimer`) o la invulnerabilidad
  (`WeaponManager.invulnerabilityTimer`), definidos en una sola lista. Recién cuando el del
  escudo funcione de punta a punta, y el usuario elija cuáles.
- **`Disfigure.Comun` en `UserLibs\`**, con el pegamento compartido con el juego. Recién
  cuando un segundo mod repita código del primero (ADR-0003).
