using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// When moving between physics materials we need to continue any previous tire kickup particles until they expire.
/// This class manages the individual effect per-physics-material. Each wheel can have multiple at once. When the
/// particles have despawned and the effect is no longer needed, the effect game object is returned to the effect
/// pool and this class is returned to <see cref="F:SDG.Unturned.Wheel.motionEffectInstancesPool" />.
/// </summary>
internal class TireMotionEffectInstance
{
    /// <summary>
    /// Name from <see cref="M:SDG.Unturned.PhysicsTool.GetMaterialName(UnityEngine.Vector3,UnityEngine.Transform,UnityEngine.Collider)" />.
    /// </summary>
    public string materialName;

    /// <summary>
    /// Instantiated effect. Null after returning to pool.
    /// </summary>
    private GameObject gameObject;

    /// <summary>
    /// Effect's transform. Null after returning to pool.
    /// </summary>
    public Transform transform;

    /// <summary>
    /// Component on gameObject. Null after returning to pool.
    /// </summary>
    public ParticleSystem particleSystem;

    /// <summary>
    /// Whether this effect should be emitting particles. False stops the particle system immediately, whereas true
    /// only starts playing on the next frame to avoid filling a gap between positions, e.g., after a jump.
    /// </summary>
    public bool isReadyToPlay;

    /// <summary>
    /// Prevents repeated lookups if asset is null, while allowing asset to be looked up each time this effect
    /// becomes active so that it can be iterated on without restarting the game.
    /// </summary>
    public bool hasTriedToInstantiateEffect;

    public void StopParticleSystem()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
        isReadyToPlay = false;
    }

    public void DestroyEffect()
    {
        if (gameObject != null)
        {
            EffectManager.DestroyIntoPool(gameObject);
            gameObject = null;
            transform = null;
            particleSystem = null;
            isReadyToPlay = false;
        }
    }

    public void InstantiateEffect()
    {
        hasTriedToInstantiateEffect = true;
        EffectAsset effectAsset = PhysicMaterialCustomData.GetTireMotionEffect(materialName).Find();
        if (effectAsset != null && effectAsset.effect != null)
        {
            gameObject = EffectManager.InstantiateFromPool(effectAsset);
            transform = gameObject.transform;
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            isReadyToPlay = false;
        }
    }

    public void Reset()
    {
        gameObject = null;
        transform = null;
        particleSystem = null;
        isReadyToPlay = false;
        hasTriedToInstantiateEffect = false;
    }
}
