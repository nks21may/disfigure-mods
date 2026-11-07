# Bitácora del loop de construcción — Mods de Disfigure

Registro append-only, una entrada por iteración. El backlog está en
[backlog.md](backlog.md) y el protocolo en
[.claude/commands/tdd.md](../.claude/commands/tdd.md).

**Para qué sirve:** una entrada sin la salida del comando pegada textualmente
**es** el delirio. Se detecta de un vistazo, sin tener que releer el diff.

Nunca se edita ni se reordena una entrada anterior. Si algo salió mal, se agrega
una entrada nueva que lo corrige.

**Formato:** ítem · archivos tocados · comando del gate · salida del rojo →
salida del verde · sha.

---

## D0 — Harness y estructura · 2026-11-07

**Archivos:** `.gitattributes`, `.gitignore`, `.editorconfig`, `global.json`,
`Directory.Build.props`, `DisfigureMods.slnx`, `CLAUDE.md`, `README.md`,
`.claude/commands/tdd.md`, `.claude/settings.json`, `docs/adr/README.md`,
`docs/adr/0000-plantilla.md`, `docs/adr/0001-melonloader-net6-y-gate-en-el-host.md`,
`docs/adr/0002-logica-pura-separada-del-juego.md`, `docs/adr/0003-un-proyecto-por-mod.md`,
`docs/backlog.md`, `docs/bitacora.md`, `src/Disfigure.Nucleo/Disfigure.Nucleo.csproj`,
`src/Disfigure.Nucleo/EnsambladoNucleo.cs`,
`tests/Disfigure.Nucleo.Tests/Disfigure.Nucleo.Tests.csproj`,
`tests/Disfigure.Nucleo.Tests/HarnessTests.cs`

**Gate:** `dotnet test DisfigureMods.slnx`

**El gate sabe ponerse rojo.** Con un `using System.Text;` sin usar puesto a propósito en
`EnsambladoNucleo.cs`:

```
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.
P:\disfigure-mods\src\Disfigure.Nucleo\EnsambladoNucleo.cs(1,1): error IDE0005: El uso de la directiva no es necesario. (https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0005) [P:\disfigure-mods\src\Disfigure.Nucleo\Disfigure.Nucleo.csproj]
EXIT=1
```

**No dio verde.** Con el hallazgo sacado, el build y el lint pasan, pero Smart App Control
(activado en esta máquina, `VerifiedAndReputablePolicyState = 1`) no deja cargar
`Disfigure.Nucleo.dll`, que no está firmado. Es el evento 3077 de
`Microsoft-Windows-CodeIntegrity/Operational`. El bloqueo se repitió en las tres corridas, y
también al cargar el DLL directo desde `src/Disfigure.Nucleo/bin`, así que no es por la
copia al directorio de los tests:

```
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.
  Disfigure.Nucleo -> P:\disfigure-mods\src\Disfigure.Nucleo\bin\Debug\net6.0\Disfigure.Nucleo.dll
  Disfigure.Nucleo.Tests -> P:\disfigure-mods\tests\Disfigure.Nucleo.Tests\bin\Debug\net10.0\Disfigure.Nucleo.Tests.dll
Serie de pruebas para P:\disfigure-mods\tests\Disfigure.Nucleo.Tests\bin\Debug\net10.0\Disfigure.Nucleo.Tests.dll (.NETCoreApp,Version=v10.0)
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.12]     Disfigure.Nucleo.Tests.HarnessTests.El_nucleo_compila_para_el_runtime_de_MelonLoader [FAIL]
  Con error Disfigure.Nucleo.Tests.HarnessTests.El_nucleo_compila_para_el_runtime_de_MelonLoader [10 ms]
  Mensaje de error:
   System.IO.FileLoadException : Could not load file or assembly 'P:\disfigure-mods\tests\Disfigure.Nucleo.Tests\bin\Debug\net10.0\Disfigure.Nucleo.dll'. Una directiva de Control de aplicaciones bloqueó este archivo. (0x800711C7)
  Seguimiento de la pila:
     at Disfigure.Nucleo.Tests.HarnessTests.El_nucleo_compila_para_el_runtime_de_MelonLoader()
   at System.Reflection.MethodBaseInvoker.InterpretedInvoke_Method(Object obj, IntPtr* args)
   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)

Con error! - Con error:     1, Superado:     1, Omitido:     0, Total:     2, Duración: 22 ms - Disfigure.Nucleo.Tests.dll (net10.0)
EXIT=1
```

El usuario decidió no desactivar Smart App Control. **D0 queda sin tildar** hasta que el
gate dé verde en esta máquina: o se desactiva Smart App Control, o los DLL se firman con un
certificado que Windows acepte. Mientras tanto, el loop no arranca.

**Nota:** D0 lo dejó armado la sesión de preparación, no una iteración del
loop. La primera iteración real del loop es **D1**.

**Sha:** 78947d8

---

## D0 — Corrección: Smart App Control decide archivo por archivo · 2026-11-07

No es una iteración del loop: corrige una afirmación de la preparación. El README y el
ADR-0001 decían que Smart App Control no deja cargar *los* DLL sin firmar que se compilan
acá, como si fueran todos. No es así.

**Archivos:** `README.md`, `docs/adr/0001-melonloader-net6-y-gate-en-el-host.md`,
`docs/bitacora.md`

**Evidencia.** El usuario abrió el juego dos veces (19:00 y 20:02) y vio un par de avisos
de Smart App Control. En `Microsoft-Windows-CodeIntegrity/Operational` hay 80 eventos de
bloqueo de las últimas tres horas fuera de este repo, y son 12 líneas distintas. Los tres
archivos son siempre los mismos, en los dos arranques:

```
19:00:18  id=3033  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.ShaderRuntimeModule.dll
19:00:18  id=3077  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.ShaderRuntimeModule.dll
19:00:22  id=3033  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.TerrainPhysicsModule.dll
19:00:22  id=3077  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.TerrainPhysicsModule.dll
19:00:33  id=3033  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.UnityWebRequestTextureModule.dll
19:00:33  id=3077  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.UnityWebRequestTextureModule.dll
20:02:48  id=3033  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.ShaderRuntimeModule.dll
20:02:48  id=3033  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.TerrainPhysicsModule.dll
20:02:48  id=3077  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.ShaderRuntimeModule.dll
20:02:48  id=3077  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.TerrainPhysicsModule.dll
20:02:49  id=3033  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.UnityWebRequestTextureModule.dll
20:02:49  id=3077  Disfigure.exe  ->  \SteamLibrary\steamapps\common\Disfigure\MelonLoader\Il2CppAssemblies\UnityEngine.UnityWebRequestTextureModule.dll
```

Los otros 151 assemblies que generó MelonLoader en esta máquina, todos sin firmar, cargaron
bien, y el juego arrancó igual. En `MelonLoader\Latest.log` del arranque de las 20:02:

```
[20:02:41.069] 0 UserLibs loaded.
[20:02:41.073] 0 Plugins loaded.
[20:02:42.332] 0 Mods loaded.
[20:02:50.921] Support Module Loaded: J:\SteamLibrary\steamapps\common\Disfigure\MelonLoader\Dependencies\SupportModules\Il2Cpp.dll
```

En el gate pasa lo mismo: `Disfigure.Nucleo.Tests.dll`, también sin firmar, carga, y
`Disfigure.Nucleo.dll` no. La corrida que siguió a esos arranques da igual que las
anteriores (salida filtrada a las líneas de resultado):

```
Smart App Control (1 = activado): 1
[xUnit.net 00:00:01.01]     Disfigure.Nucleo.Tests.HarnessTests.El_nucleo_compila_para_el_runtime_de_MelonLoader [FAIL]
Con error Disfigure.Nucleo.Tests.HarnessTests.El_nucleo_compila_para_el_runtime_de_MelonLoader [33 ms]
System.IO.FileLoadException : Could not load file or assembly 'P:\disfigure-mods\tests\Disfigure.Nucleo.Tests\bin\Debug\net10.0\Disfigure.Nucleo.dll'. Una directiva de Control de aplicaciones bloqueó este archivo. (0x800711C7)
Con error! - Con error:     1, Superado:     1, Omitido:     0, Total:     2, Duración: 51 ms - Disfigure.Nucleo.Tests.dll (net10.0)
EXIT=1
```

**Qué cambia:** que un mod cargue o no con Smart App Control activado no se puede saber de
antemano, y cada recompilación es un archivo nuevo que Windows vuelve a evaluar. D0 sigue
sin tildar por el mismo motivo.

**Sha:** el commit que agrega esta entrada (`D0: corrección sobre Smart App Control`).
