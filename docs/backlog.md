# Backlog — Mods de Disfigure

Los pasos que faltan, en orden. Un paso a la vez y un commit por paso.

Un paso está hecho cuando `dotnet build DisfigureMods.slnx` da exit 0 y, si se ve en el
juego, se lo vio funcionar; el commit lleva las líneas de `MelonLoader\Latest.log` que lo
muestran ([ADR-0004](adr/0004-sin-gate-se-prueba-en-el-juego.md)).

Los pasos citan símbolos por nombre, nunca por línea.

---

## Timers de los escudos en el HUD

El HUD de la partida muestra cuánto falta para que vuelva cada escudo después de que lo
rompen. El juego guarda esos tiempos pero no los muestra. Son dos escudos, y van los dos:

- **Shield Barrier (la barrera):** `PlayerStats.shieldBarrierCooldown`,
  `PlayerStats.shieldBarrierTimer` y `WeaponManager.shieldBarrierActive`;
- **Projectile Barrier (el escudo de proyectiles):**
  `WeaponManager.projectileShieldRechargeTimer`,
  `WeaponManager.ProjectileShieldRechargeDuration` y
  `WeaponManager.projectileShieldIsActive`.

Cómo cuenta cada uno está en el resultado del paso 3.

- [x] **1. Las referencias al juego.** Un `Juego.props` define `RutaJuego` (por defecto
      `J:\SteamLibrary\steamapps\common\Disfigure`, que se pisa con la variable de entorno
      `DISFIGURE_RUTA_JUEGO`) y referencia `MelonLoader.dll`, `0Harmony.dll`,
      `Il2CppInterop.Runtime.dll` y los `Il2CppAssemblies` con `Private=false`. Si falta
      `MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll`, el build corta con un error que
      dice qué ruta buscó y qué variable la pisa.
      *Hecho cuando:* el proyecto del paso 2 compila contra el juego, y con
      `DISFIGURE_RUTA_JUEGO` apuntando a una carpeta vacía el build muestra ese error.
- [x] **2. El mod carga en el juego.** `src/Disfigure.Mods.Temporizadores` (`net6.0`,
      importa `Juego.props`) declara `MelonInfo` y
      `MelonGame("ColdBrew Entertainment", "Disfigure")`, y escribe una línea con
      `MelonLogger` al iniciar. `dotnet build -p:Desplegar=true` copia el DLL a `Mods\`
      dentro de `RutaJuego`; sin esa propiedad no copia nada. Escribe la parte de
      despliegue del README.
      *Hecho cuando:* al abrir el juego, `Latest.log` dice `1 Mod loaded.` y muestra la línea
      del mod. Si no aparece, revisar primero si lo bloqueó Smart App Control.
- [x] **3. Diagnóstico: el mod registra los campos de los dos escudos.** Mientras haya un
      `PlayerStats` en la escena, escribe una línea por segundo con
      `PlayerStats.shieldBarrierCooldown`, `PlayerStats.shieldBarrierTimer`,
      `PlayerStats.projectileShield`, `WeaponManager.shieldBarrierActive`,
      `WeaponManager.projectileShieldRechargeTimer`,
      `WeaponManager.ProjectileShieldRechargeDuration`,
      `WeaponManager.projectileShieldIsActive` y `WeaponManager.tempShieldHits`.
      *Hecho cuando:* jugando con cada uno de los dos escudos, el usuario deja que se lo
      rompan y espera a que vuelva, y queda anotado acá, para cada escudo, qué campos son
      los suyos, si el timer cuenta para arriba o para abajo y contra qué valor. El usuario
      dice también cómo se llaman los dos poderes en el juego.
      **Necesita al usuario con el juego abierto.**

      **Resultado (partida del 2026-11-07, 21:12 a 21:18):**

      - **Shield Barrier** (la barrera; según su descripción, tarda 3 minutos en volver).
        `PlayerStats.shieldBarrierCooldown` vale 0 hasta tener el poder y 180 desde que se
        agarra. `PlayerStats.shieldBarrierTimer` **cuenta para arriba**: arranca en 0 al
        agarrarlo, vuelve a 0 cuando lo rompen (`WeaponManager.shieldBarrierActive` pasa a
        `False`) y sigue subiendo. Falta = `shieldBarrierCooldown − shieldBarrierTimer`,
        mientras `shieldBarrierActive` sea `False`.
      - **Projectile Barrier** (el escudo de proyectiles; según su descripción, tarda 2
        minutos en reactivarse). `PlayerStats.projectileShield` pasa a `True` al agarrarlo.
        `WeaponManager.projectileShieldRechargeTimer` **cuenta para abajo**: queda en 0
        mientras está activo y, cuando lo rompen (`WeaponManager.projectileShieldIsActive`
        pasa a `False`), salta a `WeaponManager.ProjectileShieldRechargeDuration` (120) y
        baja. Falta = `projectileShieldRechargeTimer`, mientras `projectileShieldIsActive`
        sea `False`. `PlayerStats.ProjectileShieldRechargeDuration` queda en 0: no es el
        campo que usa.
      - Los dos timers son tiempo de juego: se frenan cuando el juego está en pausa (por
        ejemplo, en las pantallas de subir de nivel).
      - **No se llegó a ver la vuelta de ninguno de los dos:** el juego se cerró a las
        21:18:40, con la barrera en 119.59 de 180 y el de proyectiles en 116.31. Que vuelvan
        con la barrera en 180 y el de proyectiles en 0 se verifica en el paso 4.
      - En los últimos 4 segundos, el timer de proyectiles se quedó en 116.31 mientras el de
        la barrera seguía subiendo. No sabemos por qué; si se repite en el paso 4, hay que
        mirarlo.
- [ ] **4. Los timers de los dos escudos en el HUD.** Antes del código, un ADR sobre cómo se
      dibuja: IMGUI (`OnGUI` de MelonLoader; el juego ya lo usa en algunos scripts) o Unity
      UI con TextMeshPro, como `ToggleStatusHud` del juego. Cada timer se muestra como
      `m:ss`, redondeado para arriba (así nunca dice `0:00` mientras falta algo), y
      desaparece cuando su escudo está listo. Lo que falta sale de las cuentas del
      resultado del paso 3. Saca el registro de una línea por segundo del paso 3.
      *Depende del paso 3.*
      *Hecho cuando:* en una partida, al romperse cada escudo aparece su timer, baja hasta
      que el escudo vuelve y desaparece.
      **Necesita al usuario con el juego abierto.**

---

## Después

Ideas que todavía no son pasos: falta una decisión o un dato.

- **Más timers en el HUD**, como el dash (`PlayerStats.dashRechargeTimer`), el parry
  (`WeaponManager.parryRechargeTimer`) o la invulnerabilidad
  (`WeaponManager.invulnerabilityTimer`), definidos en una sola lista. Cuando los escudos
  funcionen de punta a punta, y el usuario elija cuáles.
- **`Disfigure.Comun` en `UserLibs\`**, con el código compartido entre mods. Recién cuando un
  segundo mod repita código del primero (ADR-0003).
