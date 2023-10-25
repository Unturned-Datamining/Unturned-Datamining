using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to broadcast text chat messages from the server.
/// </summary>
[AddComponentMenu("Unturned/Server Text Chat Messenger")]
public class ServerTextChatMessenger : MonoBehaviour
{
    /// <summary>
    /// Text to use when SendDefaultTextChatMessage is invoked.
    /// </summary>
    public string DefaultText;

    /// <summary>
    /// URL of a png or jpg image file to show next to the message.
    /// </summary>
    public string IconURL;

    /// <summary>
    /// Text color when rich text does not override with color tags.
    /// </summary>
    public Color DefaultColor = Color.white;

    /// <summary>
    /// Should rich text tags be parsed?
    /// e.g. bold, italic, color
    /// </summary>
    public bool UseRichTextFormatting;

    public void SendTextChatMessage(string text)
    {
        if (Provider.isServer)
        {
            ChatManager.serverSendMessage_UnityEvent(text, DefaultColor, IconURL, UseRichTextFormatting, this);
        }
    }

    public void SendDefaultTextChatMessage()
    {
        SendTextChatMessage(DefaultText);
    }

    public void ExecuteTextChatCommand(string command)
    {
        Commander.execute_UnityEvent(command, this);
    }

    public void ExecuteDefaultTextChatCommand()
    {
        ExecuteTextChatCommand(DefaultText);
    }
}
