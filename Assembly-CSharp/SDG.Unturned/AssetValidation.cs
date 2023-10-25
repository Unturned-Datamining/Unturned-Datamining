using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SDG.Unturned;

public static class AssetValidation
{
    private static List<int> texturePropertyNameIDs = new List<int>();

    private static List<MeshFilter> staticMeshComponents = new List<MeshFilter>();

    private static List<MeshCollider> meshColliderComponents = new List<MeshCollider>();

    private static List<Renderer> allRenderers = new List<Renderer>();

    private static List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    private static List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

    private static List<AudioSource> audioSources = new List<AudioSource>();

    private static List<Cloth> clothComponents = new List<Cloth>();

    public static void ValidateLayersEqual(Asset owningAsset, GameObject gameObject, int expectedLayer)
    {
        int layer = gameObject.layer;
        if (layer != expectedLayer)
        {
            Assets.reportError(owningAsset, "expected '{0}' to have layer {1}, but it actually has layer {2}", gameObject.name, expectedLayer, layer);
        }
    }

    public static void ValidateLayersEqualRecursive(Asset owningAsset, GameObject gameObject, int expectedLayer)
    {
        ValidateLayersEqual(owningAsset, gameObject, expectedLayer);
        foreach (Transform item in gameObject.transform)
        {
            ValidateLayersEqualRecursive(owningAsset, item.gameObject, expectedLayer);
        }
    }

    public static void ValidateClothComponents(Asset owningAsset, GameObject gameObject)
    {
        clothComponents.Clear();
        gameObject.GetComponentsInChildren(clothComponents);
        foreach (Cloth clothComponent in clothComponents)
        {
            if (clothComponent.capsuleColliders.Length != 0 || clothComponent.sphereColliders.Length != 0)
            {
                Assets.reportError(owningAsset, gameObject.name + " cloth component \"" + clothComponent.name + "\" has colliders which is problematic because unfortunately the game does not yet have a way for weapons to ignore them");
            }
        }
    }

    /// <summary>
    /// Relatively efficiently find mesh components, and log an error if their mesh is missing, among other checks.
    /// </summary>
    public static void searchGameObjectForErrors(Asset owningAsset, GameObject gameObject)
    {
        if (gameObject == null)
        {
            throw new ArgumentNullException("gameObject");
        }
        if (!Assets.shouldValidateAssets)
        {
            return;
        }
        staticMeshComponents.Clear();
        gameObject.GetComponentsInChildren(staticMeshComponents);
        foreach (MeshFilter staticMeshComponent in staticMeshComponents)
        {
            internalValidateMesh(owningAsset, gameObject, staticMeshComponent, staticMeshComponent.sharedMesh, 20000);
        }
        meshColliderComponents.Clear();
        gameObject.GetComponentsInChildren(meshColliderComponents);
        foreach (MeshCollider meshColliderComponent in meshColliderComponents)
        {
            internalValidateMesh(owningAsset, gameObject, meshColliderComponent, meshColliderComponent.sharedMesh, 10000);
        }
        allRenderers.Clear();
        gameObject.GetComponentsInChildren(allRenderers);
        foreach (Renderer allRenderer in allRenderers)
        {
            if (allRenderer.motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion)
            {
                Assets.reportError(owningAsset, "{0} Renderer \"{1}\" motion vectors disabled could be a problem for TAA", gameObject.name, allRenderer.name);
            }
        }
        meshRenderers.Clear();
        gameObject.GetComponentsInChildren(meshRenderers);
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (meshRenderer.GetComponent<MeshFilter>() == null)
            {
                if (meshRenderer.GetComponent<TextMeshPro>() == null)
                {
                    Assets.reportError(owningAsset, "{0} missing MeshFilter or TextMesh for MeshRenderer '{1}'", gameObject.name, meshRenderer.name);
                }
            }
            else if (meshRenderer.name != "DepthMask")
            {
                internalValidateRendererMaterials(owningAsset, gameObject, meshRenderer);
            }
        }
        skinnedMeshRenderers.Clear();
        gameObject.GetComponentsInChildren(skinnedMeshRenderers);
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            internalValidateMesh(owningAsset, gameObject, skinnedMeshRenderer, skinnedMeshRenderer.sharedMesh, 20000);
            internalValidateRendererMaterials(owningAsset, gameObject, skinnedMeshRenderer);
        }
        audioSources.Clear();
        gameObject.GetComponentsInChildren(audioSources);
        foreach (AudioSource audioSource in audioSources)
        {
            AudioClip clip = audioSource.clip;
            if (clip != null && clip.samples > 2000000)
            {
                Assets.reportError(owningAsset, "{0} clip '{1}' for AudioSource '{2}' has {3} samples (ideal maximum of {4}) and could be compressed.", gameObject.name, clip.name, audioSource.name, clip.samples, 2000000);
            }
        }
    }

    private static void internalValidateMesh(Asset owningAsset, GameObject gameObject, Component component, Mesh sharedMesh, int maximumVertexCount)
    {
        if (sharedMesh == null)
        {
            if (!(component.GetComponent<TextMeshPro>() != null))
            {
                Assets.reportError(owningAsset, "{0} missing mesh for {1} '{2}'", gameObject.name, component.GetType().Name, component.name);
            }
        }
        else if (sharedMesh.vertexCount > maximumVertexCount)
        {
            Assets.reportError(owningAsset, "{0} mesh for {1} '{2}' has {3} vertices (ideal maximum of {4}) and could be optimized.", gameObject.name, component.GetType().Name, component.name, sharedMesh.vertexCount, maximumVertexCount);
        }
    }

    private static void internalValidateRendererMaterials(Asset owningAsset, GameObject gameObject, Renderer component)
    {
        int num = component.sharedMaterials.Length;
        if (num == 0)
        {
            Assets.reportError(owningAsset, "{0} missing materials for Renderer '{1}'", gameObject.name, component.name);
            return;
        }
        if (num > 4)
        {
            Assets.reportError(owningAsset, "{0} Renderer '{1}' has {2} separate materials (ideal maximum of {3}) which should be optimized to reduce draw calls.", gameObject.name, component.name, num, 4);
        }
        for (int i = 0; i < num; i++)
        {
            Material material = component.sharedMaterials[i];
            if (material == null)
            {
                Assets.reportError(owningAsset, "{0} missing material[{1}] for Renderer '{2}'", gameObject.name, i, component.name);
            }
            else
            {
                internalValidateMaterialTextures(owningAsset, gameObject, component, material);
            }
        }
    }

    private static void internalValidateMaterialTextures(Asset owningAsset, GameObject gameObject, Renderer component, Material sharedMaterial)
    {
        texturePropertyNameIDs.Clear();
        sharedMaterial.GetTexturePropertyNameIDs(texturePropertyNameIDs);
        foreach (int texturePropertyNameID in texturePropertyNameIDs)
        {
            Texture texture = sharedMaterial.GetTexture(texturePropertyNameID);
            if (texture != null && !owningAsset.ignoreTextureReadable && texture.isReadable)
            {
                Assets.reportError(owningAsset, "{0} texture '{1}' referenced by material '{2}' used by Renderer '{3}' can save memory by disabling read/write.", gameObject.name, texture.name, sharedMaterial.name, component.name);
            }
            Texture2D texture2D = texture as Texture2D;
            if (texture2D != null && !owningAsset.ignoreNPOT && (!Mathf.IsPowerOfTwo(texture2D.width) || !Mathf.IsPowerOfTwo(texture2D.height)))
            {
                Assets.reportError(owningAsset, "{0} texture '{1}' referenced by material '{2}' used by Renderer '{3}' has NPOT dimensions ({4} x {5})", gameObject.name, texture.name, sharedMaterial.name, component.name, texture2D.width, texture2D.height);
            }
        }
    }
}
