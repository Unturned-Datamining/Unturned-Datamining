using UnityEngine;

namespace SDG.Unturned;

public class SleekButtonState : SleekWrapper
{
    private int _state;

    public bool useContentTooltip;

    public SwappedState onSwappedState;

    private SleekButtonIcon button;

    public GUIContent[] states { get; private set; }

    public int state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            synchronizeActiveContent();
        }
    }

    public string tooltip
    {
        get
        {
            return button.tooltip;
        }
        set
        {
            button.tooltip = value;
        }
    }

    public bool isInteractable
    {
        get
        {
            return button.isClickable;
        }
        set
        {
            button.isClickable = value;
        }
    }

    private void synchronizeActiveContent()
    {
        if (states != null && state >= 0 && state < states.Length && states[state] != null)
        {
            button.text = states[state].text;
            button.icon = states[state].image as Texture2D;
            if (useContentTooltip)
            {
                button.tooltip = states[state].tooltip;
            }
        }
        else
        {
            button.text = string.Empty;
            button.icon = null;
            if (useContentTooltip)
            {
                button.tooltip = string.Empty;
            }
        }
    }

    protected virtual void onClickedState(ISleekElement button)
    {
        _state++;
        if (state >= states.Length)
        {
            _state = 0;
        }
        synchronizeActiveContent();
        onSwappedState?.Invoke(this, state);
    }

    protected virtual void onRightClickedState(ISleekElement button)
    {
        _state--;
        if (state < 0)
        {
            _state = states.Length - 1;
        }
        synchronizeActiveContent();
        onSwappedState?.Invoke(this, state);
    }

    public void setContent(params GUIContent[] newStates)
    {
        states = newStates;
        if (state >= states.Length)
        {
            _state = 0;
        }
        synchronizeActiveContent();
    }

    public SleekButtonState(params GUIContent[] newStates)
    {
        _state = 0;
        button = new SleekButtonIcon(null);
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        AddChild(button);
        if (newStates != null)
        {
            setContent(newStates);
        }
        button.onClickedButton += onClickedState;
        button.onRightClickedButton += onRightClickedState;
    }
}
