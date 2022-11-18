using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class ButtonEx : Button
{
    public ButtonClickedEvent onRightClick = new ButtonClickedEvent();

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (IsActive() && IsInteractable())
            {
                onRightClick.Invoke();
            }
        }
        else
        {
            base.OnPointerClick(eventData);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        PointerEventData.InputButton button = eventData.button;
        eventData.button = PointerEventData.InputButton.Left;
        base.OnPointerDown(eventData);
        eventData.button = button;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        PointerEventData.InputButton button = eventData.button;
        eventData.button = PointerEventData.InputButton.Left;
        base.OnPointerUp(eventData);
        eventData.button = button;
    }
}
