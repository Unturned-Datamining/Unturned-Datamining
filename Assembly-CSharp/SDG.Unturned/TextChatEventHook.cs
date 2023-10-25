using System;
using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

/// <summary>
/// Can be added to any GameObject to receive text chat events.
/// </summary>
[AddComponentMenu("Unturned/Text Chat Event Hook")]
public class TextChatEventHook : MonoBehaviour
{
    public enum EModeFilter
    {
        /// <summary>
        /// Message can be in any chat channel.
        /// </summary>
        Any,
        /// <summary>
        /// Message must be in Global channel.
        /// </summary>
        Global,
        /// <summary>
        /// Message must be in Local channel.
        /// </summary>
        Local
    }

    public enum EPhraseFilter
    {
        /// <summary>
        /// Message must start with phrase text.
        /// </summary>
        StartsWith,
        /// <summary>
        /// Message must contain phrase text.
        /// </summary>
        Contains,
        /// <summary>
        /// Message must end with phrase text.
        /// </summary>
        EndsWith
    }

    /// <summary>
    /// Filter to apply to message type.
    /// </summary>
    public EModeFilter ModeFilter;

    /// <summary>
    /// Sphere radius (squared) around this transform to detect player messages.
    /// e.g. 16 is 4 meters
    /// </summary>
    public float SqrDetectionRadius;

    /// <summary>
    /// Substring to search for in message.
    /// </summary>
    public string Phrase;

    /// <summary>
    /// Filter to apply to message text.
    /// </summary>
    public EPhraseFilter PhraseFilter;

    /// <summary>
    /// Invoked when a player message passes the filters.
    /// </summary>
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
