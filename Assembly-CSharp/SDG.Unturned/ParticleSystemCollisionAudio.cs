using System;
using System.Collections.Generic;
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

    [NonSerialized]
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    [NonSerialized]
    private List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>();

    private void OnParticleCollision(GameObject other)
    {
        if (particleSystemComponent == null || other == null || (audioDef == null && string.IsNullOrEmpty(materialPropertyName)))
        {
            return;
        }
        float num = speedThreshold * speedThreshold;
        int num2 = particleSystemComponent.GetCollisionEvents(other, collisionEvents);
        for (int i = 0; i < num2; i++)
        {
            ParticleCollisionEvent particleCollisionEvent = collisionEvents[i];
            float sqrMagnitude = particleCollisionEvent.velocity.sqrMagnitude;
            if (sqrMagnitude < num)
            {
                continue;
            }
            float value = Mathf.Sqrt(sqrMagnitude);
            float t = Mathf.InverseLerp(minSpeed, maxSpeed, value);
            float num3 = Mathf.Lerp(minVolume, maxVolume, t);
            OneShotAudioDefinition oneShotAudioDefinition;
            if (string.IsNullOrEmpty(materialPropertyName))
            {
                oneShotAudioDefinition = audioDef;
            }
            else
            {
                string text = ((!particleCollisionEvent.colliderComponent.transform.CompareTag("Ground")) ? (particleCollisionEvent.colliderComponent as Collider)?.sharedMaterial?.name : PhysicsTool.GetTerrainMaterialName(particleCollisionEvent.intersection));
                if (!string.IsNullOrEmpty(text))
                {
                    oneShotAudioDefinition = PhysicMaterialCustomData.GetAudioDef(text, materialPropertyName);
                    if (oneShotAudioDefinition == null)
                    {
                        oneShotAudioDefinition = audioDef;
                    }
                }
                else
                {
                    oneShotAudioDefinition = audioDef;
                }
                if (oneShotAudioDefinition == null)
                {
                    continue;
                }
            }
            AudioClip randomClip = oneShotAudioDefinition.GetRandomClip();
            if (!(randomClip == null))
            {
                OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(particleCollisionEvent.intersection, randomClip);
                oneShotAudioParameters.volume = num3 * oneShotAudioDefinition.volumeMultiplier;
                oneShotAudioParameters.RandomizePitch(oneShotAudioDefinition.minPitch, oneShotAudioDefinition.maxPitch);
                oneShotAudioParameters.Play();
            }
        }
    }

    private void OnParticleTrigger()
    {
        if (particleSystemComponent == null)
        {
            return;
        }
        float num = speedThreshold * speedThreshold;
        int triggerParticles = particleSystemComponent.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles);
        for (int i = 0; i < triggerParticles; i++)
        {
            ParticleSystem.Particle particle = enterParticles[i];
            float sqrMagnitude = particle.velocity.sqrMagnitude;
            if (sqrMagnitude < num)
            {
                continue;
            }
            float value = Mathf.Sqrt(sqrMagnitude);
            float t = Mathf.InverseLerp(minSpeed, maxSpeed, value);
            float num2 = Mathf.Lerp(minVolume, maxVolume, t);
            OneShotAudioDefinition oneShotAudioDefinition = PhysicMaterialCustomData.GetAudioDef("Water", materialPropertyName);
            if (!(oneShotAudioDefinition == null))
            {
                AudioClip randomClip = oneShotAudioDefinition.GetRandomClip();
                if (!(randomClip == null))
                {
                    OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(particle.position, randomClip);
                    oneShotAudioParameters.volume = num2 * oneShotAudioDefinition.volumeMultiplier;
                    oneShotAudioParameters.RandomizePitch(oneShotAudioDefinition.minPitch, oneShotAudioDefinition.maxPitch);
                    oneShotAudioParameters.Play();
                }
            }
        }
    }
}
