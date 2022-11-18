using UnityEngine;
using UnityEngine.Serialization;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Particle System Collision Audio")]
public class ParticleSystemCollisionAudio : MonoBehaviour
{
    public ParticleSystem particleSystemComponent;

    [FormerlySerializedAs("audioPrefab")]
    public OneShotAudioDefinition audioDef;

    public string materialPropertyName;

    public float speedThreshold = 0.01f;

    public float minSpeed = 0.2f;

    public float maxSpeed = 1f;

    public float minVolume = 0.5f;

    public float maxVolume = 1f;
}
