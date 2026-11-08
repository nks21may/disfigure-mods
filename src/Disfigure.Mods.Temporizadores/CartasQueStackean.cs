using System.Diagnostics;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Diagnóstico para elegir los próximos indicadores: escribe una vez en el log las cartas
/// cuya descripción dice "stack", con la descripción. Se saca cuando el usuario elija.
/// </summary>
internal sealed class CartasQueStackean
{
    private static readonly TimeSpan EsperaEntreBusquedas = TimeSpan.FromSeconds(5);

    private readonly ManualLogSource registro;
    private readonly Stopwatch reloj = Stopwatch.StartNew();
    private TimeSpan proximaBusqueda = TimeSpan.Zero;
    private bool registradas;

    internal CartasQueStackean(ManualLogSource registro)
    {
        this.registro = registro;
    }

    internal void Actualizar()
    {
        if (registradas || reloj.Elapsed < proximaBusqueda)
        {
            return;
        }

        proximaBusqueda = reloj.Elapsed + EsperaEntreBusquedas;
        var cartas = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Upgrade>())
            .Select(objeto => objeto.TryCast<Upgrade>())
            .Where(carta => carta != null)
            .ToList();
        if (cartas.Count == 0)
        {
            return;
        }

        registradas = true;
        var queStackean = cartas
            .Where(carta => (carta!.statdescription ?? string.Empty)
                .Contains("stack", StringComparison.OrdinalIgnoreCase))
            .GroupBy(carta => carta!.upgradeName)
            .OrderBy(grupo => grupo.Key, StringComparer.Ordinal)
            .ToList();
        registro.LogInfo($"Cartas que stackean: {queStackean.Count} de {cartas.Count}.");
        foreach (var grupo in queStackean)
        {
            registro.LogInfo($"stackea | {grupo.Key} | {grupo.First()!.statdescription}");
        }
    }
}
