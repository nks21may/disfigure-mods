namespace Disfigure.Mods.Temporizadores;

/// <summary>Si un timer apareció o desapareció del HUD en este frame.</summary>
internal enum Cambio
{
    Ninguno,
    Aparecio,
    Desaparecio,
}

/// <summary>
/// Un timer del HUD. Se actualiza en <c>OnUpdate</c> y se lee en <c>OnGUI</c>.
/// </summary>
internal sealed class TimerEnPantalla
{
    internal TimerEnPantalla(string poder)
    {
        Poder = poder;
    }

    /// <summary>El nombre del poder en el juego, como en su carta.</summary>
    internal string Poder { get; }

    /// <summary>Lo que falta como <c>m:ss</c>, o <c>null</c> si el timer no se muestra.</summary>
    internal string? Tiempo { get; private set; }

    internal Cambio Actualizar(float? falta)
    {
        var antes = Tiempo is not null;
        Tiempo = falta is { } segundos ? FormatoDeTiempo.Formatear(segundos) : null;
        var ahora = Tiempo is not null;

        if (antes == ahora)
        {
            return Cambio.Ninguno;
        }

        return ahora ? Cambio.Aparecio : Cambio.Desaparecio;
    }
}
