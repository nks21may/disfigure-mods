namespace Disfigure.Mods.Temporizadores;

/// <summary>Si un indicador apareció o desapareció del HUD en este frame.</summary>
internal enum Cambio
{
    Ninguno,
    Aparecio,
    Desaparecio,
}

/// <summary>
/// Una línea del HUD: el poder y lo que se muestra al lado de su icono (un tiempo como
/// <c>2:59</c> o un progreso como <c>37/100</c>). Se actualiza en <c>Update</c> y se lee en
/// <c>OnGUI</c>.
/// </summary>
internal sealed class Indicador
{
    internal Indicador(string poder)
    {
        Poder = poder;
    }

    /// <summary>El nombre del poder en el juego, como en su carta.</summary>
    internal string Poder { get; }

    /// <summary>Lo que se muestra, o <c>null</c> si el indicador no se muestra.</summary>
    internal string? Texto { get; private set; }

    internal Cambio Actualizar(string? texto)
    {
        var antes = Texto is not null;
        Texto = texto;
        var ahora = Texto is not null;

        if (antes == ahora)
        {
            return Cambio.Ninguno;
        }

        return ahora ? Cambio.Aparecio : Cambio.Desaparecio;
    }
}
