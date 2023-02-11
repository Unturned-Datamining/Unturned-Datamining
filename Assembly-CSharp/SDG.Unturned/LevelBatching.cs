using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SDG.Unturned;

internal class LevelBatching
{
    private class MeshUsers
    {
        public List<MeshFilter> meshFilters = new List<MeshFilter>();

        public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
    }

    private class TextureUsers
    {
        public Texture2D texture;

        public bool isGeneratedTexture;

        public Dictionary<Mesh, MeshUsers> componentsUsingMesh = new Dictionary<Mesh, MeshUsers>();

        public List<Renderer> renderersUsingTexture = new List<Renderer>();

        public void AddMeshFilter(MeshFilter meshFilter)
        {
            GetOrAddListForMesh(meshFilter.sharedMesh).meshFilters.Add(meshFilter);
        }

        public void AddSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            GetOrAddListForMesh(skinnedMeshRenderer.sharedMesh).skinnedMeshRenderers.Add(skinnedMeshRenderer);
        }

        private MeshUsers GetOrAddListForMesh(Mesh mesh)
        {
            if (!componentsUsingMesh.TryGetValue(mesh, out var value))
            {
                value = new MeshUsers();
                componentsUsingMesh.Add(mesh, value);
            }
            return value;
        }
    }

    private class ShaderGroup
    {
        public Material materialTemplate;

        public Dictionary<Texture2D, TextureUsers> batchableTextures = new Dictionary<Texture2D, TextureUsers>();

        public FilterMode filterMode;

        public TextureUsers GetOrAddListForTexture(Texture2D texture)
        {
            if (!batchableTextures.TryGetValue(texture, out var value))
            {
                value = new TextureUsers();
                value.texture = texture;
                batchableTextures.Add(texture, value);
            }
            return value;
        }

        public void Clear()
        {
            batchableTextures.Clear();
        }
    }

    private struct StaticBatchingInitialState
    {
        public Transform parent;

        public bool wasEnabled;

        public bool wasActive;
    }

    private List<Transform> ignoreTransforms = new List<Transform>();

    private List<Renderer> renderers = new List<Renderer>();

    private List<Vector2> uvs = new List<Vector2>();

    private List<Material> sharedMaterials = new List<Material>();

    private ShaderGroup standardDecalableOpaque;

    private ShaderGroup standardSpecularSetupDecalableOpaque;

    private ShaderGroup batchableCard;

    private ShaderGroup batchableFoliage;

    internal static LevelBatching instance;

    private int propertyID_MainTex = Shader.PropertyToID("_MainTex");

    private int propertyID_Mode = Shader.PropertyToID("_Mode");

    private int propertyID_Color = Shader.PropertyToID("_Color");

    private int propertyID_SpecColor = Shader.PropertyToID("_SpecColor");

    private int propertyID_Metallic = Shader.PropertyToID("_Metallic");

    private int propertyID_Glossiness = Shader.PropertyToID("_Glossiness");

    private HashSet<Mesh> loggedMeshes = new HashSet<Mesh>();

    private HashSet<Texture2D> loggedTextures = new HashSet<Texture2D>();

    private HashSet<Material> loggedMaterials = new HashSet<Material>();

    private List<MeshRenderer> staticBatchingMeshRenderers = new List<MeshRenderer>();

    private List<Object> objectsToDestroy = new List<Object>();

    private CommandLineFlag wantsToPreviewTextureAtlas = new CommandLineFlag(defaultValue: false, "-PreviewLevelBatchingTextureAtlas");

    private CommandLineFlag shouldLogTextureAtlasExclusions = new CommandLineFlag(defaultValue: false, "-LogLevelBatchingTextureAtlasExclusions");

    private CommandLineFlag shouldValidateUVs = new CommandLineFlag(defaultValue: false, "-ValidateLevelBatchingUVs");

    private Dictionary<Material, Texture2D> colorTextures = new Dictionary<Material, Texture2D>();

    public static LevelBatching Get()
    {
        return instance;
    }

    public void Reset()
    {
        if (standardDecalableOpaque == null)
        {
            standardDecalableOpaque = new ShaderGroup();
            standardDecalableOpaque.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/StandardDecalableOpaque");
        }
        else
        {
            standardDecalableOpaque.Clear();
        }
        if (standardSpecularSetupDecalableOpaque == null)
        {
            standardSpecularSetupDecalableOpaque = new ShaderGroup();
            standardSpecularSetupDecalableOpaque.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/StandardSpecularSetupDecalableOpaque");
        }
        else
        {
            standardSpecularSetupDecalableOpaque.Clear();
        }
        if (batchableCard == null)
        {
            batchableCard = new ShaderGroup();
            batchableCard.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/Card");
        }
        else
        {
            batchableCard.Clear();
        }
        if (batchableFoliage == null)
        {
            batchableFoliage = new ShaderGroup();
            batchableFoliage.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/Foliage");
            batchableFoliage.filterMode = FilterMode.Trilinear;
        }
        else
        {
            batchableFoliage.Clear();
        }
        loggedMeshes.Clear();
        loggedTextures.Clear();
        loggedMaterials.Clear();
        staticBatchingMeshRenderers.Clear();
    }

    public void Destroy()
    {
        foreach (Object item in objectsToDestroy)
        {
            Object.Destroy(item);
        }
        objectsToDestroy.Clear();
    }

    public void AddLevelObject(LevelObject levelObject)
    {
        if (levelObject == null || levelObject.asset == null || levelObject.asset.shouldExcludeFromLevelBatching)
        {
            return;
        }
        if (levelObject.transform != null)
        {
            if (levelObject.rubble != null && levelObject.rubble.rubbleInfos != null)
            {
                RubbleInfo[] rubbleInfos = levelObject.rubble.rubbleInfos;
                foreach (RubbleInfo rubbleInfo in rubbleInfos)
                {
                    if (rubbleInfo.ragdolls == null)
                    {
                        continue;
                    }
                    RubbleRagdollInfo[] ragdolls = rubbleInfo.ragdolls;
                    foreach (RubbleRagdollInfo rubbleRagdollInfo in ragdolls)
                    {
                        if (rubbleRagdollInfo.ragdollGameObject != null)
                        {
                            ignoreTransforms.Add(rubbleRagdollInfo.ragdollGameObject.transform);
                        }
                    }
                }
            }
            AddGameObject(levelObject.transform.gameObject);
            ignoreTransforms.Clear();
        }
        if (levelObject.skybox != null)
        {
            AddGameObject(levelObject.skybox.gameObject);
        }
    }

    public void AddResourceSpawnpoint(ResourceSpawnpoint resourceSpawnpoint)
    {
        if (resourceSpawnpoint != null && resourceSpawnpoint.asset != null && !resourceSpawnpoint.asset.shouldExcludeFromLevelBatching)
        {
            if (resourceSpawnpoint.model != null)
            {
                AddGameObject(resourceSpawnpoint.model.gameObject);
            }
            if (resourceSpawnpoint.skybox != null)
            {
                AddGameObject(resourceSpawnpoint.skybox.gameObject);
            }
        }
    }

    public void ApplyTextureAtlas()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool shouldPreview = Provider.isServer && (bool)wantsToPreviewTextureAtlas;
        batch(standardDecalableOpaque, shouldPreview);
        batch(standardSpecularSetupDecalableOpaque, shouldPreview);
        batch(batchableCard, shouldPreview);
        batch(batchableFoliage, shouldPreview);
        DestroyColorTextures();
        stopwatch.Stop();
        UnturnedLog.info($"Level texture atlas generation took: {stopwatch.ElapsedMilliseconds}ms");
    }

    public void ApplyStaticBatching()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        GameObject[] array = new GameObject[staticBatchingMeshRenderers.Count];
        StaticBatchingInitialState[] array2 = new StaticBatchingInitialState[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            MeshRenderer meshRenderer = staticBatchingMeshRenderers[i];
            Transform transform = meshRenderer.transform;
            GameObject gameObject = meshRenderer.gameObject;
            StaticBatchingInitialState staticBatchingInitialState = default(StaticBatchingInitialState);
            staticBatchingInitialState.parent = transform.parent;
            staticBatchingInitialState.wasEnabled = meshRenderer.enabled;
            staticBatchingInitialState.wasActive = gameObject.activeSelf;
            array[i] = gameObject;
            array2[i] = staticBatchingInitialState;
            meshRenderer.enabled = true;
            if (staticBatchingInitialState.parent != null)
            {
                transform.parent = null;
            }
            if (!staticBatchingInitialState.wasActive)
            {
                gameObject.SetActive(value: true);
            }
        }
        GameObject staticBatchRoot = new GameObject("Static Batching Root (LevelBatching)");
        StaticBatchingUtility.Combine(array, staticBatchRoot);
        for (int j = 0; j < array.Length; j++)
        {
            MeshRenderer meshRenderer2 = staticBatchingMeshRenderers[j];
            Transform transform2 = meshRenderer2.transform;
            GameObject gameObject2 = meshRenderer2.gameObject;
            StaticBatchingInitialState staticBatchingInitialState2 = array2[j];
            meshRenderer2.enabled = staticBatchingInitialState2.wasEnabled;
            if (!staticBatchingInitialState2.wasActive)
            {
                gameObject2.SetActive(staticBatchingInitialState2.wasActive);
            }
            if (staticBatchingInitialState2.parent != null)
            {
                transform2.parent = staticBatchingInitialState2.parent;
            }
        }
        stopwatch.Stop();
        UnturnedLog.info($"Level static batching took: {stopwatch.ElapsedMilliseconds}ms");
    }

    private void AddGameObject(GameObject gameObject)
    {
        renderers.Clear();
        gameObject.GetComponentsInChildren(includeInactive: true, renderers);
        foreach (Renderer renderer in renderers)
        {
            if (ignoreTransforms.Count > 0)
            {
                bool flag = false;
                foreach (Transform ignoreTransform in ignoreTransforms)
                {
                    if (renderer.transform.IsChildOf(ignoreTransform))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }
            }
            if (renderer is MeshRenderer meshRenderer)
            {
                MeshFilter component = renderer.GetComponent<MeshFilter>();
                Mesh mesh = component?.sharedMesh;
                if (mesh != null && CanBatchMesh(mesh, renderer))
                {
                    AddMesh(component, meshRenderer);
                    staticBatchingMeshRenderers.Add(meshRenderer);
                }
            }
            else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
                if (sharedMesh != null && CanBatchMesh(sharedMesh, renderer))
                {
                    AddSkinnedMesh(skinnedMeshRenderer);
                }
            }
        }
    }

    private TextureUsers AddMeshCommon(Renderer renderer)
    {
        sharedMaterials.Clear();
        renderer.GetSharedMaterials(sharedMaterials);
        if (sharedMaterials.Count < 1)
        {
            if ((bool)shouldLogTextureAtlasExclusions)
            {
                UnturnedLog.info("Excluding renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because it has no materials");
            }
            return null;
        }
        if (sharedMaterials.Count > 1)
        {
            if ((bool)shouldLogTextureAtlasExclusions)
            {
                UnturnedLog.info("Excluding renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because more than one material is not supported (yet?)");
            }
            return null;
        }
        Material material = sharedMaterials[0];
        if (material == null)
        {
            if ((bool)shouldLogTextureAtlasExclusions)
            {
                UnturnedLog.info("Excluding renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because material is null");
            }
            return null;
        }
        if (material.name.EndsWith(" (Instance)"))
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because it was probably instantiated for dynamic use");
            }
            return null;
        }
        Shader shader = material.shader;
        if (shader == null)
        {
            return null;
        }
        if (!material.HasProperty(propertyID_MainTex))
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because shader \"" + shader.name + "\" does not use a main texture");
            }
            return null;
        }
        ShaderGroup shaderGroup = null;
        Texture2D texture2D = material.mainTexture as Texture2D;
        bool isGeneratedTexture = false;
        if (texture2D == null)
        {
            if (shader.name == "Standard (Decalable)")
            {
                if (CanAtlasStandardMaterialSimpleOpaque(material, renderer, requireWhiteColor: false, isSpecular: false))
                {
                    texture2D = GetOrAddColorTexture(material);
                    isGeneratedTexture = true;
                    shaderGroup = standardDecalableOpaque;
                }
            }
            else if (shader.name == "Standard (Specular setup) (Decalable)" && CanAtlasStandardMaterialSimpleOpaque(material, renderer, requireWhiteColor: false, isSpecular: true))
            {
                texture2D = GetOrAddColorTexture(material);
                isGeneratedTexture = true;
                shaderGroup = standardSpecularSetupDecalableOpaque;
            }
        }
        else
        {
            if (texture2D.width > 128 || texture2D.height > 128)
            {
                if ((bool)shouldLogTextureAtlasExclusions && loggedTextures.Add(texture2D))
                {
                    UnturnedLog.info($"Excluding texture \"{texture2D.name}\" in material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because dimensions ({texture2D.width}x{texture2D.height}) are higher than limit ({128}x{128})");
                }
                return null;
            }
            if (texture2D.wrapMode != TextureWrapMode.Clamp)
            {
                if ((bool)shouldLogTextureAtlasExclusions && loggedTextures.Add(texture2D))
                {
                    UnturnedLog.info($"Excluding texture \"{texture2D.name}\" in material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Wrap Mode ({texture2D.wrapMode}) is not Clamp");
                }
                return null;
            }
            if (shader.name == "Standard (Decalable)")
            {
                if (CanAtlasStandardMaterialSimpleOpaque(material, renderer, requireWhiteColor: true, isSpecular: false) && CanAtlasTextureFilterMode(texture2D, material, renderer, FilterMode.Point))
                {
                    shaderGroup = standardDecalableOpaque;
                }
            }
            else if (shader.name == "Standard (Specular setup) (Decalable)")
            {
                if (CanAtlasStandardMaterialSimpleOpaque(material, renderer, requireWhiteColor: true, isSpecular: true) && CanAtlasTextureFilterMode(texture2D, material, renderer, FilterMode.Point))
                {
                    shaderGroup = standardSpecularSetupDecalableOpaque;
                }
            }
            else if (shader.name == "Custom/Card")
            {
                shaderGroup = batchableCard;
            }
            else if (shader.name == "Custom/Foliage" && CanAtlasTextureFilterMode(texture2D, material, renderer, FilterMode.Trilinear))
            {
                shaderGroup = batchableFoliage;
            }
        }
        if (shaderGroup != null)
        {
            TextureUsers orAddListForTexture = shaderGroup.GetOrAddListForTexture(texture2D);
            orAddListForTexture.isGeneratedTexture = isGeneratedTexture;
            orAddListForTexture.renderersUsingTexture.Add(renderer);
            return orAddListForTexture;
        }
        return null;
    }

    private void AddMesh(MeshFilter meshFilter, MeshRenderer meshRenderer)
    {
        AddMeshCommon(meshRenderer)?.AddMeshFilter(meshFilter);
    }

    private void AddSkinnedMesh(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        AddMeshCommon(skinnedMeshRenderer)?.AddSkinnedMeshRenderer(skinnedMeshRenderer);
    }

    private bool CanBatchMesh(Mesh mesh, Renderer renderer)
    {
        if (mesh.isReadable)
        {
            return true;
        }
        if ((bool)shouldLogTextureAtlasExclusions && loggedMeshes.Add(mesh))
        {
            UnturnedLog.info("Excluding mesh \"" + mesh.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from level batching because it is not CPU-readable");
        }
        return false;
    }

    private bool CanAtlasTextureFilterMode(Texture2D texture, Material material, Renderer renderer, FilterMode requiredFilterMode)
    {
        if (texture.filterMode == requiredFilterMode)
        {
            return true;
        }
        if ((bool)shouldLogTextureAtlasExclusions && loggedTextures.Add(texture))
        {
            UnturnedLog.info($"Excluding texture \"{texture.name}\" in material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Filter Mode ({texture.filterMode}) is not {requiredFilterMode}");
        }
        return false;
    }

    private bool CanAtlasStandardMaterialSimpleOpaque(Material material, Renderer renderer, bool requireWhiteColor, bool isSpecular)
    {
        if (!Mathf.Approximately(material.GetFloat(propertyID_Mode), 0f))
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Mode is not Opaque");
            }
            return false;
        }
        if (requireWhiteColor && !material.GetColor(propertyID_Color).IsNearlyWhite())
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Color is not white");
            }
            return false;
        }
        if (isSpecular)
        {
            if (!material.GetColor(propertyID_SpecColor).IsNearlyBlack())
            {
                if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
                {
                    UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Specular Color is not black");
                }
                return false;
            }
            if (material.IsKeywordEnabled("_SPECGLOSSMAP"))
            {
                if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
                {
                    UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Specular Map is enabled");
                }
                return false;
            }
        }
        else
        {
            if (!Mathf.Approximately(material.GetFloat(propertyID_Metallic), 0f))
            {
                if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
                {
                    UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Metallic is not zero");
                }
                return false;
            }
            if (material.IsKeywordEnabled("_METALLICGLOSSMAP"))
            {
                if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
                {
                    UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Metallic Map is enabled");
                }
                return false;
            }
        }
        if (!Mathf.Approximately(material.GetFloat(propertyID_Glossiness), 0f))
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Smoothness is not zero");
            }
            return false;
        }
        if (material.IsKeywordEnabled("_NORMALMAP"))
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Normal Map is enabled");
            }
            return false;
        }
        if (material.IsKeywordEnabled("_EMISSION"))
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Emission is enabled");
            }
            return false;
        }
        if (material.IsKeywordEnabled("_PARALLAXMAP"))
        {
            if ((bool)shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
            {
                UnturnedLog.info("Excluding material \"" + material.name + "\" in renderer \"" + renderer.GetSceneHierarchyPath() + "\" from atlas because Parallax Map is enabled");
            }
            return false;
        }
        return true;
    }

    private Texture2D GetOrAddColorTexture(Material material)
    {
        if (!colorTextures.TryGetValue(material, out var value))
        {
            value = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false, linear: false);
            value.wrapMode = TextureWrapMode.Clamp;
            value.filterMode = FilterMode.Point;
            value.SetPixel(0, 0, material.GetColor(propertyID_Color));
            value.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            colorTextures.Add(material, value);
        }
        return value;
    }

    private void DestroyColorTextures()
    {
        foreach (KeyValuePair<Material, Texture2D> colorTexture in colorTextures)
        {
            Object.Destroy(colorTexture.Value);
        }
        colorTextures.Clear();
    }

    private void batch(ShaderGroup group, bool shouldPreview)
    {
        Material materialTemplate = group.materialTemplate;
        Dictionary<Texture2D, TextureUsers> batchableTextures = group.batchableTextures;
        UnturnedLog.info($"{batchableTextures.Count} texture(s) in {group.materialTemplate.shader.name} group");
        if (batchableTextures.Count <= 0)
        {
            return;
        }
        Texture2D texture2D = new Texture2D(16, 16);
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.filterMode = group.filterMode;
        TextureUsers[] array = new TextureUsers[batchableTextures.Count];
        batchableTextures.Values.CopyTo(array, 0);
        Texture2D[] array2 = new Texture2D[array.Length];
        RenderTexture active = RenderTexture.active;
        for (int i = 0; i < array.Length; i++)
        {
            Texture2D texture = array[i].texture;
            RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(texture, temporary);
            RenderTexture.active = temporary;
            Texture2D texture2D2 = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, mipChain: false, linear: true);
            texture2D2.ReadPixels(new Rect(0f, 0f, texture.width, texture.height), 0, 0);
            array2[i] = texture2D2;
            RenderTexture.ReleaseTemporary(temporary);
        }
        RenderTexture.active = active;
        Rect[] array3 = texture2D.PackTextures(array2, 0, 2048, makeNoLongerReadable: true);
        if (array3 != null)
        {
            objectsToDestroy.Add(texture2D);
            Material material = Object.Instantiate(materialTemplate);
            objectsToDestroy.Add(material);
            if (!shouldPreview)
            {
                material.mainTexture = texture2D;
            }
            for (int j = 0; j < array.Length; j++)
            {
                TextureUsers textureUsers = array[j];
                foreach (KeyValuePair<Mesh, MeshUsers> item in textureUsers.componentsUsingMesh)
                {
                    Mesh key = item.Key;
                    List<MeshFilter> meshFilters = item.Value.meshFilters;
                    List<SkinnedMeshRenderer> skinnedMeshRenderers = item.Value.skinnedMeshRenderers;
                    Mesh mesh = Object.Instantiate(key);
                    objectsToDestroy.Add(mesh);
                    uvs.Clear();
                    mesh.GetUVs(0, uvs);
                    if (textureUsers.isGeneratedTexture)
                    {
                        Rect rect = array3[j];
                        Vector2 value = new Vector2(rect.x + 0.5f * rect.width, rect.y + 0.5f * rect.height);
                        for (int k = 0; k < uvs.Count; k++)
                        {
                            uvs[k] = value;
                        }
                    }
                    else
                    {
                        if ((bool)shouldValidateUVs)
                        {
                            bool flag = false;
                            foreach (Vector2 uv in uvs)
                            {
                                if (uv.x < 0f || uv.y < 0f || uv.x > 1f || uv.y > 1f)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag && loggedMeshes.Add(key))
                            {
                                Component component = meshFilters.HeadOrDefault();
                                if (component == null)
                                {
                                    component = skinnedMeshRenderers.HeadOrDefault();
                                }
                                UnturnedLog.error("Mesh \"" + key.name + "\" in renderer \"" + component?.GetSceneHierarchyPath() + "\" has UVs outside [0, 1] range (should be excluded from level batching)");
                            }
                        }
                        for (int l = 0; l < uvs.Count; l++)
                        {
                            Rect rect2 = array3[j];
                            Vector2 value2 = uvs[l];
                            value2.x = rect2.x + value2.x * rect2.width;
                            value2.y = rect2.y + value2.y * rect2.height;
                            uvs[l] = value2;
                        }
                    }
                    mesh.SetUVs(0, uvs, 0, uvs.Count);
                    foreach (MeshFilter item2 in meshFilters)
                    {
                        item2.sharedMesh = mesh;
                    }
                    foreach (SkinnedMeshRenderer item3 in skinnedMeshRenderers)
                    {
                        item3.sharedMesh = mesh;
                    }
                }
                foreach (Renderer item4 in textureUsers.renderersUsingTexture)
                {
                    item4.sharedMaterial = material;
                }
            }
        }
        else
        {
            Object.Destroy(texture2D);
        }
        Texture2D[] array4 = array2;
        for (int m = 0; m < array4.Length; m++)
        {
            Object.Destroy(array4[m]);
        }
    }
}
