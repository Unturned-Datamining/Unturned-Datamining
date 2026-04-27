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

    private Material materialBeard;

    /// <summary>
    /// For non-gold players' hairOverride and beardOverride cosmetics default color.
    /// Worst case scenario is 3 hair overrides and 3 beard overrides.
    /// </summary>
    private Material[] extraHairOverrideMaterials;

    private Transform spine;

    private Transform skull;

    private Transform[] upperBones;

    private MythicalEffectController[] upperSystems;

    private Transform[] lowerBones;

    private MythicalEffectController[] lowerSystems;

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

    /// <summary>
    /// If true, this character is for capturing clothing icons.
    /// </summary>
    internal bool isCosmeticPreview;

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

    public bool ShouldHairOverridesUseFallbackColor { get; set; }

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
            if (visualShirt == value)
            {
                return;
            }
            _visualShirt = value;
            if (Dedicator.IsDedicatedServer)
            {
                return;
            }
            if (visualShirt != 0)
            {
                try
                {
                    visualShirtAsset = Assets.find<ItemShirtAsset>(Provider.provider.economyService.getInventoryItemGuid(visualShirt));
                }
                catch
                {
                    visualShirtAsset = null;
                }
                if (visualShirtAsset != null && !visualShirtAsset.isPro)
                {
                    _visualShirt = 0;
                    visualShirtAsset = null;
                }
            }
            else
            {
                visualShirtAsset = null;
            }
            shirtDirty = true;
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
            if (visualPants == value)
            {
                return;
            }
            _visualPants = value;
            if (Dedicator.IsDedicatedServer)
            {
                return;
            }
            if (visualPants != 0)
            {
                try
                {
                    visualPantsAsset = Assets.find<ItemPantsAsset>(Provider.provider.economyService.getInventoryItemGuid(visualPants));
                }
                catch
                {
                    visualPantsAsset = null;
                }
                if (visualPantsAsset != null && !visualPantsAsset.isPro)
                {
                    _visualPants = 0;
                    visualPantsAsset = null;
                }
            }
            else
            {
                visualPantsAsset = null;
            }
            pantsDirty = true;
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
            if (visualHat == value)
            {
                return;
            }
            _visualHat = value;
            if (Dedicator.IsDedicatedServer)
            {
                return;
            }
            if (visualHat != 0)
            {
                try
                {
                    visualHatAsset = Assets.find<ItemHatAsset>(Provider.provider.economyService.getInventoryItemGuid(visualHat));
                }
                catch
                {
                    visualHatAsset = null;
                }
                if (visualHatAsset != null && !visualHatAsset.isPro)
                {
                    _visualHat = 0;
                    visualHatAsset = null;
                }
            }
            else
            {
                visualHatAsset = null;
            }
            hatDirty = true;
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
            if (visualBackpack == value)
            {
                return;
            }
            _visualBackpack = value;
            if (Dedicator.IsDedicatedServer)
            {
                return;
            }
            if (visualBackpack != 0)
            {
                try
                {
                    visualBackpackAsset = Assets.find<ItemBackpackAsset>(Provider.provider.economyService.getInventoryItemGuid(visualBackpack));
                }
                catch
                {
                    visualBackpackAsset = null;
                }
                if (visualBackpackAsset != null && !visualBackpackAsset.isPro)
                {
                    _visualBackpack = 0;
                    visualBackpackAsset = null;
                }
            }
            else
            {
                visualBackpackAsset = null;
            }
            backpackDirty = true;
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
            if (visualVest == value)
            {
                return;
            }
            _visualVest = value;
            if (Dedicator.IsDedicatedServer)
            {
                return;
            }
            bool flag = visualVestAsset?.hasFallbackShirt ?? false;
            if (visualVest != 0)
            {
                try
                {
                    visualVestAsset = Assets.find<ItemVestAsset>(Provider.provider.economyService.getInventoryItemGuid(visualVest));
                }
                catch
                {
                    visualVestAsset = null;
                }
                if (visualVestAsset != null && !visualVestAsset.isPro)
                {
                    _visualVest = 0;
                    visualVestAsset = null;
                }
            }
            else
            {
                visualVestAsset = null;
            }
            vestDirty = true;
            bool flag2 = visualVestAsset?.hasFallbackShirt ?? false;
            shirtDirty |= flag2 != flag;
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
            if (visualMask == value)
            {
                return;
            }
            _visualMask = value;
            if (Dedicator.IsDedicatedServer)
            {
                return;
            }
            if (visualMask != 0)
            {
                try
                {
                    visualMaskAsset = Assets.find<ItemMaskAsset>(Provider.provider.economyService.getInventoryItemGuid(visualMask));
                }
                catch
                {
                    visualMaskAsset = null;
                }
                if (visualMaskAsset != null && !visualMaskAsset.isPro)
                {
                    _visualMask = 0;
                    visualMaskAsset = null;
                }
            }
            else
            {
                visualMaskAsset = null;
            }
            maskDirty = true;
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
            if (visualGlasses == value)
            {
                return;
            }
            _visualGlasses = value;
            if (Dedicator.IsDedicatedServer)
            {
                return;
            }
            if (visualGlasses != 0)
            {
                try
                {
                    visualGlassesAsset = Assets.find<ItemGlassesAsset>(Provider.provider.economyService.getInventoryItemGuid(visualGlasses));
                }
                catch
                {
                    visualGlassesAsset = null;
                }
                if (visualGlassesAsset != null && !visualGlassesAsset.isPro)
                {
                    _visualGlasses = 0;
                    visualGlassesAsset = null;
                }
            }
            else
            {
                visualGlassesAsset = null;
            }
            glassesDirty = true;
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
            bool flag = _vestAsset?.hasFallbackShirt ?? false;
            _vestAsset = value;
            vestDirty = true;
            bool flag2 = _vestAsset?.hasFallbackShirt ?? false;
            shirtDirty |= flag2 != flag;
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
            vestAsset = Assets.find(value) as ItemVestAsset;
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
            vestAsset = Assets.find(EAssetType.ITEM, value) as ItemVestAsset;
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

    public Color BeardColor { get; set; }

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

    private Material GetHairOverrideMaterialAtIndex(int index)
    {
        if (extraHairOverrideMaterials == null)
        {
            extraHairOverrideMaterials = new Material[6];
        }
        Material material = extraHairOverrideMaterials[index];
        if (material == null)
        {
            material = new Material(shader);
            material.name = $"ExtraHair_{index}";
            material.hideFlags = HideFlags.HideAndDontSave;
            material.SetFloat("_Glossiness", 0f);
            material.SetColor("_SpecColor", Color.black);
            extraHairOverrideMaterials[index] = material;
        }
        return material;
    }

    private int GetHairOverrideMaterialIndex(ItemGearAsset itemAsset, bool isBeard)
    {
        int num = ((!isBeard) ? 3 : 0);
        return itemAsset.type switch
        {
            EItemType.GLASSES => num + 1, 
            EItemType.MASK => num + 2, 
            _ => num, 
        };
    }

    private Material GetHairOverrideMaterial(ItemGearAsset itemAsset)
    {
        if (!ShouldHairOverridesUseFallbackColor || !itemAsset.hairOverrideNonGoldColor.HasValue)
        {
            return materialHair;
        }
        int hairOverrideMaterialIndex = GetHairOverrideMaterialIndex(itemAsset, isBeard: false);
        Material hairOverrideMaterialAtIndex = GetHairOverrideMaterialAtIndex(hairOverrideMaterialIndex);
        hairOverrideMaterialAtIndex.color = itemAsset.hairOverrideNonGoldColor.Value;
        return hairOverrideMaterialAtIndex;
    }

    private Material GetBeardOverrideMaterial(ItemGearAsset itemAsset)
    {
        if (!ShouldHairOverridesUseFallbackColor || !itemAsset.beardOverrideNonGoldColor.HasValue)
        {
            return materialBeard;
        }
        int hairOverrideMaterialIndex = GetHairOverrideMaterialIndex(itemAsset, isBeard: true);
        Material hairOverrideMaterialAtIndex = GetHairOverrideMaterialAtIndex(hairOverrideMaterialIndex);
        hairOverrideMaterialAtIndex.color = itemAsset.beardOverrideNonGoldColor.Value;
        return hairOverrideMaterialAtIndex;
    }

    private void ApplyHairOverride(ItemGearAsset itemAsset, Transform rootModel)
    {
        if (string.IsNullOrEmpty(itemAsset.hairOverride))
        {
            return;
        }
        Transform transform = rootModel.FindChildRecursive(itemAsset.hairOverride);
        if (transform == null)
        {
            Assets.ReportError(itemAsset, "cannot find hair override \"{0}\"", itemAsset.hairOverride);
            return;
        }
        Renderer component = transform.GetComponent<Renderer>();
        if (component != null)
        {
            component.sharedMaterial = GetHairOverrideMaterial(itemAsset);
        }
        else
        {
            Assets.ReportError(itemAsset, "hair override \"{0}\" does not have a renderer component", itemAsset.hairOverride);
        }
    }

    private void ApplyBeardOverride(ItemGearAsset itemAsset, Transform rootModel)
    {
        if (string.IsNullOrEmpty(itemAsset.BeardOverride))
        {
            return;
        }
        Transform transform = rootModel.FindChildRecursive(itemAsset.BeardOverride);
        if (transform == null)
        {
            Assets.ReportError(itemAsset, "cannot find beard override \"{0}\"", itemAsset.hairOverride);
            return;
        }
        Renderer component = transform.GetComponent<Renderer>();
        if (component != null)
        {
            component.sharedMaterial = GetBeardOverrideMaterial(itemAsset);
        }
        else
        {
            Assets.ReportError(itemAsset, "beard override \"{0}\" does not have a renderer component", itemAsset.BeardOverride);
        }
    }

    private void ApplySkinOverride(ItemClothingAsset itemAsset, Transform rootModel)
    {
        if (string.IsNullOrEmpty(itemAsset.skinOverride))
        {
            return;
        }
        Transform transform = rootModel.FindChildRecursive(itemAsset.skinOverride);
        if (transform == null)
        {
            Assets.ReportError(itemAsset, "cannot find skin override \"{0}\"", itemAsset.skinOverride);
            return;
        }
        Renderer component = transform.GetComponent<Renderer>();
        if (component != null)
        {
            component.sharedMaterial = materialClothing;
        }
        else
        {
            Assets.ReportError(itemAsset, "skin override \"{0}\" does not have a renderer component", itemAsset.skinOverride);
        }
    }

    public void apply()
    {
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        if (_shirtAsset != null && _shirtAsset.isPro && !canWearPro)
        {
            _shirtAsset = null;
            shirtDirty = true;
        }
        if (_pantsAsset != null && _pantsAsset.isPro && !canWearPro)
        {
            _pantsAsset = null;
            pantsDirty = true;
        }
        if (_hatAsset != null && _hatAsset.isPro && !canWearPro)
        {
            _hatAsset = null;
            hatDirty = true;
        }
        if (_backpackAsset != null && _backpackAsset.isPro && !canWearPro)
        {
            _backpackAsset = null;
            backpackDirty = true;
        }
        if (_vestAsset != null && _vestAsset.isPro && !canWearPro)
        {
            vestAsset = null;
        }
        if (_maskAsset != null && _maskAsset.isPro && !canWearPro)
        {
            _maskAsset = null;
            maskDirty = true;
        }
        if (_glassesAsset != null && _glassesAsset.isPro && !canWearPro)
        {
            _glassesAsset = null;
            glassesDirty = true;
        }
        bool flag = (Provider.isServer && !Dedicator.IsDedicatedServer) || !Provider.isPvP;
        ItemShirtAsset itemShirtAsset = ((visualShirtAsset != null && isVisual && (flag || shirtAsset == null || !shirtAsset.TakesPriorityOverCosmetic)) ? visualShirtAsset : shirtAsset);
        ItemPantsAsset itemPantsAsset = ((visualPantsAsset != null && isVisual && (flag || pantsAsset == null || !pantsAsset.TakesPriorityOverCosmetic)) ? visualPantsAsset : pantsAsset);
        ItemHatAsset itemHatAsset = ((visualHatAsset != null && isVisual && (flag || hatAsset == null || !hatAsset.TakesPriorityOverCosmetic)) ? visualHatAsset : hatAsset);
        ItemBackpackAsset itemBackpackAsset = ((visualBackpackAsset != null && isVisual && (flag || backpackAsset == null || !backpackAsset.TakesPriorityOverCosmetic)) ? visualBackpackAsset : backpackAsset);
        ItemVestAsset itemVestAsset = ((visualVestAsset != null && isVisual && (flag || vestAsset == null || !vestAsset.TakesPriorityOverCosmetic)) ? visualVestAsset : vestAsset);
        ItemMaskAsset itemMaskAsset = ((visualMaskAsset != null && isVisual && (flag || maskAsset == null || !maskAsset.TakesPriorityOverCosmetic)) ? visualMaskAsset : maskAsset);
        ItemGlassesAsset itemGlassesAsset = ((visualGlassesAsset != null && isVisual && (flag || glassesAsset == null || !glassesAsset.TakesPriorityOverCosmetic)) ? visualGlassesAsset : glassesAsset);
        if (itemShirtAsset == null && itemVestAsset != null && itemVestAsset.hasFallbackShirt)
        {
            itemShirtAsset = itemVestAsset.fallbackShirt.Get<ItemShirtAsset>();
            if (itemShirtAsset == null && (bool)Assets.shouldValidateAssets)
            {
                itemVestAsset.ReportAssetError("missing fallback shirt asset");
            }
        }
        if (skinColorDirty)
        {
            materialClothing.SetColor(skinColorPropertyID, _skinColor);
        }
        if (faceDirty)
        {
            Texture2D value = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Faces/" + face + "/Texture.png");
            Texture2D value2 = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Faces/" + face + "/Emission.png");
            materialClothing.SetTexture(faceAlbedoTexturePropertyID, value);
            materialClothing.SetTexture(faceEmissionTexturePropertyID, value2);
        }
        if (shirtDirty)
        {
            bool flag2 = true;
            bool flag3 = true;
            if (itemShirtAsset != null && itemShirtAsset.shouldBeVisible(isRagdoll))
            {
                materialClothing.SetTexture(shirtAlbedoTexturePropertyID, itemShirtAsset.shirt);
                materialClothing.SetTexture(shirtEmissionTexturePropertyID, itemShirtAsset.emission);
                materialClothing.SetTexture(shirtMetallicTexturePropertyID, itemShirtAsset.metallic);
                materialClothing.SetFloat(flipShirtPropertyID, (_isLeftHanded && itemShirtAsset.ignoreHand) ? 1f : 0f);
                Mesh[] array = (isMine ? itemShirtAsset.characterMeshOverride1pLODs : itemShirtAsset.characterMeshOverride3pLODs);
                if (array != null)
                {
                    flag2 = false;
                    setCharacterMeshes(array);
                }
                if (itemShirtAsset.characterMaterialOverride != null)
                {
                    flag3 = false;
                    setCharacterMaterial(itemShirtAsset.characterMaterialOverride);
                }
            }
            else
            {
                materialClothing.SetTexture(shirtAlbedoTexturePropertyID, null);
                materialClothing.SetTexture(shirtEmissionTexturePropertyID, null);
                materialClothing.SetTexture(shirtMetallicTexturePropertyID, null);
            }
            if (flag2 != usingHumanMeshes)
            {
                usingHumanMeshes = flag2;
                if (usingHumanMeshes)
                {
                    setCharacterMeshes(humanMeshes);
                }
            }
            if (flag3 != usingHumanMaterials)
            {
                usingHumanMaterials = flag3;
                if (usingHumanMaterials)
                {
                    setCharacterMaterial(materialClothing);
                }
            }
        }
        if (pantsDirty)
        {
            if (itemPantsAsset != null && itemPantsAsset.shouldBeVisible(isRagdoll))
            {
                materialClothing.SetTexture(pantsAlbedoTexturePropertyID, itemPantsAsset.pants);
                materialClothing.SetTexture(pantsEmissionTexturePropertyID, itemPantsAsset.emission);
                materialClothing.SetTexture(pantsMetallicTexturePropertyID, itemPantsAsset.metallic);
            }
            else
            {
                materialClothing.SetTexture(pantsAlbedoTexturePropertyID, null);
                materialClothing.SetTexture(pantsEmissionTexturePropertyID, null);
                materialClothing.SetTexture(pantsMetallicTexturePropertyID, null);
            }
        }
        if (!isMine)
        {
            bool flag4 = true;
            bool flag5 = true;
            if (shirtDirty)
            {
                if (isUpper && upperSystems != null)
                {
                    for (int i = 0; i < upperSystems.Length; i++)
                    {
                        MythicalEffectController mythicalEffectController = upperSystems[i];
                        if (mythicalEffectController != null)
                        {
                            UnityEngine.Object.Destroy(mythicalEffectController);
                        }
                    }
                    isUpper = false;
                }
                if (isVisual && isMythic && visualShirt != 0)
                {
                    ushort inventoryMythicID = Provider.provider.economyService.getInventoryMythicID(visualShirt);
                    if (inventoryMythicID != 0)
                    {
                        ItemTool.ApplyMythicalEffectToMultipleTransforms(upperBones, upperSystems, inventoryMythicID, EEffectType.AREA);
                        isUpper = true;
                    }
                }
            }
            if (itemShirtAsset != null)
            {
                flag4 &= itemShirtAsset.hairVisible;
                flag5 &= itemShirtAsset.beardVisible;
            }
            if (pantsDirty)
            {
                if (isLower && lowerSystems != null)
                {
                    for (int j = 0; j < lowerSystems.Length; j++)
                    {
                        MythicalEffectController mythicalEffectController2 = lowerSystems[j];
                        if (mythicalEffectController2 != null)
                        {
                            UnityEngine.Object.Destroy(mythicalEffectController2);
                        }
                    }
                    isLower = false;
                }
                if (isVisual && isMythic && visualPants != 0)
                {
                    ushort inventoryMythicID2 = Provider.provider.economyService.getInventoryMythicID(visualPants);
                    if (inventoryMythicID2 != 0)
                    {
                        ItemTool.ApplyMythicalEffectToMultipleTransforms(lowerBones, lowerSystems, inventoryMythicID2, EEffectType.AREA);
                        isLower = true;
                    }
                }
            }
            if (itemPantsAsset != null)
            {
                flag4 &= itemPantsAsset.hairVisible;
                flag5 &= itemPantsAsset.beardVisible;
            }
            if (hatDirty)
            {
                if (hatModel != null)
                {
                    UnityEngine.Object.Destroy(hatModel.gameObject);
                }
                if (itemHatAsset != null && itemHatAsset.hat != null && itemHatAsset.shouldBeVisible(isRagdoll))
                {
                    GameObject original = ((isCosmeticPreview && itemHatAsset.cosmeticPreviewModelOverride != null) ? itemHatAsset.cosmeticPreviewModelOverride : itemHatAsset.hat);
                    InstantiateParameters instantiateParameters = default(InstantiateParameters);
                    instantiateParameters.parent = skull;
                    instantiateParameters.worldSpace = false;
                    InstantiateParameters parameters = instantiateParameters;
                    hatModel = UnityEngine.Object.Instantiate(original, Vector3.zero, Quaternion.identity, parameters).transform;
                    hatModel.name = "Hat";
                    hatModel.transform.localScale = new Vector3(1f, (_isLeftHanded && itemHatAsset.shouldMirrorLeftHandedModel) ? (-1f) : 1f, 1f);
                    if (!isView && itemHatAsset.shouldDestroyClothingColliders)
                    {
                        PrefabUtil.DestroyCollidersInChildren(hatModel.gameObject, includeInactive: true);
                    }
                    hatModel.DestroyRigidbody();
                    if (isVisual && isMythic && visualHat != 0)
                    {
                        ushort inventoryMythicID3 = Provider.provider.economyService.getInventoryMythicID(visualHat);
                        if (inventoryMythicID3 != 0)
                        {
                            if (itemHatAsset != visualHatAsset)
                            {
                                TransferEffectTransform(visualHatAsset.hat, hatModel);
                            }
                            centerHeadEffect(skull, hatModel);
                            ItemTool.ApplyMythicalEffect(hatModel, inventoryMythicID3, EEffectType.HEAD_COSMETIC);
                        }
                    }
                    ApplyHairOverride(itemHatAsset, hatModel);
                    ApplyBeardOverride(itemHatAsset, hatModel);
                    ApplySkinOverride(itemHatAsset, hatModel);
                }
            }
            if (itemHatAsset != null && itemHatAsset.hat != null)
            {
                flag4 &= itemHatAsset.hairVisible;
                flag5 &= itemHatAsset.beardVisible;
            }
            if (backpackDirty)
            {
                if (backpackModel != null)
                {
                    UnityEngine.Object.Destroy(backpackModel.gameObject);
                }
                if (itemBackpackAsset != null && itemBackpackAsset.backpack != null && itemBackpackAsset.shouldBeVisible(isRagdoll))
                {
                    GameObject original2 = ((isCosmeticPreview && itemBackpackAsset.cosmeticPreviewModelOverride != null) ? itemBackpackAsset.cosmeticPreviewModelOverride : itemBackpackAsset.backpack);
                    InstantiateParameters instantiateParameters = default(InstantiateParameters);
                    instantiateParameters.parent = spine;
                    instantiateParameters.worldSpace = false;
                    InstantiateParameters parameters2 = instantiateParameters;
                    backpackModel = UnityEngine.Object.Instantiate(original2, Vector3.zero, Quaternion.identity, parameters2).transform;
                    backpackModel.name = "Backpack";
                    backpackModel.transform.localScale = new Vector3(1f, (_isLeftHanded && itemBackpackAsset.shouldMirrorLeftHandedModel) ? (-1f) : 1f, 1f);
                    if (!isView && itemBackpackAsset.shouldDestroyClothingColliders)
                    {
                        PrefabUtil.DestroyCollidersInChildren(backpackModel.gameObject, includeInactive: true);
                    }
                    backpackModel.DestroyRigidbody();
                    if (isVisual && isMythic && visualBackpack != 0)
                    {
                        ushort inventoryMythicID4 = Provider.provider.economyService.getInventoryMythicID(visualBackpack);
                        if (inventoryMythicID4 != 0)
                        {
                            if (itemBackpackAsset != visualBackpackAsset)
                            {
                                TransferEffectTransform(visualBackpackAsset.backpack, backpackModel);
                            }
                            ItemTool.ApplyMythicalEffect(backpackModel, inventoryMythicID4, EEffectType.BODY_COSMETIC);
                        }
                    }
                    backpackModel.gameObject.SetActive(hasBackpack);
                    ApplySkinOverride(itemBackpackAsset, backpackModel);
                }
            }
            if (itemBackpackAsset != null)
            {
                flag4 &= itemBackpackAsset.hairVisible;
                flag5 &= itemBackpackAsset.beardVisible;
            }
            if (vestDirty)
            {
                if (vestModel != null)
                {
                    UnityEngine.Object.Destroy(vestModel.gameObject);
                }
                if (itemVestAsset != null && itemVestAsset.vest != null && itemVestAsset.shouldBeVisible(isRagdoll))
                {
                    GameObject original3 = ((isCosmeticPreview && itemVestAsset.cosmeticPreviewModelOverride != null) ? itemVestAsset.cosmeticPreviewModelOverride : itemVestAsset.vest);
                    InstantiateParameters instantiateParameters = default(InstantiateParameters);
                    instantiateParameters.parent = spine;
                    instantiateParameters.worldSpace = false;
                    InstantiateParameters parameters3 = instantiateParameters;
                    vestModel = UnityEngine.Object.Instantiate(original3, Vector3.zero, Quaternion.identity, parameters3).transform;
                    vestModel.name = "Vest";
                    vestModel.transform.localScale = new Vector3(1f, (_isLeftHanded && itemVestAsset.shouldMirrorLeftHandedModel) ? (-1f) : 1f, 1f);
                    if (!isView && itemVestAsset.shouldDestroyClothingColliders)
                    {
                        PrefabUtil.DestroyCollidersInChildren(vestModel.gameObject, includeInactive: true);
                    }
                    vestModel.DestroyRigidbody();
                    if (isVisual && isMythic && visualVest != 0)
                    {
                        ushort inventoryMythicID5 = Provider.provider.economyService.getInventoryMythicID(visualVest);
                        if (inventoryMythicID5 != 0)
                        {
                            if (itemVestAsset != visualVestAsset)
                            {
                                TransferEffectTransform(visualVestAsset.vest, vestModel);
                            }
                            ItemTool.ApplyMythicalEffect(vestModel, inventoryMythicID5, EEffectType.BODY_COSMETIC);
                        }
                    }
                    ApplySkinOverride(itemVestAsset, vestModel);
                }
            }
            if (itemVestAsset != null)
            {
                flag4 &= itemVestAsset.hairVisible;
                flag5 &= itemVestAsset.beardVisible;
            }
            if (maskDirty)
            {
                if (maskModel != null)
                {
                    UnityEngine.Object.Destroy(maskModel.gameObject);
                }
                if (itemMaskAsset != null && itemMaskAsset.mask != null && itemMaskAsset.shouldBeVisible(isRagdoll))
                {
                    GameObject original4 = ((isCosmeticPreview && itemMaskAsset.cosmeticPreviewModelOverride != null) ? itemMaskAsset.cosmeticPreviewModelOverride : itemMaskAsset.mask);
                    InstantiateParameters instantiateParameters = default(InstantiateParameters);
                    instantiateParameters.parent = skull;
                    instantiateParameters.worldSpace = false;
                    InstantiateParameters parameters4 = instantiateParameters;
                    maskModel = UnityEngine.Object.Instantiate(original4, Vector3.zero, Quaternion.identity, parameters4).transform;
                    maskModel.name = "Mask";
                    maskModel.transform.localScale = new Vector3(1f, (_isLeftHanded && itemMaskAsset.shouldMirrorLeftHandedModel) ? (-1f) : 1f, 1f);
                    if (!isView && itemMaskAsset.shouldDestroyClothingColliders)
                    {
                        PrefabUtil.DestroyCollidersInChildren(maskModel.gameObject, includeInactive: true);
                    }
                    maskModel.DestroyRigidbody();
                    ushort num = 0;
                    if (isVisual && isMythic && visualMask != 0)
                    {
                        num = Provider.provider.economyService.getInventoryMythicID(visualMask);
                    }
                    if (num != 0)
                    {
                        if (itemMaskAsset != visualMaskAsset && visualMaskAsset != null)
                        {
                            TransferEffectTransform(visualMaskAsset.mask, maskModel);
                        }
                        centerHeadEffect(skull, maskModel);
                        ItemTool.ApplyMythicalEffect(maskModel, num, EEffectType.HEAD_COSMETIC);
                    }
                    ApplyHairOverride(itemMaskAsset, maskModel);
                    ApplyBeardOverride(itemMaskAsset, maskModel);
                    ApplySkinOverride(itemMaskAsset, maskModel);
                }
            }
            if (itemMaskAsset != null && itemMaskAsset.mask != null)
            {
                flag4 &= itemMaskAsset.hairVisible;
                flag5 &= itemMaskAsset.beardVisible;
            }
            if (glassesDirty)
            {
                if (glassesModel != null)
                {
                    UnityEngine.Object.Destroy(glassesModel.gameObject);
                }
                if (itemGlassesAsset != null && itemGlassesAsset.glasses != null && itemGlassesAsset.shouldBeVisible(isRagdoll))
                {
                    GameObject original5 = ((isCosmeticPreview && itemGlassesAsset.cosmeticPreviewModelOverride != null) ? itemGlassesAsset.cosmeticPreviewModelOverride : itemGlassesAsset.glasses);
                    InstantiateParameters instantiateParameters = default(InstantiateParameters);
                    instantiateParameters.parent = skull;
                    instantiateParameters.worldSpace = false;
                    InstantiateParameters parameters5 = instantiateParameters;
                    glassesModel = UnityEngine.Object.Instantiate(original5, Vector3.zero, Quaternion.identity, parameters5).transform;
                    glassesModel.name = "Glasses";
                    glassesModel.localScale = new Vector3(1f, (_isLeftHanded && itemGlassesAsset.shouldMirrorLeftHandedModel) ? (-1f) : 1f, 1f);
                    if (!isView && itemGlassesAsset.shouldDestroyClothingColliders)
                    {
                        PrefabUtil.DestroyCollidersInChildren(glassesModel.gameObject, includeInactive: true);
                    }
                    glassesModel.DestroyRigidbody();
                    if (isVisual && isMythic && visualGlasses != 0)
                    {
                        ushort inventoryMythicID6 = Provider.provider.economyService.getInventoryMythicID(visualGlasses);
                        if (inventoryMythicID6 != 0)
                        {
                            if (itemGlassesAsset != visualGlassesAsset)
                            {
                                TransferEffectTransform(visualGlassesAsset.glasses, glassesModel);
                            }
                            centerHeadEffect(skull, glassesModel);
                            ItemTool.ApplyMythicalEffect(glassesModel, inventoryMythicID6, EEffectType.HEAD_COSMETIC);
                        }
                    }
                    ApplyHairOverride(itemGlassesAsset, glassesModel);
                    ApplyBeardOverride(itemGlassesAsset, glassesModel);
                    ApplySkinOverride(itemGlassesAsset, glassesModel);
                }
            }
            if (itemGlassesAsset != null && itemGlassesAsset.glasses != null)
            {
                flag4 &= itemGlassesAsset.hairVisible;
                flag5 &= itemGlassesAsset.beardVisible;
            }
            if (materialHair != null)
            {
                materialHair.color = color;
            }
            if (materialBeard != null)
            {
                materialBeard.color = BeardColor;
            }
            if (hasHair != flag4)
            {
                hasHair = flag4;
                hairDirty = true;
            }
            if (hairDirty)
            {
                if (hairModel != null)
                {
                    UnityEngine.Object.Destroy(hairModel.gameObject);
                }
                if (hasHair)
                {
                    GameObject gameObject = Assets.coreMasterBundle.LoadAsset<GameObject>("Items/Hairs/" + hair + "/Hair.prefab");
                    if (gameObject != null)
                    {
                        InstantiateParameters instantiateParameters = default(InstantiateParameters);
                        instantiateParameters.parent = skull;
                        instantiateParameters.worldSpace = false;
                        InstantiateParameters parameters6 = instantiateParameters;
                        hairModel = UnityEngine.Object.Instantiate(gameObject, Vector3.zero, Quaternion.identity, parameters6).transform;
                        hairModel.name = "Hair";
                        hairModel.transform.localScale = Vector3.one;
                        if (hairModel.Find("Model_0") != null)
                        {
                            hairModel.Find("Model_0").GetComponent<Renderer>().sharedMaterial = materialHair;
                        }
                        hairModel.DestroyRigidbody();
                    }
                }
            }
            if (hasBeard != flag5)
            {
                hasBeard = flag5;
                beardDirty = true;
            }
            if (beardDirty)
            {
                if (beardModel != null)
                {
                    UnityEngine.Object.Destroy(beardModel.gameObject);
                }
                if (hasBeard)
                {
                    GameObject gameObject2 = Assets.coreMasterBundle.LoadAsset<GameObject>("Items/Beards/" + beard + "/Beard.prefab");
                    if (gameObject2 != null)
                    {
                        InstantiateParameters instantiateParameters = default(InstantiateParameters);
                        instantiateParameters.parent = skull;
                        instantiateParameters.worldSpace = false;
                        InstantiateParameters parameters7 = instantiateParameters;
                        beardModel = UnityEngine.Object.Instantiate(gameObject2, Vector3.zero, Quaternion.identity, parameters7).transform;
                        beardModel.name = "Beard";
                        beardModel.localScale = Vector3.one;
                        if (beardModel.Find("Model_0") != null)
                        {
                            beardModel.Find("Model_0").GetComponent<Renderer>().sharedMaterial = materialBeard;
                        }
                        beardModel.DestroyRigidbody();
                    }
                }
            }
        }
        markAllDirty(isDirty: false);
    }

    /// <summary>
    /// Used when item takes priority over cosmetic but mythical effect is still visible.
    /// </summary>
    private void TransferEffectTransform(GameObject prefab, Transform model)
    {
        Transform transform = prefab?.transform.Find("Effect");
        if (!(transform == null))
        {
            Transform transform2 = model.Find("Effect");
            if (transform2 == null)
            {
                transform2 = new GameObject("Effect").transform;
                transform2.parent = model;
                transform2.localScale = Vector3.one;
            }
            transform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            transform2.SetLocalPositionAndRotation(localPosition, localRotation);
        }
    }

    /// <summary>
    /// Center mythical effect hook horizontally, but maintain vertical placement.
    /// Lots of hats/masks/glasses have off-center effects intentionally, but community
    /// feedback suggests centering to make effects like circling atoms look better.
    /// </summary>
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

    /// <summary>
    /// Set mesh of all character mesh renderers.
    /// Tries to match renderer index to mesh LOD index.
    /// </summary>
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
                    skinnedMeshRenderer2.sharedMesh = meshes[^1];
                }
                num++;
            }
        }
    }

    /// <summary>
    /// Set material of all character mesh renderers.
    /// </summary>
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
        upperSystems = new MythicalEffectController[upperBones.Length];
        lowerBones = new Transform[4]
        {
            spine.parent.Find("Left_Hip/Left_Leg"),
            spine.parent.Find("Left_Hip/Left_Leg/Left_Foot"),
            spine.parent.Find("Right_Hip/Right_Leg"),
            spine.parent.Find("Right_Hip/Right_Leg/Right_Foot")
        };
        lowerSystems = new MythicalEffectController[lowerBones.Length];
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
        if (!Dedicator.IsDedicatedServer)
        {
            if (shader == null)
            {
                shader = Shader.Find("Standard (Specular setup)");
            }
            if (clothingShader == null)
            {
                clothingShader = Shader.Find("Standard/Clothes");
            }
            humanMeshes = new Mesh[characterMeshRenderers.Length];
            for (int i = 0; i < humanMeshes.Length; i++)
            {
                if (characterMeshRenderers[i] != null)
                {
                    humanMeshes[i] = characterMeshRenderers[i].sharedMesh;
                }
            }
            materialClothing = new Material(clothingShader);
            materialClothing.hideFlags = HideFlags.HideAndDontSave;
            materialHair = new Material(shader);
            materialHair.name = "Hair";
            materialHair.hideFlags = HideFlags.HideAndDontSave;
            materialHair.SetFloat("_Glossiness", 0f);
            materialHair.SetColor("_SpecColor", Color.black);
            materialBeard = new Material(shader);
            materialBeard.name = "Hair";
            materialBeard.hideFlags = HideFlags.HideAndDontSave;
            materialBeard.SetFloat("_Glossiness", 0f);
            materialBeard.SetColor("_SpecColor", Color.black);
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
        if (materialBeard != null)
        {
            UnityEngine.Object.DestroyImmediate(materialBeard);
            materialBeard = null;
        }
        if (extraHairOverrideMaterials == null)
        {
            return;
        }
        Material[] array = extraHairOverrideMaterials;
        foreach (Material material in array)
        {
            if (material != null)
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
        extraHairOverrideMaterials = null;
    }
}
