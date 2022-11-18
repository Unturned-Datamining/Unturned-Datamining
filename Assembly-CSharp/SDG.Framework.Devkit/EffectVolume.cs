using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit;

public class EffectVolume : LevelVolume<EffectVolume, EffectVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private EffectVolume volume;

        public Menu(EffectVolume volume)
        {
            this.volume = volume;
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 110;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.sizeOffset_X = 200;
            sleekField.sizeOffset_Y = 30;
            if (volume._effectGuid.IsEmpty())
            {
                sleekField.text = volume._id.ToString();
            }
            else
            {
                sleekField.text = volume._effectGuid.ToString("N");
            }
            sleekField.addLabel("Effect ID", ESleekSide.RIGHT);
            sleekField.onTyped += OnIdChanged;
            AddChild(sleekField);
            ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field.positionOffset_Y = 40;
            sleekFloat32Field.sizeOffset_X = 200;
            sleekFloat32Field.sizeOffset_Y = 30;
            sleekFloat32Field.state = volume.emissionMultiplier;
            sleekFloat32Field.addLabel("Emission Rate", ESleekSide.RIGHT);
            sleekFloat32Field.onTypedSingle += OnEmissionChanged;
            AddChild(sleekFloat32Field);
            ISleekFloat32Field sleekFloat32Field2 = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field2.positionOffset_Y = 80;
            sleekFloat32Field2.sizeOffset_X = 200;
            sleekFloat32Field2.sizeOffset_Y = 30;
            sleekFloat32Field2.state = volume.audioRangeMultiplier;
            sleekFloat32Field2.addLabel("Audio Range", ESleekSide.RIGHT);
            sleekFloat32Field2.onTypedSingle += OnAudioRangeChanged;
            AddChild(sleekFloat32Field2);
        }

        private void OnIdChanged(ISleekField field, string effectIdString)
        {
            if (ushort.TryParse(effectIdString, out volume._id))
            {
                volume._effectGuid = Guid.Empty;
                volume.SyncEffect();
            }
            else if (Guid.TryParse(effectIdString, out volume._effectGuid))
            {
                volume._id = 0;
                volume.SyncEffect();
            }
            else
            {
                volume._effectGuid = Guid.Empty;
                volume._id = 0;
                volume.SyncEffect();
            }
        }

        private void OnEmissionChanged(ISleekFloat32Field field, float value)
        {
            volume.emissionMultiplier = value;
        }

        private void OnAudioRangeChanged(ISleekFloat32Field field, float value)
        {
            volume.audioRangeMultiplier = value;
        }
    }

    [SerializeField]
    internal Guid _effectGuid;

    [SerializeField]
    protected ushort _id;

    [SerializeField]
    protected int maxParticlesBase;

    [SerializeField]
    protected float rateOverTimeBase;

    [SerializeField]
    protected float _emissionMultiplier = 1f;

    [SerializeField]
    protected float _audioRangeMultiplier = 1f;

    protected Transform effect;

    public Guid EffectGuid => _effectGuid;

    public ushort id
    {
        [Obsolete]
        get
        {
            return _id;
        }
        [Obsolete]
        set
        {
            _id = value;
            SyncEffect();
        }
    }

    public float emissionMultiplier
    {
        get
        {
            return _emissionMultiplier;
        }
        set
        {
            _emissionMultiplier = value;
            if (effect != null)
            {
                applyEmission();
            }
        }
    }

    public float audioRangeMultiplier
    {
        get
        {
            return _audioRangeMultiplier;
        }
        set
        {
            _audioRangeMultiplier = value;
            if (effect != null)
            {
                applyAudioRange();
            }
        }
    }

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    private void SyncEffect()
    {
        if (effect != null)
        {
            UnityEngine.Object.Destroy(effect.gameObject);
            effect = null;
        }
        EffectAsset effectAsset = Assets.FindEffectAssetByGuidOrLegacyId(_effectGuid, _id);
        if (effectAsset != null && effectAsset.spawnOnDedicatedServer)
        {
            effect = UnityEngine.Object.Instantiate(effectAsset.effect).transform;
            effect.name = "Effect";
            effect.transform.parent = base.transform;
            effect.transform.localPosition = new Vector3(0f, 0f, 0f);
            effect.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            effect.transform.localScale = new Vector3(1f, 1f, 1f);
            ParticleSystem component = effect.GetComponent<ParticleSystem>();
            if (component != null)
            {
                maxParticlesBase = component.main.maxParticles;
                rateOverTimeBase = component.emission.rateOverTimeMultiplier;
            }
            AudioSource component2 = effect.GetComponent<AudioSource>();
            if (component2 != null && component2.clip != null)
            {
                component2.time = UnityEngine.Random.Range(0f, component2.clip.length);
            }
        }
        if (effect != null)
        {
            applyEmission();
            applyAudioRange();
        }
    }

    protected virtual void applyEmission()
    {
        if (!(effect == null))
        {
            ParticleSystem component = effect.GetComponent<ParticleSystem>();
            if (!(component == null))
            {
                ParticleSystem.MainModule main = component.main;
                main.maxParticles = (int)((float)maxParticlesBase * emissionMultiplier);
                ParticleSystem.EmissionModule emission = component.emission;
                emission.rateOverTimeMultiplier = rateOverTimeBase * emissionMultiplier;
            }
        }
    }

    protected virtual void applyAudioRange()
    {
        if (!(effect == null))
        {
            AudioSource component = effect.GetComponent<AudioSource>();
            if (!(component == null))
            {
                component.maxDistance = audioRangeMultiplier;
            }
        }
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        if (reader.containsKey("Emission"))
        {
            _emissionMultiplier = reader.readValue<float>("Emission");
        }
        if (reader.containsKey("Audio_Range"))
        {
            _audioRangeMultiplier = reader.readValue<float>("Audio_Range");
        }
        string text = reader.readValue("ID");
        if (ushort.TryParse(text, out _id))
        {
            _effectGuid = Guid.Empty;
            SyncEffect();
        }
        else if (Guid.TryParse(text, out _effectGuid))
        {
            _id = 0;
            SyncEffect();
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        if (!_effectGuid.IsEmpty())
        {
            writer.writeValue("ID", _effectGuid);
        }
        else
        {
            writer.writeValue("ID", _id);
        }
        writer.writeValue("Emission", emissionMultiplier);
        writer.writeValue("Audio_Range", audioRangeMultiplier);
    }

    protected override void Awake()
    {
        supportsSphereShape = false;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        effect = base.transform.Find("Effect");
    }
}
