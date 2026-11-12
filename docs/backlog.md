# Backlog — Disfigure mods

The remaining steps, in order. One step at a time and one commit per step.

A step is done when `dotnet build DisfigureMods.slnx` exits 0 and, if it shows in the game,
it was seen working; the commit carries the `BepInEx\LogOutput.log` lines that show it
([ADR-0004](adr/0004-no-gate-test-in-the-game.md)).

Steps 1 to 4 were done with MelonLoader; step 6 moved the mod to BepInEx 6
([ADR-0006](adr/0006-bepinex-6-instead-of-melonloader.md)). Until step 13 the mod was called
`Temporizadores` and the code was in Spanish; class, file and log names quoted in those
steps are the ones from that time ([ADR-0009](adr/0009-english-and-buff-tracker-name.md)).

Steps quote symbols by name, never by line.

---

## Shield timers on the HUD

The run HUD shows how long until each shield comes back after it breaks. The game stores
those times but does not show them. There are two shields, and both are covered:

- **Shield Barrier:** `PlayerStats.shieldBarrierCooldown`, `PlayerStats.shieldBarrierTimer`
  and `WeaponManager.shieldBarrierActive`;
- **Projectile Barrier:** `WeaponManager.projectileShieldRechargeTimer`,
  `WeaponManager.ProjectileShieldRechargeDuration` and
  `WeaponManager.projectileShieldIsActive`.

How each one counts is in the result of step 3.

- [x] **1. Game references.** A props file defines the game path (default
      `J:\SteamLibrary\steamapps\common\Disfigure`, overridable with an environment
      variable) and references `MelonLoader.dll`, `0Harmony.dll`, `Il2CppInterop.Runtime.dll`
      and the game interop assemblies with `Private=false`. If `Assembly-CSharp.dll` is
      missing, the build stops with an error saying which path it looked at and which
      variable overrides it.
      *Done when:* the step 2 project builds against the game, and with the variable
      pointing to an empty folder the build shows that error.
- [x] **2. The mod loads in the game.** The mod project (`net6.0`, imports the props file)
      declares `MelonInfo` and `MelonGame("ColdBrew Entertainment", "Disfigure")`, and logs a
      line on start. A build property copies the DLL to the game's `Mods\`; without it,
      nothing is copied. Writes the deployment part of the README.
      *Done when:* on launch, `Latest.log` says `1 Mod loaded.` and shows the mod's line. If
      it does not appear, check first whether Smart App Control blocked it.
- [x] **3. Diagnostics: the mod logs the fields of both shields.** While there is a
      `PlayerStats` in the scene, it writes one line per second with
      `PlayerStats.shieldBarrierCooldown`, `PlayerStats.shieldBarrierTimer`,
      `PlayerStats.projectileShield`, `WeaponManager.shieldBarrierActive`,
      `WeaponManager.projectileShieldRechargeTimer`,
      `WeaponManager.ProjectileShieldRechargeDuration`,
      `WeaponManager.projectileShieldIsActive` and `WeaponManager.tempShieldHits`.
      *Done when:* playing with each shield, the user lets it break and waits for it to come
      back, and for each shield it is written down here which fields are its own, whether the
      timer counts up or down and against which value. The user also says what the two powers
      are called in the game.
      **Needs the user with the game open.**

      **Result (run on 2026-11-07, 21:12 to 21:18):**

      - **Shield Barrier** (by its description, it takes 3 minutes to come back).
        `PlayerStats.shieldBarrierCooldown` is 0 until the power is owned and 180 after.
        `PlayerStats.shieldBarrierTimer` **counts up**: it starts at 0 when picked, goes back
        to 0 when it breaks (`WeaponManager.shieldBarrierActive` turns `False`) and keeps
        rising. Remaining = `shieldBarrierCooldown − shieldBarrierTimer`, while
        `shieldBarrierActive` is `False`.
      - **Projectile Barrier** (by its description, it takes 2 minutes to reactivate).
        `PlayerStats.projectileShield` turns `True` when picked.
        `WeaponManager.projectileShieldRechargeTimer` **counts down**: it stays at 0 while
        active and, when it breaks (`WeaponManager.projectileShieldIsActive` turns `False`),
        jumps to `WeaponManager.ProjectileShieldRechargeDuration` (120) and goes down.
        Remaining = `projectileShieldRechargeTimer`, while `projectileShieldIsActive` is
        `False`. `PlayerStats.ProjectileShieldRechargeDuration` stays at 0: it is not the
        field in use.
      - Both timers are game time: they stop while the game is paused (for example, on the
        level-up screens).
      - **Neither return was seen:** the game closed at 21:18:40, with the barrier at 119.59
        of 180 and the projectile shield at 116.31. Step 4 checks they come back at 180 and
        at 0.
      - In the last 4 seconds, the projectile timer stayed at 116.31 while the barrier kept
        rising. Unknown why; to be looked at if it happens again in step 4.
- [x] **4. Both shield timers on the HUD.** Before the code, an ADR about how they are
      drawn: IMGUI (`OnGUI`; the game already uses it in some scripts) or Unity UI with
      TextMeshPro, like the game's `ToggleStatusHud`. Each timer shows as `m:ss`, rounded up
      (so it never says `0:00` while something is left), and disappears when its shield is
      ready. The remaining time comes from the step 3 result. Removes the one-line-per-second
      log from step 3.
      *Depends on step 3.*
      *Done when:* in a run, when each shield breaks its timer appears, goes down until the
      shield comes back and disappears.
      **Needs the user with the game open.**

      **Result (run on 2026-11-07, from 21:22):** the user saw both timers work. In
      `Latest.log`, the Projectile Barrier appeared at `2:00` and disappeared with
      `timer=0.00 active=True`; the Shield Barrier appeared at `3:00` and disappeared with
      `timer=120.00 cooldown=120.00`. Mid-run the barrier cooldown went from 180 to 120,
      most likely from a cooldown upgrade: the mod followed it because it reads
      `shieldBarrierCooldown` every frame.
- [x] **5. The power icon instead of the name, at the top right.** Asked by the user, so it
      does not overlap the rest of the HUD: the indicators go against the right edge, at 2/5
      of the screen height (under the score), and each one shows its power's card icon
      (`Upgrade.authoritativeUpgradeCardIconSprite`, looked up by `Upgrade.upgradeName`)
      instead of the name. If the icon is not found, the name stays. Sizes scale with the
      screen height.
      *Done when:* in a run, each indicator appears at the top right with its power's icon,
      and the log shows which sprite it used for each. Checked together with step 6, on
      BepInEx.
      **Needs the user with the game open.**

      **What was seen with MelonLoader (2026-11-07, 21:43 to 21:48):** the game loads 470
      cards, with upper-case names (`SHIELD BARRIER`, `PROJECTILE BARRIER`); the lookup is
      case-insensitive. The Projectile Barrier icon only showed up after two minutes (sprite
      `projectileshield`), and the Shield Barrier one never did: it looks like
      `authoritativeUpgradeCardIconSprite` is filled when the game shows the card. The
      session ended in a crash on alt+tab (`d3d12: Device failed error (887a0007)` in
      `Player-prev.log`), before the user saw the indicators.

      **With BepInEx (22:09):** the game crashed in the same second the Projectile Barrier
      timer with its icon appeared (`0xc0000005` in `dotnet\coreclr.dll`). Icons are removed:
      `GUI.DrawTextureWithTexCoords` crashes the game (note in ADR-0005). **Step 5 stays at the
      new position with the power name, without icons** (v0.4.1). Icons move to "Later", with
      Unity UI.
- [x] **6. The mod becomes a BepInEx 6 be.785 plugin.** MelonLoader is removed from the game
      (backed up in `J:\SteamLibrary\Disfigure-MelonLoader-respaldo-2026-11-07`) and BepInEx
      6.0.0-be.785, Unity IL2CPP, win-x64 is installed. The props file references
      `BepInEx\core` and `BepInEx\interop` and deploys to `BepInEx\plugins\<mod>\`. The
      plugin is a `BasePlugin` that adds an own `MonoBehaviour` with `Update` and `OnGUI`.
      The shield logic, the formatting and the icons do not change. See
      [ADR-0006](adr/0006-bepinex-6-instead-of-melonloader.md).
      *Done when:* `BepInEx\LogOutput.log` lists the plugin and its load line, and in a run
      the timers of both shields appear and disappear as with MelonLoader.
      **Needs the user with the game open.**

      **Result (2026-11-07, from 22:13, v0.4.1):** the user confirmed the timers work at the
      top right and the game did not crash. In `LogOutput.log`, at commit time: the plugin
      load line and `Shield Barrier: appears at 3:00 (timer=0.00 cooldown=180.00
      active=False)`. The timer disappearing when the shield comes back was seen with
      MelonLoader (step 4); with BepInEx it had not reached the log yet. The Il2CppInterop
      warning about a behaviour method with unsupported parameters went away when the logic
      moved to a plain class.
- [x] **7. Power icons, with Unity UI.** An own `Canvas` with one `Image` per power, with its
      card's sprite (`Upgrade.authoritativeUpgradeCardIconSprite`), to the left of the value;
      text stays on IMGUI. Without a sprite, the name stays. Every Unity method it uses is
      native in the game build (`tools/native-methods.ps1`). See
      [ADR-0007](adr/0007-icons-with-unity-ui.md).
      *Done when:* in a run, each indicator appears at the top right with its power's icon,
      the log says which sprite it used, and the game does not crash (alt+tab included).
      **Needs the user with the game open.**

      **Result (2026-11-07, v0.5.0):** the user saw both icons next to the timers. In
      `LogOutput.log`: the Projectile Barrier uses sprite `projectileshield` and the Shield
      Barrier `shieldbarrierv2`; the `Canvas` and one `Image` per power were created. No
      crashes in Windows since the session started (22:20 to 22:26). Whether there was an
      alt+tab was not logged.
- [x] **8. Diagnostics: the shield passive that charges by killing enemies.** Asked by the
      user: the shields have a passive that charges by killing enemies and triggers a buff,
      and they want it on the HUD while unlocked and not active. Candidates in the code:
      `PlayerStats.shieldBoost`, `PlayerStats.shieldBoostMult`, `WeaponManager.shieldBoostI`,
      `WeaponManager.shieldActiveKills`, `PlayerStats.shieldFlare` and
      `WeaponManager.shieldFlareActive` (the `SHIELD BOOST` and `SHIELD FLARE` cards exist).
      The diagnostics write the cards with `SHIELD` in their name once, with their
      description, and one line every time one of those fields changes.
      *Done when:* playing with the passive, it is written down here which card it is, which
      field says it is unlocked, which one counts kills, how many trigger it, how long the
      buff lasts and which field says it is active.
      **Needs the user with the game open.**

      **Result (run on 2026-11-08, until 00:09):** the cards say:
      - `SHIELD FLARE`: "100 kills while Shield Barrier is active temporarily increases
        Vision Size by 50%".
      - `SHIELD BOOST`: "Killing every 50 enemies while Shield Barrier is active temporarily
        increases all Weapon Stats by 5% (Stacks up to +25%)".

      What the log shows:
      - `WeaponManager.shieldActiveKills` counts kills while the Shield Barrier is active and
        **goes back to 0 when it breaks**. Both cards use it.
      - **Shield Flare:** `PlayerStats.shieldFlare` turns `True` when the card is picked.
        `WeaponManager.shieldFlareActive` turns on when kills reach 100 (the second time,
        exactly at 100) and off when the barrier breaks. The 100 is not in any field: it
        comes from the card.
      - **Shield Boost:** `PlayerStats.shieldBoost` turns `True` when picked.
        `WeaponManager.shieldBoostI` is the next target (50, 100, 150…): when reached,
        `PlayerStats.shieldBoostMult` goes up 0.05, up to 1.25. When the barrier breaks,
        `shieldBoostMult` goes back to 1.00, but `shieldBoostI` stays where it was (300).
        With the barrier active again, kills went past 300 (up to 1655) and the multiplier
        did not go up again: it looks like a game bug.
      - The passive that charges to trigger a buff is the **Shield Flare**.
- [x] **9. Shield Flare kills on the HUD.** While the Shield Flare is unlocked
      (`PlayerStats.shieldFlare`) and not active (`WeaponManager.shieldFlareActive`), one more
      row shows its icon and `kills/100` (`WeaponManager.shieldActiveKills`, capped at 100).
      Rows become a generic indicator that shows any text, and the step 8 diagnostics are
      removed.
      *Done when:* in a run with the Shield Flare, the row appears with its icon, counts
      kills, disappears at 100 and comes back at `0/100` when the barrier breaks.
      **Needs the user with the game open.**

      **Result (run on 2026-11-08, until 02:03, v0.6.0):** the user saw the counter work. In
      `LogOutput.log`, the Shield Flare appears at `0/100` with sprite `shieldflarev2`,
      disappears with `kills=101 active=True barrier=True` and comes back at `0/100` when
      the barrier breaks. No crashes in Windows since 00:15.
- [x] **10. Turn each indicator on and off from the `.cfg` and from an F7 panel.** Asked by
      the user, before adding indicators for other stacking skills. One `bool` per indicator
      in the plugin's `.cfg`; F7 opens a panel with one button per indicator, and keys 1 to
      9 do the same. Also, the cards whose description says "stack" are logged once, to
      pick the next indicators. See [ADR-0008](adr/0008-config-and-f7-panel.md).
      *Done when:* the `.cfg` shows up with the three entries; in the game, F7 opens the
      panel, turning an indicator off removes it from the HUD and it stays saved in the
      `.cfg` after restarting; and the log has the list of stacking cards.
      **Needs the user with the game open.**

      **Result (2026-11-08, until 02:26, v0.7.0):** the user says it works, though "it looks
      a bit ugly". The `.cfg` was created with `ShieldBarrier`, `ProjectileBarrier` and
      `ShieldFlare` set to `true`. In `LogOutput.log`: the panel opens, Shield Barrier is
      turned off and right after disappears, then is turned on and appears at `2:59`. No
      crashes since 02:10. **Not verified:** that a turned-off indicator stays off after a
      restart; the user turned it back on before closing. The log also has the 48 cards (of
      498) whose description says "stack".
- [ ] **11. Diagnostics: the 17 stacking cards the user picked.** Of the 48, the user picked
      three groups: the ones lost if you stop killing or hitting (Berserk, Kill Spree,
      Rampage, Rapid Fire, Accelerating Barrel), the ones lasting a few seconds (Overdrive,
      Fragment Surge, Ignition, Saw Burst, Radiant Missiles, Shrouded Frost) and the ones
      charged for the next shot (Magnetic Rounds, Molten Rounds, One in the Chamber,
      Ricochet Rebound, Plague, Psychic Fortification). `StackDiagnostics` reads the
      candidate fields by reflection every 0.1 s and writes a line when a card's unlock or
      value changes, with the time at that moment. Besides the BepInEx log, it appends to a
      file in `BepInEx\` that keeps growing across launches: cards are random and several
      runs are needed. Since 1.0.0 it is off by default (`[Debug] StackDiagnostics` in the
      `.cfg`).

      | Card | Unlock | Value | Time |
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
      | Radiant Missiles | none found | `radiantMissilesMultiplier` | `radiantMissilesTimer` |
      | Shrouded Frost | `concealedSpiral`? | `concealedSpiralBuff`, `concealedSpiralBuffLevel` | `concealedSpiralBuffTimer` |
      | Magnetic Rounds | `magneticRounds` | `WM.magneticRoundMultiplier` | — |
      | Molten Rounds | `moltenRounds` | `moltenRoundsDamageMultiplier` | — |
      | One in the Chamber | `oneInTheChamber` | `oneInTheChamberMultiplier` | — |
      | Ricochet Rebound | `ricochetRebound` | `ricochetReboundMultiplier` | — |
      | Plague | `plague` | `plagueStackCount` | — |
      | Psychic Fortification | `noxiousFortification`? | `WM.noxiousRemnantBuff` | — |

      Without a prefix, fields are on `PlayerStats`; `WM.` is `WeaponManager`. The ones with
      `?` have an internal name different from the card's and are the least certain.
      *Done when:* for each card the user picked, it is written down which field unlocks it,
      how the value rises, how long it lasts and when it resets. The ones that do not come
      up stay in this step until they do.
      **Needs the user playing several runs.**

      **Partial result (three launches on 2026-11-08, 9549 lines of diagnostics):** 4 of the
      17 came up. The other 13 were never unlocked, and the diagnostics keep following them.
      - **Kill Spree:** `killSpree` unlocks it. `killSpreeBuff` goes up 0.05 per kill up to
        1.35. `killSpreeTimer` goes back to about 0 on every kill and counts up; when it
        reaches 3.00, the buff goes back to 1.00 (the card says "Resets after 3 seconds
        without killing"). Outside a run it is 100.
      - **Rapid Fire:** `rapidFire` unlocks it. `rapidFireMultiplier` goes up 0.15 up to
        **1.90** (the card says +75%) and goes back to 1.00 between 0.7 and 1.1 s after the
        last increase. No field holds that time.
      - **Ricochet Rebound:** `ricochetRebound` unlocks it. `ricochetReboundMultiplier` goes
        up 0.15 up to **1.90** (the card says +75%) and goes back to 1.00 on the next shot.
      - **Magnetic Rounds:** `magneticRounds` unlocks it.
        `WeaponManager.magneticRoundMultiplier` goes up 0.05 per EXP picked up, reached 2.05,
        and goes back to 1.00 on every shot: it changed 7790 times and went back to 1.00 3236
        times.
- [x] **12. Kill Spree, Rapid Fire, Ricochet Rebound and Magnetic Rounds indicators.** With
      what step 11 showed, the text of each: `+35% · 2.1s` for Kill Spree (the bonus and the
      time left before it resets) and `+90%` for the other three. They show while the card
      is unlocked, including at `+0%`: the values change several times per second, and a
      row appearing and disappearing that fast cannot be read. Each has its `.cfg` entry and
      its F7 panel button. The four cards leave the diagnostics.
      *Done when:* in a run with any of the four, its row appears with the icon and the bonus
      goes up and down as in the game.
      **Needs the user with the game open.**

      **Result (2026-11-09, v0.8.0):** the user says they look good. **No log evidence:** the
      last launch, which is the one left in `LogOutput.log` (until 01:42), had no run (only
      the load line), and BepInEx overwrites the log of earlier launches. The `.cfg` has the
      four new entries set to `true`. No crashes in Windows since 2026-11-09 00:00.

---

## Public release

- [x] **13. Buff Tracker 1.0.0, everything in English.** Asked by the user, to publish the
      mod so anyone can use it. The mod becomes Buff Tracker (project, DLL, GUID and `.cfg`
      renamed); code, comments, HUD, panel, `.cfg`, log lines, README, ADRs, this backlog
      and `CLAUDE.md` are translated to English. The stack diagnostics are off by default.
      `tools/package.ps1` builds `dist/BuffTracker-1.0.0.zip` with the layout the mod manager
      requires. MIT license and a `CHANGELOG.md`. See
      [ADR-0009](adr/0009-english-and-buff-tracker-name.md).
      *Done when:* the old `Temporizadores` plugin is gone from the game,
      `BepInEx\LogOutput.log` loads `Buff Tracker 1.0.0`, the new `.cfg` is created, and in a
      run the indicators, icons and F7 panel work as before, in English.
      **Needs the user with the game open.**

      **Result (2026-11-12, until 11:13):** the user says everything works as before. The old
      plugin, its `.cfg` and its diagnostics log were moved to
      `J:\SteamLibrary\Disfigure-plugins-desactivados`. In `LogOutput.log`: `Loading [Buff
      Tracker 1.0.0]`, the load line, `Ricochet Rebound: appears at +0% (multiplier=1.00)`,
      `Icon for Ricochet Rebound: sprite 'ricochetreboundv1_new (2)'`, the canvas and the
      image created. The new `.cfg` has `[Debug] StackDiagnostics = false` and the seven
      `[Indicators]` entries. No crashes in Windows that day.
- [ ] **14. Publish the repo and the 1.0.0 release on GitHub.** A public repository on the
      user's account, with the code and a `v1.0.0` release carrying the zip and its
      `.sha256`. Only with the user's go-ahead: it is public.
      *Done when:* the release zip downloads from its `https` URL and installs by hand.
- [ ] **15. Propose Buff Tracker to the Disfigure-Mod-Manager catalog.** A pull request to
      `Purpledudr/Disfigure-Mod-Manager` adding an entry to `plugins.json`
      (`packageType: zip`, `archivePath:
      BepInEx/plugins/Disfigure.Mods.BuffTracker/Disfigure.Mods.BuffTracker.dll`,
      `downloadUrl` pointing to the step 14 release), plus a note on the Discord. Only with
      the user's go-ahead.
      *Done when:* the pull request is open; being listed depends on the author.

---

## Later

Ideas that are not steps yet: a decision or a piece of data is missing.

- **Shield Boost on the HUD** (`+X%` and the kills left for the next +5%). What to show after
  the barrier breaks has to be decided: the game leaves `shieldBoostI` at the old target and
  the multiplier does not go up again (step 8). When the user asks for it.
- **A nicer F7 panel**, with Unity UI instead of basic IMGUI. The user said the current one
  "looks a bit ugly but works". Needs an ADR superseding the panel part of ADR-0008.
- **Indicators for the 13 stacking cards still in step 11**: each one when it comes up in the
  diagnostics.
- **More timers on the HUD**, like the dash (`PlayerStats.dashRechargeTimer`), the parry
  (`WeaponManager.parryRechargeTimer`) or invulnerability
  (`WeaponManager.invulnerabilityTimer`), defined in a single list. When the user picks
  which.
- **A common library in `BepInEx\plugins\`**, with code shared between mods. Only when a
  second mod repeats code from the first (ADR-0003).
