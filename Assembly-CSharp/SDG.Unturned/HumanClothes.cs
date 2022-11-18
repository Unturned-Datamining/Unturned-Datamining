using System;
using UnityEngine;

namespace SDG.Unturned;

public class HumanClothes : MonoBehaviour
{
    private static Shader shader;

    private static Shader clothingShader;

    private Mesh[] humanMeshes;

    private Material materialClothing;

    private Material materialHair;

    private Transform spine;

    private Transform skull;

    private Transform[] upperBones;

    private Transform[] upperSystems;

    private Transform[] lowerBones;

    private Transform[] lowerSystems;

    public bool isMine;

    public bool isView;

    public bool canWearPro;

    public bool isRagdoll;

    private SkinnedMeshRenderer[] characterMeshRenderers;

    private bool _isVisual = true;

    private bool _isMythic = true;

    private bool _isLeftHanded;

    private bool _hasBackpack = true;

    private bool isUpper;

    private bool isLower;

    private ItemShirtAsset visualShirtAsset;

    private ItemPantsAsset visualPantsAsset;

    private ItemHatAsset visualHatAsset;

    private ItemBackpackAsset visualBackpackAsset;

    private ItemVestAsset visualVestAsset;

    private ItemMaskAsset visualMaskAsset;

    private ItemGlassesAsset visualGlassesAsset;

    private int _visualShirt;

    private int _visualPants;

    private int _visualHat;

    public int _visualBackpack;

    public int _visualVest;

    public int _visualMask;

    public int _visualGlasses;

    private ItemShirtAsset _shirtAsset;

    private ItemPantsAsset _pantsAsset;

    private ItemHatAsset _hatAsset;

    private ItemBackpackAsset _backpackAsset;

    private ItemVestAsset _vestAsset;

    private ItemMaskAsset _maskAsset;

    private ItemGlassesAsset _glassesAsset;

    private byte _face = byte.MaxValue;

    private byte _hair;

    private byte _beard;

    private Color _skinColor;

    private Color _hairColor;

    private bool hasHair;

    private bool hasBeard;

    private bool usingHumanMeshes = true;

    private bool usingHumanMaterials = true;

    private bool hairDirty;

    private bool beardDirty;

    private bool skinColorDirty;

    private bool faceDirty;

    private bool shirtDirty;

    private bool pantsDirty;

    private bool hatDirty;

    private bool backpackDirty;

    private bool vestDirty;

    private bool maskDirty;

    private bool glassesDirty;

    internal static readonly int skinColorPropertyID = Shader.PropertyToID("_SkinColor");

    internal static readonly int flipShirtPropertyID = Shader.PropertyToID("_FlipShirt");

    internal static readonly int faceAlbedoTexturePropertyID = Shader.PropertyToID("_FaceAlbedoTexture");

    internal static readonly int faceEmissionTexturePropertyID = Shader.PropertyToID("_FaceEmissionTexture");

    internal static readonly int shirtAlbedoTexturePropertyID = Shader.PropertyToID("_ShirtAlbedoTexture");

    internal static readonly int shirtEmissionTexturePropertyID = Shader.PropertyToID("_ShirtEmissionTexture");

    internal static readonly int shirtMetallicTexturePropertyID = Shader.PropertyToID("_ShirtMetallicTexture");

    internal static readonly int pantsAlbedoTexturePropertyID = Shader.PropertyToID("_PantsAlbedoTexture");

    internal static readonly int pantsEmissionTexturePropertyID = Shader.PropertyToID("_PantsEmissionTexture");

    internal static readonly int pantsMetallicTexturePropertyID = Shader.PropertyToID("_PantsMetallicTexture");

    public Transform hatModel { get; private set; }

    public Transform backpackModel { get; private set; }

    public Transform vestModel { get; private set; }

    public Transform maskModel { get; private set; }

    public Transform glassesModel { get; private set; }

    public Transform hairModel { get; private set; }

    public Transform beardModel { get; private set; }

    public bool isVisual
    {
        get
        {
            return _isVisual;
        }
        set
        {
            if (isVisual != value)
            {
                _isVisual = value;
                markAllDirty(isDirty: true);
            }
        }
    }

    public bool isMythic
    {
        get
        {
            return _isMythic;
        }
        set
        {
            if (isMythic != value)
            {
                _isMythic = value;
                markAllDirty(isDirty: true);
            }
        }
    }

    public bool hand
    {
        get
        {
            return _isLeftHanded;
        }
        set
        {
            if (_isLeftHanded != value)
            {
                _isLeftHanded = value;
                markAllDirty(isDirty: true);
            }
        }
    }

    public bool hasBackpack
    {
        get
        {
            return _hasBackpack;
        }
        set
        {
            if (value != _hasBackpack)
            {
                _hasBackpack = value;
                if (backpackModel != null)
                {
                    backpackModel.gameObject.SetActive(hasBackpack);
                }
            }
        }
    }

    public int visualShirt
    {
        get
        {
            return _visualShirt;
        }
        set
        {
            if (visualShirt != value)
            {
                _visualShirt = value;
            }
        }
    }

    public int visualPants
    {
        get
        {
            return _visualPants;
        }
        set
        {
            if (visualPants != value)
            {
                _visualPants = value;
            }
        }
    }

    public int visualHat
    {
        get
        {
            return _visualHat;
        }
        set
        {
            if (visualHat != value)
            {
                _visualHat = value;
            }
        }
    }

    public int visualBackpack
    {
        get
        {
            return _visualBackpack;
        }
        set
        {
            if (visualBackpack != value)
            {
                _visualBackpack = value;
            }
        }
    }

    public int visualVest
    {
        get
        {
            return _visualVest;
        }
        set
        {
            if (visualVest != value)
            {
                _visualVest = value;
            }
        }
    }

    public int visualMask
    {
        get
        {
            return _visualMask;
        }
        set
        {
            if (visualMask != value)
            {
                _visualMask = value;
            }
        }
    }

    public int visualGlasses
    {
        get
        {
            return _visualGlasses;
        }
        set
        {
            if (visualGlasses != value)
            {
                _visualGlasses = value;
            }
        }
    }

    public ItemShirtAsset shirtAsset
    {
        get
        {
            return _shirtAsset;
        }
        internal set
        {
            _shirtAsset = value;
            shirtDirty = true;
        }
    }

    public ItemPantsAsset pantsAsset
    {
        get
        {
            return _pantsAsset;
        }
        internal set
        {
            _pantsAsset = value;
            pantsDirty = true;
        }
    }

    public ItemHatAsset hatAsset
    {
        get
        {
            return _hatAsset;
        }
        internal set
        {
            _hatAsset = value;
            hatDirty = true;
        }
    }

    public ItemBackpackAsset backpackAsset
    {
        get
        {
            return _backpackAsset;
        }
        internal set
        {
            _backpackAsset = value;
            backpackDirty = true;
        }
    }

    public ItemVestAsset vestAsset
    {
        get
        {
            return _vestAsset;
        }
        internal set
        {
            _vestAsset = value;
            vestDirty = true;
        }
    }

    public ItemMaskAsset maskAsset
    {
        get
        {
            return _maskAsset;
        }
        internal set
        {
            _maskAsset = value;
            maskDirty = true;
        }
    }

    public ItemGlassesAsset glassesAsset
    {
        get
        {
            return _glassesAsset;
        }
        internal set
        {
            _glassesAsset = value;
            glassesDirty = true;
        }
    }

    public Guid shirtGuid
    {
        get
        {
            return _shirtAsset?.GUID ?? Guid.Empty;
        }
        set
        {
            _shirtAsset = Assets.find(value) as ItemShirtAsset;
            shirtDirty = true;
        }
    }

    public ushort shirt
    {
        get
        {
            return _shirtAsset?.id ?? 0;
        }
        set
        {
            _shirtAsset = Assets.find(EAssetType.ITEM, value) as ItemShirtAsset;
            shirtDirty = true;
        }
    }

    public Guid pantsGuid
    {
        get
        {
            return _pantsAsset?.GUID ?? Guid.Empty;
        }
        set
        {
            _pantsAsset = Assets.find(value) as ItemPantsAsset;
            pantsDirty = true;
        }
    }

    public ushort pants
    {
        get
        {
            return _pantsAsset?.id ?? 0;
        }
        set
        {
            _pantsAsset = Assets.find(EAssetType.ITEM, value) as ItemPantsAsset;
            pantsDirty = true;
        }
    }

    public Guid hatGuid
    {
        get
        {
            return _hatAsset?.GUID ?? Guid.Empty;
        }
        set
        {
            _hatAsset = Assets.find(value) as ItemHatAsset;
            hatDirty = true;
        }
    }

    public ushort hat
    {
        get
        {
            return _hatAsset?.id ?? 0;
        }
        set
        {
            _hatAsset = Assets.find(EAssetType.ITEM, value) as ItemHatAsset;
            hatDirty = true;
        }
    }

    public Guid backpackGuid
    {
        get
        {
            return _backpackAsset?.GUID ?? Guid.Empty;
        }
        set
        {
            _backpackAsset = Assets.find(value) as ItemBackpackAsset;
            backpackDirty = true;
        }
    }

    public ushort backpack
    {
        get
        {
            return _backpackAsset?.id ?? 0;
        }
        set
        {
            _backpackAsset = Assets.find(EAssetType.ITEM, value) as ItemBackpackAsset;
            backpackDirty = true;
        }
    }

    public Guid vestGuid
    {
        get
        {
            return _vestAsset?.GUID ?? Guid.Empty;
        }
        set
        {
            _vestAsset = Assets.find(value) as ItemVestAsset;
            vestDirty = true;
        }
    }

    public ushort vest
    {
        get
        {
            return _vestAsset?.id ?? 0;
        }
        set
        {
            _vestAsset = Assets.find(EAssetType.ITEM, value) as ItemVestAsset;
            vestDirty = true;
        }
    }

    public Guid maskGuid
    {
        get
        {
            return _maskAsset?.GUID ?? Guid.Empty;
        }
        set
        {
            _maskAsset = Assets.find(value) as ItemMaskAsset;
            maskDirty = true;
        }
    }

    public ushort mask
    {
        get
        {
            return _maskAsset?.id ?? 0;
        }
        set
        {
            _maskAsset = Assets.find(EAssetType.ITEM, value) as ItemMaskAsset;
            maskDirty = true;
        }
    }

    public Guid glassesGuid
    {
        get
        {
            return _glassesAsset?.GUID ?? Guid.Empty;
        }
        set
        {
            _glassesAsset = Assets.find(value) as ItemGlassesAsset;
            glassesDirty = true;
        }
    }

    public ushort glasses
    {
        get
        {
            return _glassesAsset?.id ?? 0;
        }
        set
        {
            _glassesAsset = Assets.find(EAssetType.ITEM, value) as ItemGlassesAsset;
            glassesDirty = true;
        }
    }

    public byte face
    {
        get
        {
            return _face;
        }
        set
        {
            if (face != value)
            {
                _face = value;
                faceDirty = true;
            }
        }
    }

    public byte hair
    {
        get
        {
            return _hair;
        }
        set
        {
            if (hair != value)
            {
                _hair = value;
                hairDirty = true;
            }
        }
    }

    public byte beard
    {
        get
        {
            return _beard;
        }
        set
        {
            if (beard != value)
            {
                _beard = value;
                beardDirty = true;
            }
        }
    }

    public Color skin
    {
        get
        {
            return _skinColor;
        }
        set
        {
            _skinColor = value;
            skinColorDirty = true;
        }
    }

    public Color color
    {
        get
        {
            return _hairColor;
        }
        set
        {
            _hairColor = value;
        }
    }

    private void markAllDirty(bool isDirty)
    {
        hairDirty = isDirty;
        beardDirty = isDirty;
        skinColorDirty = isDirty;
        faceDirty = isDirty;
        shirtDirty = isDirty;
        pantsDirty = isDirty;
        hatDirty = isDirty;
        backpackDirty = isDirty;
        vestDirty = isDirty;
        maskDirty = isDirty;
        glassesDirty = isDirty;
    }

    private void applyHairOverride(ItemGearAsset itemAsset, Transform rootModel)
    {
        if (string.IsNullOrEmpty(itemAsset.hairOverride))
        {
            return;
        }
        Transform transform = rootModel.FindChildRecursive(itemAsset.hairOverride);
        if (transform == null)
        {
            Assets.reportError(itemAsset, "cannot find hair override '{0}'", itemAsset.hairOverride);
            return;
        }
        Renderer component = transform.GetComponent<Renderer>();
        if (component != null)
        {
            component.sharedMaterial = materialHair;
        }
    }

    public void apply()
    {
    }

    private void centerHeadEffect(Transform skull, Transform model)
    {
        Transform transform = model.Find("Effect");
        if (transform == null)
        {
            transform = new GameObject("Effect").transform;
            transform.parent = model;
            transform.localPosition = new Vector3(-0.45f, 0f, 0f);
            transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            transform.localScale = Vector3.one;
        }
        else
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.y = 0f;
            localPosition.z = 0f;
            transform.localPosition = localPosition;
        }
    }

    private void setCharacterMeshes(Mesh[] meshes)
    {
        SkinnedMeshRenderer[] array;
        if (meshes == null || meshes.Length < 1)
        {
            array = characterMeshRenderers;
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
            {
                if (!(skinnedMeshRenderer == null))
                {
                    skinnedMeshRenderer.sharedMesh = null;
                }
            }
            return;
        }
        int num = 0;
        array = characterMeshRenderers;
        foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in array)
        {
            if (!(skinnedMeshRenderer2 == null))
            {
                if (num < meshes.Length)
                {
                    skinnedMeshRenderer2.sharedMesh = meshes[num];
                }
                else
                {
                    skinnedMeshRenderer2.sharedMesh = meshes[meshes.Length - 1];
                }
                num++;
            }
        }
    }

    private void setCharacterMaterial(Material material)
    {
        SkinnedMeshRenderer[] array = characterMeshRenderers;
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
        {
            if (!(skinnedMeshRenderer == null))
            {
                skinnedMeshRenderer.sharedMaterial = material;
            }
        }
    }

    private void Awake()
    {
        spine = base.transform.Find("Skeleton").Find("Spine");
        skull = spine.Find("Skull");
        upperBones = new Transform[5]
        {
            spine,
            spine.Find("Left_Shoulder/Left_Arm"),
            spine.Find("Left_Shoulder/Left_Arm/Left_Hand"),
            spine.Find("Right_Shoulder/Right_Arm"),
            spine.Find("Right_Shoulder/Right_Arm/Right_Hand")
        };
        upperSystems = new Transform[upperBones.Length];
        lowerBones = new Transform[4]
        {
            spine.parent.Find("Left_Hip/Left_Leg"),
            spine.parent.Find("Left_Hip/Left_Leg/Left_Foot"),
            spine.parent.Find("Right_Hip/Right_Leg"),
            spine.parent.Find("Right_Hip/Right_Leg/Right_Foot")
        };
        lowerSystems = new Transform[lowerBones.Length];
        Transform obj = base.transform.Find("Model_0");
        Transform transform = base.transform.Find("Model_1");
        characterMeshRenderers = new SkinnedMeshRenderer[(transform == null) ? 1 : 2];
        if (obj != null)
        {
            characterMeshRenderers[0] = base.transform.Find("Model_0").GetComponent<SkinnedMeshRenderer>();
        }
        if (transform != null)
        {
            characterMeshRenderers[1] = base.transform.Find("Model_1").GetComponent<SkinnedMeshRenderer>();
        }
        setCharacterMaterial(materialClothing);
        markAllDirty(isDirty: true);
    }

    private void OnDestroy()
    {
        if (materialClothing != null)
        {
            UnityEngine.Object.DestroyImmediate(materialClothing);
            materialClothing = null;
        }
        if (materialHair != null)
        {
            UnityEngine.Object.DestroyImmediate(materialHair);
            materialHair = null;
        }
    }
}
