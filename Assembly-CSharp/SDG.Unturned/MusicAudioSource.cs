using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Music Audio Source")]
[RequireComponent(typeof(AudioSource))]
[Tooltip("Reassigns AudioSource's outputAudioMixerGroup to the vanilla Music group")]
public class MusicAudioSource : MonoBehaviour
{
    private void Awake()
    {
        AudioSource component = GetComponent<AudioSource>();
        if (component != null)
        {
            component.outputAudioMixerGroup = UnturnedAudioMixer.GetMusicGroup();
        }
    }
}
