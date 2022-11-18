using System;
using UnityEngine;

namespace SDG.Unturned;

public class SleekButtonStateEnum<T> : SleekButtonState where T : struct, Enum
{
    public Action<SleekButtonStateEnum<T>, T> OnSwappedEnum;

    public T GetEnum()
    {
        return (T)Enum.ToObject(typeof(T), base.state);
    }

    public void SetEnum(T value)
    {
        base.state = Convert.ToInt32(value);
    }

    protected override void onClickedState(ISleekElement button)
    {
        base.onClickedState(button);
        OnSwappedEnum(this, GetEnum());
    }

    protected override void onRightClickedState(ISleekElement button)
    {
        base.onRightClickedState(button);
        OnSwappedEnum(this, GetEnum());
    }

    public SleekButtonStateEnum()
    {
        string[] names = Enum.GetNames(typeof(T));
        GUIContent[] array = new GUIContent[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            array[i] = new GUIContent(names[i]);
        }
        setContent(array);
    }
}
