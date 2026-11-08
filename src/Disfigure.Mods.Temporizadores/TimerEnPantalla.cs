namespace Disfigure.Mods.Temporizadores;

/// <summary>Si un timer apareció o desapareció del HUD en este frame.</summary>
internal enum Cambio
{
    Ninguno,
    Aparecio,
    Desaparecio,
}

/// <summary>
/// El texto de un timer en el HUD. Se actualiza en <c>OnUpdate</c> y se lee en <c>OnGUI</c>.
/// </summary>
internal sealed class TimerEnPantalla
{
    internal TimerEnPantalla(string nombre)
    {
        Nombre = nombre;
    }

    internal string Nombre { get; }

    /// <summary>Lo que se dibuja, o <c>null</c> si el timer no se muestra.</summary>
    internal string? Texto { get; private set; }

    internal Cambio Actualizar(float? falta)
    {
        var antes = Texto is not null;
        Texto = falta is { } segundos ? $"{Nombre}  {FormatoDeTiempo.Formatear(segundos)}" : null;
        var ahora = Texto is not null;

        if (antes == ahora)
        {
            return Cambio.Ninguno;
        }

        return ahora ? Cambio.Aparecio : Cambio.Desaparecio;
    }
}
