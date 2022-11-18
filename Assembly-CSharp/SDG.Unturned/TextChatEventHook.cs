using System;
using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Text Chat Event Hook")]
public class TextChatEventHook : MonoBehaviour
{
    public enum EModeFilter
    {
        Any,
        Global,
        Local
    }

    public enum EPhraseFilter
    {
        StartsWith,
        Contains,
        EndsWith
    }

    public EModeFilter ModeFilter;

    public float SqrDetectionRadius;

    public string Phrase;

    public EPhraseFilter PhraseFilter;

    public UnityEvent OnTriggered;

    private bool isListening;

    protected bool passesModeFilter(EChatMode mode)
    {
        return ModeFilter switch
        {
            EModeFilter.Global => mode == EChatMode.GLOBAL, 
            EModeFilter.Local => mode == EChatMode.LOCAL, 
            _ => true, 
        };
    }

    protected bool passesPositionFilter(Vector3 playerPosition)
    {
        if (SqrDetectionRadius < 0.01f)
        {
            return true;
        }
        return (playerPosition - base.transform.position).sqrMagnitude < SqrDetectionRadius;
    }

    protected bool passesPhraseFilter(string text)
    {
        return PhraseFilter switch
        {
            EPhraseFilter.StartsWith => text.StartsWith(Phrase, StringComparison.InvariantCultureIgnoreCase), 
            EPhraseFilter.EndsWith => text.EndsWith(Phrase, StringComparison.InvariantCultureIgnoreCase), 
            _ => text.IndexOf(Phrase, StringComparison.InvariantCultureIgnoreCase) >= 0, 
        };
    }

    protected void onChatted(SteamPlayer player, EChatMode mode, ref Color chatted, ref bool isRich, string text, ref bool isVisible)
    {
        if (player != null && !(player.player == null) && !(player.player.transform == null) && passesModeFilter(mode) && passesPositionFilter(player.player.transform.position) && passesPhraseFilter(text))
        {
            OnTriggered.TryInvoke(this);
        }
    }

    protected void OnEnable()
    {
        if (Provider.isServer)
        {
            if (string.IsNullOrWhiteSpace(Phrase))
            {
                UnturnedLog.warn("{0} phrase is empty", base.name);
            }
            else if (!isListening)
            {
                ChatManager.onChatted = (Chatted)Delegate.Combine(ChatManager.onChatted, new Chatted(onChatted));
                isListening = true;
            }
        }
    }

    protected void OnDisable()
    {
        if (isListening)
        {
            ChatManager.onChatted = (Chatted)Delegate.Remove(ChatManager.onChatted, new Chatted(onChatted));
            isListening = false;
        }
    }
}
