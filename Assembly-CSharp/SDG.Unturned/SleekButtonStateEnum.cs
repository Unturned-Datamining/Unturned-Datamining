using System;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public class SleekButtonStateEnum<T> : SleekButtonState where T : struct, Enum
{
    public Action<SleekButtonStateEnum<T>, T> OnSwappedEnum;

    private static StringBuilder nameSb = new StringBuilder(32);

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
            string text = names[i];
            bool flag = true;
            string text2 = text;
            for (int j = 0; j < text2.Length; j++)
            {
                if (char.IsLower(text2[j]))
                {
                    flag = false;
                }
            }
            nameSb.Clear();
            if (flag)
            {
                nameSb.Append(text[0]);
                for (int k = 1; k < text.Length; k++)
                {
                    nameSb.Append(char.ToLower(text[k]));
                }
            }
            else
            {
                for (int l = 0; l < text.Length; l++)
                {
                    char c = text[l];
                    if (l > 0 && char.IsUpper(c) && !char.IsUpper(text[l - 1]))
                    {
                        nameSb.Append(' ');
                    }
                    nameSb.Append(c);
                }
            }
            array[i] = new GUIContent(nameSb.ToString());
        }
        setContent(array);
    }
}
