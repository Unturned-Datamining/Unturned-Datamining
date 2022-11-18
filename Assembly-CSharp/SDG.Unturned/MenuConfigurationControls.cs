using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationControls : MonoBehaviour
{
    private static byte _binding;

    public static byte binding
    {
        get
        {
            return _binding;
        }
        set
        {
            _binding = value;
        }
    }

    private static void cancel()
    {
        MenuConfigurationControlsUI.cancel();
        binding = byte.MaxValue;
    }

    private static void bind(KeyCode key)
    {
        MenuConfigurationControlsUI.bind(key);
        binding = byte.MaxValue;
    }

    private void Update()
    {
        if (binding == byte.MaxValue)
        {
            return;
        }
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Escape)
            {
                cancel();
            }
            else
            {
                bind(Event.current.keyCode);
            }
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            if (Event.current.button == 0)
            {
                bind(KeyCode.Mouse0);
            }
            else if (Event.current.button == 1)
            {
                bind(KeyCode.Mouse1);
            }
            else if (Event.current.button == 2)
            {
                bind(KeyCode.Mouse2);
            }
            else if (Event.current.button == 3)
            {
                bind(KeyCode.Mouse3);
            }
            else if (Event.current.button == 4)
            {
                bind(KeyCode.Mouse4);
            }
            else if (Event.current.button == 5)
            {
                bind(KeyCode.Mouse5);
            }
            else if (Event.current.button == 6)
            {
                bind(KeyCode.Mouse6);
            }
        }
        else if (Event.current.shift)
        {
            bind(KeyCode.LeftShift);
        }
    }

    private void Awake()
    {
        binding = byte.MaxValue;
    }
}
