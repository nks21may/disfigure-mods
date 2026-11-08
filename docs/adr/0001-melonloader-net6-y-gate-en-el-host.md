# ADR-0001 — MelonLoader 0.7.3, `net6.0` para lo que carga el juego y el gate en el host con el lint en el build

- **Estado:** aceptado (el usuario eligió MelonLoader y el gate en el host el 2026-11-07);
  la parte del gate la reemplaza [ADR-0004](0004-sin-gate-se-prueba-en-el-juego.md). Siguen
  vigentes MelonLoader, `net6.0` y el lint en el build.
- **Fecha:** 2026-11-07
- **Decide:** con qué se cargan los mods, para qué runtime se compilan y qué comando dice
  que algo está hecho

## Contexto

- Disfigure es Unity 6000.3.16f1 (la versión de `UnityPlayer.dll`) compilado con IL2CPP: no
  hay assemblies de C# del juego, el código está en `GameAssembly.dll` y la metadata en
  `Disfigure_Data\il2cpp_data\Metadata\global-metadata.dat`, sin cifrar y con versión 39
  según su encabezado. Es una versión muy nueva: al 2026-11-07, ni MelonLoader ni BepInEx
  decían en sus notas de versión que la soportaran.
- MelonLoader 0.7.3 se probó el 2026-11-07 y funcionó al primer intento. En
  `MelonLoader\Latest.log`, Cpp2IL `2022.1.0-pre-release.21` reporta `Using actual IL2CPP
  Metadata version 39`, Il2CppInterop `1.5.1-ci.845` genera los assemblies, y el log cierra
  con `Assembly Generation Successful!`. Quedaron 152 assemblies en
  `MelonLoader\Il2CppAssemblies`, entre ellos `Assembly-CSharp.dll` con 1092 tipos.
- MelonLoader hospeda los mods en .NET 6: `MelonLoader\net6\MelonLoader.runtimeconfig.json`
  pide `net6.0` con `rollForward: LatestMinor`, que no salta de versión mayor. Un mod
  compilado para `net8.0` compila y pasa tests en el host, pero no carga en el juego.
- El build de los mods referencia los DLL de la instalación local, que está en `J:`, y los
  despliega ahí mismo. El IDE necesita el SDK en el host de todas formas.

## Decisión

- Los mods se cargan con **MelonLoader 0.7.3** y parchean con el Harmony que trae
  (`MelonLoader\net6\0Harmony.dll`).
- Todo lo que se carga en el juego apunta a **`net6.0`**. Los tests apuntan a `net10.0`,
  porque corren en el host y no en el juego.
- El gate es **`dotnet test DisfigureMods.slnx`** en el host, con el .NET SDK 10 fijado en
  `global.json`.
- **El lint está en el build.** `Directory.Build.props` activa `TreatWarningsAsErrors`,
  `EnforceCodeStyleInBuild` y `AnalysisLevel` `latest-recommended`, y `.editorconfig` sube a
  aviso el formato (IDE0055), los usings sin usar (IDE0005) y los namespaces de bloque
  (IDE0161). Un hallazgo no compila, y lo que no compila no pasa el gate. El test
  `El_lint_corre_adentro_del_build` cuida que nadie apague esas dos propiedades.

## Consecuencias

**A favor:** un solo comando con exit 0 dice que compila, que no hay lint pendiente y que los
tests pasan. Si alguien cambia `net6.0` por error, lo detecta
`El_nucleo_compila_para_el_runtime_de_MelonLoader`.

**En contra:**

- .NET 6 no tiene soporte desde noviembre de 2024, y hay que apagar el aviso NETSDK1138
  (`CheckEolTargetFramework=false`).
- Cuando haya proyectos que referencian el juego, el gate va a necesitar Disfigure instalado
  con MelonLoader: no se puede reproducir en una máquina sin el juego.
- Con Smart App Control activado, Windows no deja cargar los DLL sin firmar que se compilan
  acá. El 2026-11-07 el primer gate dio `FileLoadException ... Una directiva de Control de
  aplicaciones bloqueó este archivo (0x800711C7)` al cargar `Disfigure.Nucleo.dll`, con el
  evento 3077 en `Microsoft-Windows-CodeIntegrity/Operational`.

  > **Corrección (2026-11-07, más tarde):** no bloquea todos los DLL sin firmar. Decide
  > archivo por archivo: en el juego dejó pasar 149 de los 152 assemblies que genera
  > MelonLoader, y en el gate dejó pasar `Disfigure.Nucleo.Tests.dll`. La evidencia está
  > en la entrada de la bitácora del mismo día (commit `16a63a1`; la bitácora se borró con
  > ADR-0004). La decisión no cambia.

**Se revisa si:** MelonLoader pasa a hospedar los mods en otra versión de .NET (cambia su
`runtimeconfig.json`), o una actualización del juego rompe la generación de assemblies.

## Alternativas consideradas

- **BepInEx 6 (builds bleeding edge):** soporta IL2CPP, pero al 2026-11-07 no había
  confirmación de que manejara la metadata v39, y MelonLoader ya funcionaba. Probarlo era
  arriesgar sin ganar nada.
- **El gate en Docker con la imagen del SDK:** es más reproducible, pero obliga a montar la
  instalación de `J:` en el container, Docker Desktop tiene que estar prendido en cada vuelta
  y el IDE necesita el SDK en el host igual. El usuario eligió el host.
- **El lint como comando aparte (`dotnet format --verify-no-changes`):** es otro comando que
  hay que acordarse de correr, que es exactamente lo que falló en el coordinador de `bot_tg`
  (C29). Con las reglas en el build no hace falta.
