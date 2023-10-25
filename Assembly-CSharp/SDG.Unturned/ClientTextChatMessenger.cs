using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to send text chat messages from the client, for example to execute commands.
/// </summary>
[AddComponentMenu("Unturned/Client Text Chat Messenger")]
public class ClientTextChatMessenger : MonoBehaviour
{
    public enum EChannel
    {
        /// <summary>
        /// All players on the server will see the message.
        /// </summary>
        Global,
        /// <summary>
        /// Only nearby players will see the message.
        /// </summary>
        Local
    }

    /// <summary>
    /// Text to use when SendDefaultTextChatMessage is invoked.
    /// </summary>
    public string DefaultText;

    /// <summary>
    /// Chat mode to send request in.
    /// </summary>
    public EChannel Channel;

    private EChatMode getChatMode()
    {
        EChannel channel = Channel;
        if (channel == EChannel.Global || channel != EChannel.Local)
        {
            return EChatMode.GLOBAL;
        }
        return EChatMode.LOCAL;
    }

    public void SendTextChatMessage(string text)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            ChatManager.clientSendMessage_UnityEvent(getChatMode(), text, this);
        }
    }

    public void SendDefaultTextChatMessage()
    {
        SendTextChatMessage(DefaultText);
    }
}
