using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine;
using Imagen = UnityEngine.UI.Image;

namespace Disfigure.Mods.Temporizadores;

/// <summary>
/// Los iconos de los timers, dibujados con Unity UI (ADR-0007): un <c>Canvas</c> propio en
/// modo overlay, sin <c>CanvasScaler</c> (una unidad es un píxel), y un <c>Image</c> por poder,
/// anclado arriba a la derecha.
/// </summary>
internal sealed class IconosEnPantalla
{
    private const int OrdenDeDibujo = 5000;

    private readonly ManualLogSource registro;
    private readonly Dictionary<string, Imagen> imagenes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> visibles = new(StringComparer.Ordinal);
    private GameObject? raiz;
    private bool apagados;

    internal IconosEnPantalla(ManualLogSource registro)
    {
        this.registro = registro;
    }

    /// <summary>Falso si crear el <c>Canvas</c> o un <c>Image</c> falló: las filas usan el nombre.</summary>
    internal bool Disponibles => !apagados;

    /// <summary>
    /// Muestra el icono de cada fila que tiene sprite, en su posición, y apaga el resto.
    /// </summary>
    internal void Mostrar(IReadOnlyList<Fila> filas)
    {
        if (apagados)
        {
            return;
        }

        try
        {
            var conIcono = new HashSet<string>(StringComparer.Ordinal);
            foreach (var fila in filas)
            {
                if (fila.Sprite == null)
                {
                    continue;
                }

                var imagen = ImagenDe(fila.Indicador.Poder);
                if (imagen.sprite != fila.Sprite)
                {
                    imagen.sprite = fila.Sprite;
                }

                var rect = imagen.rectTransform;
                rect.sizeDelta = new Vector2(fila.Icono.Ancho, fila.Icono.Alto);
                rect.anchoredPosition = new Vector2(
                    -(Screen.width - fila.Icono.Derecha),
                    -fila.Icono.Y);
                Prender(fila.Indicador.Poder, imagen, true);
                conIcono.Add(fila.Indicador.Poder);
            }

            foreach (var (poder, imagen) in imagenes)
            {
                if (!conIcono.Contains(poder))
                {
                    Prender(poder, imagen, false);
                }
            }
        }
        catch (Exception error)
        {
            apagados = true;
            registro.LogWarning($"Los iconos se apagan hasta el próximo arranque: {error}");
        }
    }

    private void Prender(string poder, Imagen imagen, bool prender)
    {
        if (visibles.TryGetValue(poder, out var antes) && antes == prender)
        {
            return;
        }

        imagen.gameObject.SetActive(prender);
        visibles[poder] = prender;
    }

    private Imagen ImagenDe(string poder)
    {
        if (imagenes.TryGetValue(poder, out var existente) && existente != null)
        {
            return existente;
        }

        var objeto = new GameObject($"Icono {poder}");
        objeto.transform.SetParent(Raiz().transform, false);
        var imagen = objeto.AddComponent(Il2CppType.Of<Imagen>()).Cast<Imagen>();
        imagen.raycastTarget = false;
        imagen.preserveAspect = true;

        var rect = imagen.rectTransform;
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);

        imagenes[poder] = imagen;
        visibles.Remove(poder);
        registro.LogInfo($"Image creado para {poder}.");
        return imagen;
    }

    private GameObject Raiz()
    {
        if (raiz != null)
        {
            return raiz;
        }

        raiz = new GameObject("Temporizadores HUD");
        UnityEngine.Object.DontDestroyOnLoad(raiz);
        var canvas = raiz.AddComponent(Il2CppType.Of<Canvas>()).Cast<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = OrdenDeDibujo;
        registro.LogInfo("Canvas de los iconos creado.");
        return raiz;
    }
}
