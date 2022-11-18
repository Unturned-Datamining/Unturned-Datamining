using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Client Text Chat Messenger")]
public class ClientTextChatMessenger : MonoBehaviour
{
    public enum EChannel
    {
        Global,
        Local
    }

    public string DefaultText;

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
    }

    public void SendDefaultTextChatMessage()
    {
        SendTextChatMessage(DefaultText);
    }
}
