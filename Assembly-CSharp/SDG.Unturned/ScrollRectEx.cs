using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class ScrollRectEx : ScrollRect
{
    [SerializeField]
    public bool HandleScrollWheel = true;

    public override void OnScroll(PointerEventData data)
    {
        if (HandleScrollWheel)
        {
            base.OnScroll(data);
        }
        else if (base.transform.parent != null)
        {
            ScrollRect componentInParent = base.transform.parent.GetComponentInParent<ScrollRect>();
            if (componentInParent != null)
            {
                componentInParent.OnScroll(data);
            }
        }
    }
}
