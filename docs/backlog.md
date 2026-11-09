# Backlog — Mods de Disfigure

Los pasos que faltan, en orden. Un paso a la vez y un commit por paso.

Un paso está hecho cuando `dotnet build DisfigureMods.slnx` da exit 0 y, si se ve en el
juego, se lo vio funcionar; el commit lleva las líneas de `BepInEx\LogOutput.log` que lo
muestran ([ADR-0004](adr/0004-sin-gate-se-prueba-en-el-juego.md)).

Los pasos 1 a 4 se hicieron con MelonLoader; el paso 6 pasa el mod a BepInEx 6
([ADR-0006](adr/0006-bepinex-6-en-lugar-de-melonloader.md)). Los nombres de los tipos del
juego que citan esos pasos llevan el prefijo `Il2Cpp.` de MelonLoader solo en el código de
entonces: con BepInEx están en el namespace global.

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
- [x] **4. Los timers de los dos escudos en el HUD.** Antes del código, un ADR sobre cómo se
      dibuja: IMGUI (`OnGUI` de MelonLoader; el juego ya lo usa en algunos scripts) o Unity
      UI con TextMeshPro, como `ToggleStatusHud` del juego. Cada timer se muestra como
      `m:ss`, redondeado para arriba (así nunca dice `0:00` mientras falta algo), y
      desaparece cuando su escudo está listo. Lo que falta sale de las cuentas del
      resultado del paso 3. Saca el registro de una línea por segundo del paso 3.
      *Depende del paso 3.*
      *Hecho cuando:* en una partida, al romperse cada escudo aparece su timer, baja hasta
      que el escudo vuelve y desaparece.
      **Necesita al usuario con el juego abierto.**

      **Resultado (partida del 2026-11-07, desde las 21:22):** el usuario vio los dos timers
      funcionar. En `Latest.log`, la Projectile Barrier apareció en `2:00` y desapareció con
      `timer=0.00 activo=True`; la Shield Barrier apareció en `3:00` y desapareció con
      `timer=120.00 cooldown=120.00`. A mitad de la partida el cooldown de la barrera bajó de
      180 a 120, seguramente por una mejora de enfriamiento: el mod lo siguió porque lee
      `shieldBarrierCooldown` en cada frame.
- [x] **5. El icono del poder en lugar del nombre, arriba a la derecha.** A pedido del
      usuario, para que no se superponga con el resto del HUD: los timers van contra el
      borde derecho, a 2/5 de la altura de la pantalla (debajo de la puntuación), y cada uno
      muestra el icono de la carta de su poder (`Upgrade.authoritativeUpgradeCardIconSprite`,
      buscado por `Upgrade.upgradeName`) en lugar del nombre. Si no encuentra el icono,
      deja el nombre. Las medidas se escalan con el alto de la pantalla.
      *Hecho cuando:* en una partida, cada timer aparece arriba a la derecha con el icono de
      su poder, y el log muestra qué sprite usó para cada uno. Se verifica junto con el
      paso 6, ya con BepInEx.
      **Necesita al usuario con el juego abierto.**

      **Lo que se vio con MelonLoader (2026-11-07, 21:43 a 21:48):** el juego carga 470
      cartas, con nombres en mayúsculas (`SHIELD BARRIER`, `PROJECTILE BARRIER`); la búsqueda
      no distingue mayúsculas. El icono de la Projectile Barrier apareció recién a los dos
      minutos (sprite `projectileshield`), y el de la Shield Barrier no apareció: parece que
      `authoritativeUpgradeCardIconSprite` se llena cuando el juego muestra la carta. La
      sesión terminó en un crash al hacer alt+tab (`d3d12: Device failed error (887a0007)`,
      en `Player-prev.log`), antes de que el usuario viera los timers.

      **Con BepInEx (22:09):** el juego crasheó en el mismo segundo en que apareció el timer
      de la Projectile Barrier con su icono (`0xc0000005` en `dotnet\coreclr.dll`). Los
      iconos se sacan: `GUI.DrawTextureWithTexCoords` crashea el juego (nota en ADR-0005).
      **El paso 5 queda en la posición nueva con el nombre del poder, sin iconos** (v0.4.1).
      Los iconos pasan a "Después", con Unity UI.
- [x] **6. El mod pasa a ser un plugin de BepInEx 6 be.785.** MelonLoader se saca del juego
      (queda respaldado en `J:\SteamLibrary\Disfigure-MelonLoader-respaldo-2026-11-07`) y se
      instala BepInEx 6.0.0-be.785, Unity IL2CPP, win-x64. `Juego.props` referencia
      `BepInEx\core` y `BepInEx\interop` y despliega en `BepInEx\plugins\<mod>\`.
      `ModTemporizadores` es un `BasePlugin` que agrega `ComportamientoTemporizadores`, el
      `MonoBehaviour` con `Update` y `OnGUI`. La lógica de los escudos, el formato y los
      iconos no cambia. Ver [ADR-0006](adr/0006-bepinex-6-en-lugar-de-melonloader.md).
      *Hecho cuando:* `BepInEx\LogOutput.log` lista el plugin y dice `Temporizadores
      cargado.`, y en una partida los timers de los dos escudos aparecen y desaparecen como
      con MelonLoader.
      **Necesita al usuario con el juego abierto.**

      **Resultado (2026-11-07, desde las 22:13, v0.4.1):** el usuario confirmó que los timers
      andan bien arriba a la derecha y que el juego no crasheó. En `LogOutput.log`, al
      momento del commit: `Loading [Temporizadores 0.4.1]`, `Temporizadores cargado.` y
      `Shield Barrier: aparece en 3:00 (timer=0.00 cooldown=180.00 activa=False)`. Que el
      timer desaparezca cuando vuelve el escudo se vio con MelonLoader (paso 4); con BepInEx
      todavía no llegó al log. El aviso de Il2CppInterop por un método de
      `ComportamientoTemporizadores` con parámetros no soportados desapareció al pasar la
      lógica a `HudDeTemporizadores`, una clase común.
- [x] **7. Los iconos de los poderes, con Unity UI.** Un `Canvas` propio con un `Image` por
      poder, con el sprite de su carta (`Upgrade.authoritativeUpgradeCardIconSprite`), a la
      izquierda del tiempo; el texto sigue con IMGUI. Si no hay sprite, queda el nombre. Todos
      los métodos de Unity que usa son nativos en el build del juego
      (`herramientas/metodos-nativos.ps1`). Ver
      [ADR-0007](adr/0007-iconos-con-unity-ui.md).
      *Hecho cuando:* en una partida, cada timer aparece arriba a la derecha con el icono de
      su poder, el log dice qué sprite usó, y el juego no crashea (tampoco con alt+tab).
      **Necesita al usuario con el juego abierto.**

      **Resultado (2026-11-07, v0.5.0):** el usuario vio los dos iconos al lado de los
      timers. En `LogOutput.log`: la Projectile Barrier usa el sprite `projectileshield` y la
      Shield Barrier `shieldbarrierv2`; se crearon el `Canvas` y un `Image` por poder. Sin
      crashes en Windows desde que arrancó la sesión (22:20 a 22:26). No se registró si hubo
      alt+tab.
- [x] **8. Diagnóstico: la pasiva de los escudos que se carga matando enemigos.** A pedido
      del usuario: los escudos tienen una pasiva que se carga matando enemigos y activa un
      buff, y quiere verla en el HUD mientras esté desbloqueada y no activa. Candidatos en el
      código: `PlayerStats.shieldBoost`, `PlayerStats.shieldBoostMult`,
      `WeaponManager.shieldBoostI`, `WeaponManager.shieldActiveKills`,
      `PlayerStats.shieldFlare` y `WeaponManager.shieldFlareActive` (las cartas `SHIELD BOOST`
      y `SHIELD FLARE` existen). `DiagnosticoDePasiva` escribe una vez las cartas con `SHIELD`
      en el nombre, con su descripción, y una línea cada vez que cambia alguno de esos campos.
      *Hecho cuando:* jugando con la pasiva, queda anotado acá qué carta es, qué campo dice
      que está desbloqueada, cuál cuenta las kills, con cuántas se activa, cuánto dura el
      buff y qué campo dice que está activo.
      **Necesita al usuario con el juego abierto.**

      **Resultado (partida del 2026-11-08, hasta las 00:09):** las cartas dicen:
      - `SHIELD FLARE`: "100 kills while Shield Barrier is active temporarily increases
        Vision Size by 50%".
      - `SHIELD BOOST`: "Killing every 50 enemies while Shield Barrier is active temporarily
        increases all Weapon Stats by 5% (Stacks up to +25%)".

      Lo que muestra el log:
      - `WeaponManager.shieldActiveKills` cuenta las kills mientras la Shield Barrier está
        activa y **vuelve a 0 cuando la rompen**. Lo usan las dos cartas.
      - **Shield Flare:** `PlayerStats.shieldFlare` pasa a `True` al agarrar la carta.
        `WeaponManager.shieldFlareActive` se prende cuando las kills llegan a 100 (la segunda
        vez, justo en 100) y se apaga cuando rompen la barrera. El 100 no está en ningún
        campo: sale de la carta.
      - **Shield Boost:** `PlayerStats.shieldBoost` pasa a `True` al agarrarla.
        `WeaponManager.shieldBoostI` es la próxima meta (50, 100, 150…): al llegar,
        `PlayerStats.shieldBoostMult` sube 0.05, hasta 1.25. Cuando rompen la barrera,
        `shieldBoostMult` vuelve a 1.00, pero `shieldBoostI` se queda donde estaba (300). Con
        la barrera de nuevo activa, las kills pasaron de 300 (llegaron a 1655) y el
        multiplicador no volvió a subir: parece un bug del juego.
      - La pasiva que se carga para activar un buff es la **Shield Flare**.
- [x] **9. Las kills de la Shield Flare en el HUD.** Mientras la Shield Flare está
      desbloqueada (`PlayerStats.shieldFlare`) y sin activar (`WeaponManager.shieldFlareActive`),
      una fila más muestra su icono y `kills/100` (`WeaponManager.shieldActiveKills`, tope 100).
      Las filas pasan a ser `Indicador`, que muestra cualquier texto, y el diagnóstico del
      paso 8 se saca.
      *Hecho cuando:* en una partida con la Shield Flare, la fila aparece con su icono, cuenta
      las kills, desaparece al llegar a 100 y vuelve en `0/100` cuando rompen la barrera.
      **Necesita al usuario con el juego abierto.**

      **Resultado (partida del 2026-11-08, hasta las 02:03, v0.6.0):** el usuario vio el
      contador funcionar. En `LogOutput.log`, la Shield Flare aparece en `0/100` con el
      sprite `shieldflarev2`, desaparece con `kills=101 activa=True barrera=True` y vuelve en
      `0/100` cuando se rompe la barrera. Sin crashes en Windows desde las 00:15.
- [x] **10. Prender y apagar cada indicador desde el `.cfg` y desde un panel con F7.** A
      pedido del usuario, antes de sumar indicadores de otras habilidades stackeables.
      `Configuracion` guarda un `bool` por indicador en `[Indicadores]` de
      `BepInEx\config\dalessandro.disfigure.temporizadores.cfg`; `PanelDeOpciones` abre con
      F7 un panel con un botón por indicador, y las teclas 1 a 9 hacen lo mismo. Además,
      `CartasQueStackean` escribe una vez en el log las cartas cuya descripción dice
      "stack", para elegir los próximos indicadores. Ver
      [ADR-0008](adr/0008-config-y-panel-con-f7.md).
      *Hecho cuando:* el `.cfg` aparece con las tres entradas; en el juego, F7 abre el panel,
      apagar un indicador lo saca del HUD y queda guardado en el `.cfg` después de reiniciar;
      y el log tiene la lista de cartas que stackean.
      **Necesita al usuario con el juego abierto.**

      **Resultado (2026-11-08, hasta las 02:26, v0.7.0):** el usuario dice que anda, aunque
      "se ve un poco feo". El `.cfg` se creó con `ShieldBarrier`, `ProjectileBarrier` y
      `ShieldFlare` en `true`. En `LogOutput.log`: `Panel abierto.`, `Shield Barrier:
      apagado.` y enseguida `Shield Barrier: desaparece`, después `Shield Barrier: prendido.`
      y `Shield Barrier: aparece en 2:59`. Sin crashes desde las 02:10. **Sin verificar:**
      que un indicador apagado siga apagado al reiniciar; el usuario lo volvió a prender
      antes de cerrar. El log también trae las 48 cartas (de 498) cuya descripción dice
      "stack".
- [ ] **11. Diagnóstico: las 17 cartas que stackean que eligió el usuario.** De las 48, el
      usuario eligió tres grupos: las que se pierden si dejás de matar o pegar (Berserk, Kill
      Spree, Rampage, Rapid Fire, Accelerating Barrel), las que duran unos segundos
      (Overdrive, Fragment Surge, Ignition, Saw Burst, Radiant Missiles, Shrouded Frost) y las
      que se cargan para el próximo disparo (Magnetic Rounds, Molten Rounds, One in the
      Chamber, Ricochet Rebound, Plague, Psychic Fortification). `DiagnosticoDeStacks` lee los
      campos candidatos por reflexión cada 0.1 s y escribe una línea cuando cambia el
      desbloqueo o el valor de una carta, con el tiempo en ese momento. Además del log de
      BepInEx, escribe en `BepInEx\Temporizadores-diagnostico.log`, que se acumula entre
      arranques: las cartas salen al azar y hacen falta varias partidas. Saca
      `CartasQueStackean`.

      | Carta | Desbloqueo | Valor | Tiempo |
      |---|---|---|---|
      | Berserk | `berserk` | `berserkMultiplier` | `berserkTimer` |
      | Kill Spree | `killSpree` | `killSpreeBuff` | `killSpreeTimer` |
      | Rampage | `rampage` | `rampageMultiplier` | — |
      | Rapid Fire | `rapidFire` | `rapidFireMultiplier` | — |
      | Accelerating Barrel | `acceleratingBarrel` | `WM.acceleratingBarrelBuff` | `WM.acceleratingBarrelTimer` |
      | Overdrive | `overdrive` | `overdriveMultiplier` | `overdriveTimer` |
      | Fragment Surge | `splintersurge`? | `splintersurgeMultiplier` | `splintersurgeTimer` |
      | Ignition | `ignition` | `ignitionFireRateBuff` | `ignitionTimer` |
      | Saw Burst | `sawBurst` | `WM.sawBurstBuff` | `WM.sawBurstTimer` |
      | Radiant Missiles | ninguno encontrado | `radiantMissilesMultiplier` | `radiantMissilesTimer` |
      | Shrouded Frost | `concealedSpiral`? | `concealedSpiralBuff`, `concealedSpiralBuffLevel` | `concealedSpiralBuffTimer` |
      | Magnetic Rounds | `magneticRounds` | `WM.magneticRoundMultiplier` | — |
      | Molten Rounds | `moltenRounds` | `moltenRoundsDamageMultiplier` | — |
      | One in the Chamber | `oneInTheChamber` | `oneInTheChamberMultiplier` | — |
      | Ricochet Rebound | `ricochetRebound` | `ricochetReboundMultiplier` | — |
      | Plague | `plague` | `plagueStackCount` | — |
      | Psychic Fortification | `noxiousFortification`? | `WM.noxiousRemnantBuff` | — |

      Sin prefijo, los campos son de `PlayerStats`; `WM.` es `WeaponManager`. Los que llevan
      `?` tienen un nombre interno distinto del de la carta y son los más dudosos.
      *Hecho cuando:* para cada carta que el usuario haya agarrado, queda anotado qué campo
      la desbloquea, cómo sube el valor, cuánto dura y cuándo se reinicia. Las que no
      salgan siguen en este paso hasta que salgan.
      **Necesita al usuario jugando varias partidas.**

      **Resultado parcial (tres arranques del 2026-11-08, 9549 líneas en
      `Temporizadores-diagnostico.log`):** salieron 4 de las 17. Las 13 restantes no se
      desbloquearon nunca, y el diagnóstico sigue con ellas.
      - **Kill Spree:** `killSpree` la desbloquea. `killSpreeBuff` sube 0.05 por kill hasta
        1.35. `killSpreeTimer` vuelve a casi 0 en cada kill y cuenta para arriba; cuando
        llega a 3.00, el buff vuelve a 1.00 (la carta dice "Resets after 3 seconds without
        killing"). Fuera de una partida vale 100.
      - **Rapid Fire:** `rapidFire` la desbloquea. `rapidFireMultiplier` sube 0.15 hasta
        **1.90** (la carta dice +75%) y vuelve a 1.00 entre 0.7 y 1.1 s después del último
        aumento. No hay campo con ese tiempo.
      - **Ricochet Rebound:** `ricochetRebound` la desbloquea. `ricochetReboundMultiplier`
        sube 0.15 hasta **1.90** (la carta dice +75%) y vuelve a 1.00 con el próximo disparo.
      - **Magnetic Rounds:** `magneticRounds` la desbloquea.
        `WeaponManager.magneticRoundMultiplier` sube 0.05 por EXP juntada, llegó a 2.05, y
        vuelve a 1.00 con cada disparo: cambió 7790 veces y volvió a 1.00 3236 veces.
- [x] **12. Los indicadores de Kill Spree, Rapid Fire, Ricochet Rebound y Magnetic Rounds.**
      Con lo que mostró el paso 11, `Stacks` arma el texto de cada uno: `+35% · 2.1s` para
      Kill Spree (el bonus y lo que falta para que se reinicie) y `+90%` para las otras
      tres. Se muestran mientras la carta está desbloqueada, también en `+0%`: los valores
      cambian varias veces por segundo, y una fila que aparece y desaparece a ese ritmo no
      se puede leer. Cada uno tiene su entrada en el `.cfg` y su botón en el panel de F7.
      Las cuatro cartas salen del diagnóstico.
      *Hecho cuando:* en una partida con alguna de las cuatro, su fila aparece con el icono
      y el bonus sube y baja como en el juego.
      **Necesita al usuario con el juego abierto.**

      **Resultado (2026-11-09, v0.8.0):** el usuario dice que se ven bien. **Sin evidencia en
      el log:** el último arranque, que es el que queda en `LogOutput.log` (hasta las
      01:42), no tuvo ninguna partida (sólo `Temporizadores cargado.`), y BepInEx pisa el log
      de los arranques anteriores. El `.cfg` tiene las cuatro entradas nuevas en `true`. Sin
      crashes en Windows desde el 2026-11-09 a las 00:00.

---

## Después

Ideas que todavía no son pasos: falta una decisión o un dato.

- **La Shield Boost en el HUD** (`+X%` y las kills que faltan para el próximo +5%). Hay que
  decidir qué mostrar después de que rompen la barrera: el juego deja `shieldBoostI` en la
  meta vieja y el multiplicador no vuelve a subir (paso 8). Cuando el usuario la pida.
- **Un panel de F7 más lindo**, con Unity UI en lugar de IMGUI básico. El usuario dijo
  que el actual "se ve un poco feo pero funciona". Pide un ADR que reemplace la parte del
  panel de ADR-0008.
- **Indicadores de otras habilidades que stackean** (paso 10 tiene la lista): cada una con
  su diagnóstico, como la Shield Flare. Cuando el usuario elija cuáles.
- **Más timers en el HUD**, como el dash (`PlayerStats.dashRechargeTimer`), el parry
  (`WeaponManager.parryRechargeTimer`) o la invulnerabilidad
  (`WeaponManager.invulnerabilityTimer`), definidos en una sola lista. Cuando los escudos
  funcionen de punta a punta, y el usuario elija cuáles.
- **`Disfigure.Comun` en `BepInEx\plugins\`**, con el código compartido entre mods. Recién
  cuando un segundo mod repita código del primero (ADR-0003).
- **Proponerle el mod al autor de Disfigure-Mod-Manager** para que lo sume a su
  `plugins.json`. Cuando el mod esté verificado con BepInEx, y si el usuario quiere
  publicarlo.
