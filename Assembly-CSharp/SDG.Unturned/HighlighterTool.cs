using System;
using System.Collections.Generic;
using HighlightingSystem;
using UnityEngine;

namespace SDG.Unturned;

public class HighlighterTool
{
    private static List<Renderer> renderers = new List<Renderer>();

    private static List<MeshFilter> lods = new List<MeshFilter>();

    private static HighlighterShaderGroup batchableOpaque;

    private static HighlighterShaderGroup batchableCard;

    private static HighlighterShaderGroup batchableFoliage;

    private static List<List<GameObject>> batchableGameObjects = new List<List<GameObject>>();

    private static Dictionary<Material, List<GameObject>> batchableMaterials = new Dictionary<Material, List<GameObject>>();

    private static List<HighlighterBatch> batchablePool = new List<HighlighterBatch>();

    private static int batchablePoolIndex = 0;

    public static void color(Transform target, Color color)
    {
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<Renderer>() != null)
        {
            target.GetComponent<Renderer>().material.color = color;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().material.color = color;
            }
        }
    }

    public static void destroyMaterials(Transform target)
    {
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<Renderer>() != null)
        {
            UnityEngine.Object.DestroyImmediate(target.GetComponent<Renderer>().material);
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                UnityEngine.Object.DestroyImmediate(transform.GetComponent<Renderer>().material);
            }
        }
    }

    public static void help(Transform target, bool isValid)
    {
        help(target, isValid, isRecursive: false);
    }

    public static void help(Transform target, bool isValid, bool isRecursive)
    {
        Material sharedMaterial = (isValid ? ((Material)Resources.Load("Materials/PlacementPreview_Valid")) : ((Material)Resources.Load("Materials/PlacementPreview_Invalid")));
        if (target.GetComponent<Renderer>() != null)
        {
            target.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = ((!isRecursive) ? target.Find("Model_" + i) : target.FindChildRecursive("Model_" + i));
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
            }
        }
    }

    public static void guide(Transform target)
    {
        Material sharedMaterial = (Material)Resources.Load("Materials/Guide");
        renderers.Clear();
        target.GetComponentsInChildren(includeInactive: true, renderers);
        for (int i = 0; i < renderers.Count; i++)
        {
            if (!(renderers[i].transform != target) || renderers[i].name.IndexOf("Model") != -1)
            {
                renderers[i].sharedMaterial = sharedMaterial;
            }
        }
        List<Collider> list = new List<Collider>();
        target.GetComponentsInChildren(list);
        for (int j = 0; j < list.Count; j++)
        {
            UnityEngine.Object.Destroy(list[j]);
        }
    }

    public static void highlight(Transform target, Color color)
    {
        if (!target.CompareTag("Player") && !target.CompareTag("Enemy") && !target.CompareTag("Zombie") && !target.CompareTag("Animal") && !target.CompareTag("Agent"))
        {
            Highlighter highlighter = target.GetComponent<Highlighter>();
            if (highlighter == null)
            {
                highlighter = target.gameObject.AddComponent<Highlighter>();
            }
            highlighter.ConstantOn(color);
        }
    }

    public static void unhighlight(Transform target)
    {
        Highlighter component = target.GetComponent<Highlighter>();
        if (!(component == null))
        {
            UnityEngine.Object.DestroyImmediate(component);
        }
    }

    public static void skin(Transform target, Material skin)
    {
        if (target.GetComponent<Renderer>() != null)
        {
            target.GetComponent<Renderer>().sharedMaterial = skin;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().sharedMaterial = skin;
            }
        }
    }

    [Obsolete]
    public static Material getMaterial(Transform target)
    {
        if (target == null)
        {
            return null;
        }
        Renderer component = target.GetComponent<Renderer>();
        if (component != null)
        {
            return component.sharedMaterial;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (transform == null)
            {
                return null;
            }
            component = transform.GetComponent<Renderer>();
            if (component != null)
            {
                return component.sharedMaterial;
            }
        }
        return null;
    }

    public static Material getMaterialInstance(Transform target)
    {
        if (target == null)
        {
            return null;
        }
        Renderer component = target.GetComponent<Renderer>();
        if (component != null)
        {
            return component.material;
        }
        Material material = null;
        Material material2 = null;
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (transform == null)
            {
                break;
            }
            component = transform.GetComponent<Renderer>();
            if (component != null)
            {
                if (material == null)
                {
                    material2 = component.sharedMaterial;
                    material = component.material;
                }
                else if (component.sharedMaterial == material2)
                {
                    component.sharedMaterial = material;
                }
            }
        }
        return material;
    }

    public static void remesh(Transform target, List<Mesh> newMeshes, List<Mesh> outOldMeshes)
    {
        if (newMeshes == null || newMeshes.Count < 1)
        {
            return;
        }
        if (outOldMeshes != null && outOldMeshes != newMeshes)
        {
            outOldMeshes.Clear();
        }
        MeshFilter component = target.GetComponent<MeshFilter>();
        if (component != null)
        {
            Mesh sharedMesh = component.sharedMesh;
            component.sharedMesh = newMeshes[0];
            if (outOldMeshes != null)
            {
                if (outOldMeshes == newMeshes)
                {
                    newMeshes[0] = sharedMesh;
                }
                else
                {
                    outOldMeshes.Add(sharedMesh);
                }
            }
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (transform == null)
            {
                continue;
            }
            component = transform.GetComponent<MeshFilter>();
            if (!(component != null))
            {
                continue;
            }
            Mesh sharedMesh2 = component.sharedMesh;
            component.sharedMesh = ((i < newMeshes.Count) ? newMeshes[i] : newMeshes[0]);
            if (outOldMeshes == null)
            {
                continue;
            }
            if (outOldMeshes == newMeshes)
            {
                if (i < newMeshes.Count)
                {
                    newMeshes[i] = sharedMesh2;
                }
                else
                {
                    newMeshes.Add(sharedMesh2);
                }
            }
            else
            {
                outOldMeshes.Add(sharedMesh2);
            }
        }
    }

    public static void rematerialize(Transform target, Material newMaterial, out Material oldMaterial)
    {
        oldMaterial = null;
        Renderer component = target.GetComponent<Renderer>();
        if (component != null)
        {
            oldMaterial = component.sharedMaterial;
            component.sharedMaterial = newMaterial;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null))
            {
                component = transform.GetComponent<Renderer>();
                if (component != null)
                {
                    oldMaterial = component.sharedMaterial;
                    component.sharedMaterial = newMaterial;
                }
            }
        }
    }

    private static HighlighterBatch getBatchable()
    {
        if (batchablePoolIndex < batchablePool.Count)
        {
            HighlighterBatch highlighterBatch = batchablePool[batchablePoolIndex];
            highlighterBatch.texture = null;
            highlighterBatch.meshes.Clear();
            highlighterBatch.renderers.Clear();
            batchablePoolIndex++;
            return highlighterBatch;
        }
        HighlighterBatch highlighterBatch2 = new HighlighterBatch();
        batchablePool.Add(highlighterBatch2);
        batchablePoolIndex++;
        return highlighterBatch2;
    }

    private static void checkBatchable(List<GameObject> list, MeshFilter mesh, MeshRenderer renderer)
    {
        if (!(mesh != null) || !(mesh.sharedMesh != null) || !(renderer != null) || renderer.sharedMaterials == null || renderer.sharedMaterials.Length != 1)
        {
            return;
        }
        Texture2D texture2D = (Texture2D)renderer.sharedMaterial.mainTexture;
        HighlighterShaderGroup highlighterShaderGroup = null;
        if (texture2D != null && texture2D.wrapMode == TextureWrapMode.Clamp && texture2D.width <= 128 && texture2D.height <= 128)
        {
            if (renderer.sharedMaterial.shader.name == "Standard")
            {
                if (renderer.sharedMaterial.GetFloat("_Mode") == 0f && texture2D.filterMode == FilterMode.Point)
                {
                    highlighterShaderGroup = batchableOpaque;
                }
            }
            else if (renderer.sharedMaterial.shader.name == "Custom/Card")
            {
                highlighterShaderGroup = batchableCard;
            }
            else if (renderer.sharedMaterial.shader.name == "Custom/Foliage" && texture2D.filterMode == FilterMode.Trilinear)
            {
                highlighterShaderGroup = batchableFoliage;
            }
        }
        if (highlighterShaderGroup != null)
        {
            HighlighterBatch value = null;
            if (!highlighterShaderGroup.batchableTextures.TryGetValue(texture2D, out value))
            {
                value = getBatchable();
                value.texture = texture2D;
                highlighterShaderGroup.batchableTextures.Add(texture2D, value);
            }
            if (value != null)
            {
                if (!value.meshes.TryGetValue(mesh.sharedMesh, out var value2))
                {
                    value2 = new List<MeshFilter>();
                    value.meshes.Add(mesh.sharedMesh, value2);
                }
                value2.Add(mesh);
                value.renderers.Add(renderer);
                list.Add(mesh.gameObject);
            }
        }
        else
        {
            List<GameObject> value3 = null;
            if (!batchableMaterials.TryGetValue(renderer.sharedMaterial, out value3))
            {
                value3 = new List<GameObject>();
                batchableMaterials.Add(renderer.sharedMaterial, value3);
            }
            value3.Add(mesh.gameObject);
        }
    }

    private static void batch(HighlighterShaderGroup group)
    {
        Material materialTemplate = group.materialTemplate;
        Dictionary<Texture2D, HighlighterBatch> batchableTextures = group.batchableTextures;
        if (batchableTextures.Count <= 0)
        {
            return;
        }
        Texture2D texture2D = new Texture2D(16, 16);
        texture2D.name = "Atlas";
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.filterMode = group.filterMode;
        HighlighterBatch[] array = new HighlighterBatch[batchableTextures.Count];
        batchableTextures.Values.CopyTo(array, 0);
        Texture2D[] array2 = new Texture2D[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            Texture2D texture = array[i].texture;
            RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(texture, temporary);
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = temporary;
            Texture2D texture2D2 = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, mipChain: false, linear: true);
            texture2D2.name = "Copy";
            texture2D2.ReadPixels(new Rect(0f, 0f, texture.width, texture.height), 0, 0);
            texture2D2.Apply();
            array2[i] = texture2D2;
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
        }
        Rect[] array3 = texture2D.PackTextures(array2, 0, 1024, makeNoLongerReadable: true);
        if (array3 != null)
        {
            Material material = UnityEngine.Object.Instantiate(materialTemplate);
            material.name = "Material";
            material.mainTexture = texture2D;
            for (int j = 0; j < array.Length; j++)
            {
                HighlighterBatch highlighterBatch = array[j];
                List<MeshFilter>[] array4 = new List<MeshFilter>[highlighterBatch.meshes.Count];
                highlighterBatch.meshes.Values.CopyTo(array4, 0);
                for (int k = 0; k < array4.Length; k++)
                {
                    Mesh mesh = array4[k][0].mesh;
                    Vector2[] uv = mesh.uv;
                    for (int l = 0; l < uv.Length; l++)
                    {
                        uv[l].x = array3[j].x + uv[l].x * array3[j].width;
                        uv[l].y = array3[j].y + uv[l].y * array3[j].height;
                    }
                    mesh.uv = uv;
                    if (array4[k].Count > 1)
                    {
                        for (int m = 1; m < array4[k].Count; m++)
                        {
                            array4[k][m].sharedMesh = mesh;
                        }
                    }
                }
                for (int n = 0; n < highlighterBatch.renderers.Count; n++)
                {
                    highlighterBatch.renderers[n].sharedMaterial = material;
                }
            }
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(texture2D);
        }
        for (int num = 0; num < array2.Length; num++)
        {
            UnityEngine.Object.DestroyImmediate(array2[num]);
        }
    }

    public static void beginBatch()
    {
        if (batchableOpaque == null)
        {
            batchableOpaque = new HighlighterShaderGroup();
            batchableOpaque.materialTemplate = (Material)Resources.Load("Level/Opaque");
        }
        if (batchableCard == null)
        {
            batchableCard = new HighlighterShaderGroup();
            batchableCard.materialTemplate = (Material)Resources.Load("Level/Card");
        }
        if (batchableFoliage == null)
        {
            batchableFoliage = new HighlighterShaderGroup();
            batchableFoliage.materialTemplate = (Material)Resources.Load("Level/Foliage");
            batchableFoliage.filterMode = FilterMode.Trilinear;
        }
        batchableOpaque.batchableTextures.Clear();
        batchableCard.batchableTextures.Clear();
        batchableFoliage.batchableTextures.Clear();
        batchableGameObjects.Clear();
    }

    public static void collectBatch(List<GameObject> targets)
    {
        if (targets.Count == 0)
        {
            return;
        }
        batchableMaterials.Clear();
        List<GameObject> list = new List<GameObject>();
        batchableGameObjects.Add(list);
        for (int i = 0; i < targets.Count; i++)
        {
            GameObject gameObject = targets[i];
            lods.Clear();
            gameObject.GetComponentsInChildren(lods);
            for (int j = 0; j < lods.Count; j++)
            {
                MeshFilter meshFilter = lods[j];
                MeshRenderer component = meshFilter.gameObject.GetComponent<MeshRenderer>();
                checkBatchable(list, meshFilter, component);
            }
        }
        if (batchableMaterials.Count <= 0)
        {
            return;
        }
        List<GameObject>[] array = new List<GameObject>[batchableMaterials.Count];
        batchableMaterials.Values.CopyTo(array, 0);
        for (int k = 0; k < array.Length; k++)
        {
            if (array[k].Count >= 2)
            {
                StaticBatchingUtility.Combine(array[k].ToArray(), Level.roots.gameObject);
            }
        }
    }

    public static void endBatch()
    {
        batch(batchableOpaque);
        batch(batchableCard);
        batch(batchableFoliage);
        for (int i = 0; i < batchableGameObjects.Count; i++)
        {
            if (batchableGameObjects[i].Count != 0)
            {
                StaticBatchingUtility.Combine(batchableGameObjects[i].ToArray(), Level.roots.gameObject);
            }
        }
    }
}
