using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned;

internal class GlazieruGUITooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
    public string text;

    public Color color = Color.white;

    private bool onStack;

    private static List<GlazieruGUITooltip> activeTooltips = new List<GlazieruGUITooltip>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!onStack)
        {
            onStack = true;
            activeTooltips.Add(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onStack)
        {
            onStack = false;
            activeTooltips.Remove(this);
        }
    }

    public static GlazieruGUITooltip GetTooltip()
    {
        if (activeTooltips.Count > 0)
        {
            return activeTooltips[activeTooltips.Count - 1];
        }
        return null;
    }

    private void OnDisable()
    {
        if (onStack)
        {
            onStack = false;
            activeTooltips.Remove(this);
        }
    }
}
