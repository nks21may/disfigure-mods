using BepInEx.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// El panel que se abre con F7 (ADR-0008): un botón por indicador para prenderlo o apagarlo,
/// y las teclas 1 a 9 para lo mismo sin mouse. Sólo usa <c>GUI.Box(Rect, String)</c> y
/// <c>GUI.Button(Rect, String)</c>, que son nativos; <c>GUI.Toggle</c> y <c>GUILayout</c>
/// están reconstruidos en este build.
/// </summary>
internal sealed class PanelDeOpciones
{
    private const float AltoDeReferencia = 1080f;
    private const float Ancho = 420f;
    private const float AltoDelTitulo = 44f;
    private const float AltoDeFila = 40f;
    private const float Relleno = 12f;
    private const float AlturaRelativa = 0.20f;
    private const float TamanoDeLetra = 20f;

    private static readonly Key[] TeclasNumericas =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
        Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9,
    };

    private readonly Configuracion configuracion;
    private readonly IReadOnlyList<string> poderes;
    private readonly ManualLogSource registro;
    private bool visible;

    internal PanelDeOpciones(
        Configuracion configuracion,
        IReadOnlyList<string> poderes,
        ManualLogSource registro)
    {
        this.configuracion = configuracion;
        this.poderes = poderes;
        this.registro = registro;
    }

    /// <summary>Lee el teclado: F7 abre y cierra; con el panel abierto, 1 a 9 alternan.</summary>
    internal void Actualizar()
    {
        var teclado = Keyboard.current;
        if (teclado == null)
        {
            return;
        }

        if (teclado.f7Key.wasPressedThisFrame)
        {
            visible = !visible;
            if (visible)
            {
                configuracion.Releer();
            }

            registro.LogInfo(visible ? "Panel abierto." : "Panel cerrado.");
        }

        if (!visible)
        {
            return;
        }

        for (var i = 0; i < poderes.Count && i < TeclasNumericas.Length; i++)
        {
            if (teclado[TeclasNumericas[i]].wasPressedThisFrame)
            {
                Alternar(poderes[i]);
            }
        }
    }

    internal void Dibujar()
    {
        if (!visible)
        {
            return;
        }

        var escala = Screen.height / AltoDeReferencia;
        var ancho = Ancho * escala;
        var relleno = Relleno * escala;
        var altoDeFila = AltoDeFila * escala;
        var altoDelTitulo = AltoDelTitulo * escala;
        var x = (Screen.width - ancho) / 2f;
        var y = Screen.height * AlturaRelativa;
        var alto = altoDelTitulo + poderes.Count * altoDeFila + relleno;

        // El tamaño de letra se cambia sólo mientras se dibuja el panel y después vuelve.
        var caja = GUI.skin.box;
        var boton = GUI.skin.button;
        var letraDeCaja = caja.fontSize;
        var letraDeBoton = boton.fontSize;
        var tamano = Math.Max(1, (int)Math.Round(TamanoDeLetra * escala));
        caja.fontSize = tamano;
        boton.fontSize = tamano;

        GUI.Box(new Rect(x, y, ancho, alto), "Temporizadores  ·  F7 para cerrar");
        for (var i = 0; i < poderes.Count; i++)
        {
            var poder = poderes[i];
            var marca = configuracion.EstaPrendido(poder) ? "x" : "  ";
            var fila = new Rect(
                x + relleno,
                y + altoDelTitulo + i * altoDeFila,
                ancho - 2f * relleno,
                altoDeFila - 6f * escala);
            if (GUI.Button(fila, $"{i + 1}.   [{marca}]   {poder}"))
            {
                Alternar(poder);
            }
        }

        caja.fontSize = letraDeCaja;
        boton.fontSize = letraDeBoton;
    }

    private void Alternar(string poder)
    {
        configuracion.Alternar(poder);
        var estado = configuracion.EstaPrendido(poder) ? "prendido" : "apagado";
        registro.LogInfo($"{poder}: {estado}.");
    }
}
