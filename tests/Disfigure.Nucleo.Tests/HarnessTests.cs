using System.Reflection;
using System.Runtime.Versioning;
using System.Xml.Linq;

namespace Disfigure.Nucleo.Tests;

/// <summary>
/// D0 — el harness. No prueba lógica de los mods: prueba que el gate sirve. Si algo de
/// acá falla, lo que sigue no se puede interpretar.
/// </summary>
public class HarnessTests
{
    [Fact]
    public void El_nucleo_compila_para_el_runtime_de_MelonLoader()
    {
        // MelonLoader 0.7.3 carga los mods en .NET 6 (MelonLoader/net6/
        // MelonLoader.runtimeconfig.json: tfm net6.0, rollForward LatestMinor). Un núcleo
        // en net8 compilaría y pasaría estos tests, pero el juego no lo cargaría (ADR-0001).
        var marco = typeof(EnsambladoNucleo).Assembly
            .GetCustomAttribute<TargetFrameworkAttribute>();

        Assert.Equal(".NETCoreApp,Version=v6.0", marco?.FrameworkName);
    }

    [Fact]
    public void El_lint_corre_adentro_del_build()
    {
        // En el coordinador de bot_tg la convención de estilo estaba escrita y nada la
        // corría: juntó cincuenta hallazgos sin que ninguna iteración se enterara (C29).
        // Acá el lint es el build, así que lo que se cuida es que nadie lo apague.
        var props = XDocument.Load(Path.Combine(RaizDelRepo(), "Directory.Build.props"));

        Assert.Equal("true", props.Descendants("TreatWarningsAsErrors").Single().Value);
        Assert.Equal("true", props.Descendants("EnforceCodeStyleInBuild").Single().Value);
    }

    private static string RaizDelRepo()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null
            && !File.Exists(Path.Combine(carpeta.FullName, "DisfigureMods.slnx")))
        {
            carpeta = carpeta.Parent;
        }

        return carpeta?.FullName
            ?? throw new InvalidOperationException(
                $"No encontré DisfigureMods.slnx subiendo desde {AppContext.BaseDirectory}");
    }
}
