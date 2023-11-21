using UnityEngine;

namespace SDG.Unturned;

public class SleekButtonState : SleekWrapper
{
    private int _state;

    private bool _useContentTooltip;

    public SwappedState onSwappedState;

    internal SleekButtonIcon button;

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

    /// <summary>
    /// If true, button tooltip will be overridden with tooltip from states array.
    /// </summary>
    public bool UseContentTooltip
    {
        get
        {
            return _useContentTooltip;
        }
        set
        {
            _useContentTooltip = value;
            if (_useContentTooltip)
            {
                if (states != null && state >= 0 && state < states.Length && states[state] != null)
                {
                    button.tooltip = states[state].tooltip;
                }
                else
                {
                    button.tooltip = string.Empty;
                }
            }
        }
    }

    private void synchronizeActiveContent()
    {
        if (states != null && state >= 0 && state < states.Length && states[state] != null)
        {
            button.text = states[state].text;
            button.icon = states[state].image as Texture2D;
            if (_useContentTooltip)
            {
                button.tooltip = states[state].tooltip;
            }
        }
        else
        {
            button.text = string.Empty;
            button.icon = null;
            if (_useContentTooltip)
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
        : this(0, newStates)
    {
    }

    public SleekButtonState(int iconSize, params GUIContent[] newStates)
    {
        _state = 0;
        button = new SleekButtonIcon(null, iconSize);
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        AddChild(button);
        if (newStates != null)
        {
            setContent(newStates);
        }
        button.onClickedButton += onClickedState;
        button.onRightClickedButton += onRightClickedState;
    }
}
