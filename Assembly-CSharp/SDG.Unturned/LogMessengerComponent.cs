using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to print messages to the log file for debugging.
/// </summary>
[AddComponentMenu("Unturned/Log Messenger")]
public class LogMessengerComponent : MonoBehaviour
{
    /// <summary>
    /// Text to use when PrintInfo is invoked.
    /// </summary>
    public string DefaultText;

    public void PrintInfo(string text)
    {
        UnturnedLog.info(base.transform.GetSceneHierarchyPath() + ": \"" + text + "\"");
    }

    public void PrintDefaultInfo()
    {
        PrintInfo(DefaultText);
    }
}
