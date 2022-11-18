using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Server Text Chat Messenger")]
public class ServerTextChatMessenger : MonoBehaviour
{
    public string DefaultText;

    public string IconURL;

    public Color DefaultColor = Color.white;

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
