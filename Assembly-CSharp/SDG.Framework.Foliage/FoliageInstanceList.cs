using System.Collections.Generic;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageInstanceList : IPoolable
{
    public AssetReference<FoliageInstancedMeshInfoAsset> assetReference;

    public List<List<Matrix4x4>> matrices { get; protected set; }

    public List<List<bool>> clearWhenBaked { get; protected set; }

    public bool isAssetLoaded { get; protected set; }

    public Mesh mesh { get; protected set; }

    public Material material { get; protected set; }

    public bool castShadows { get; protected set; }

    public bool tileDither { get; protected set; }

    public int sqrDrawDistance { get; protected set; }

    public virtual void poolClaim()
    {
    }

    public virtual void poolRelease()
    {
        assetReference = AssetReference<FoliageInstancedMeshInfoAsset>.invalid;
        foreach (List<Matrix4x4> matrix in matrices)
        {
            ListPool<Matrix4x4>.release(matrix);
        }
        matrices.Clear();
        foreach (List<bool> item in clearWhenBaked)
        {
            ListPool<bool>.release(item);
        }
        clearWhenBaked.Clear();
        isAssetLoaded = false;
        mesh = null;
        material = null;
    }

    public bool IsListEmpty()
    {
        foreach (List<Matrix4x4> matrix in matrices)
        {
            if (matrix.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

    public virtual void clearGeneratedInstances()
    {
        for (int i = 0; i < matrices.Count; i++)
        {
            List<Matrix4x4> list = matrices[i];
            List<bool> list2 = clearWhenBaked[i];
            for (int num = list.Count - 1; num >= 0; num--)
            {
                if (list2[num])
                {
                    list.RemoveAt(num);
                    list2.RemoveAt(num);
                }
            }
        }
    }

    public virtual void applyScale()
    {
        FoliageInstancedMeshInfoAsset foliageInstancedMeshInfoAsset = Assets.find(assetReference);
        if (foliageInstancedMeshInfoAsset == null)
        {
            return;
        }
        for (int i = 0; i < matrices.Count; i++)
        {
            List<Matrix4x4> list = matrices[i];
            _ = clearWhenBaked[i];
            for (int num = list.Count - 1; num >= 0; num--)
            {
                Matrix4x4 matrix = list[num];
                Vector3 position = matrix.GetPosition();
                Quaternion rotation = matrix.GetRotation();
                Vector3 randomScale = foliageInstancedMeshInfoAsset.randomScale;
                matrix = (list[num] = Matrix4x4.TRS(position, rotation, randomScale));
            }
        }
    }

    protected virtual void getOrAddLists(out List<Matrix4x4> matrixList, out List<bool> clearWhenBakedList)
    {
        matrixList = null;
        foreach (List<Matrix4x4> matrix in matrices)
        {
            if (matrix.Count < 1023)
            {
                matrixList = matrix;
                break;
            }
        }
        if (matrixList == null)
        {
            matrixList = ListPool<Matrix4x4>.claim();
            matrices.Add(matrixList);
        }
        clearWhenBakedList = null;
        foreach (List<bool> item in clearWhenBaked)
        {
            if (item.Count < 1023)
            {
                clearWhenBakedList = item;
                break;
            }
        }
        if (clearWhenBakedList == null)
        {
            clearWhenBakedList = ListPool<bool>.claim();
            clearWhenBaked.Add(clearWhenBakedList);
        }
    }

    public virtual void addInstanceRandom(FoliageInstanceGroup group)
    {
        getOrAddLists(out var matrixList, out var clearWhenBakedList);
        int index = Random.Range(0, matrixList.Count);
        matrixList.Insert(index, group.matrix);
        clearWhenBakedList.Insert(index, group.clearWhenBaked);
    }

    public virtual void addInstanceAppend(FoliageInstanceGroup group)
    {
        getOrAddLists(out var matrixList, out var clearWhenBakedList);
        matrixList.Add(group.matrix);
        clearWhenBakedList.Add(group.clearWhenBaked);
    }

    public virtual void removeInstance(int matricesIndex, int matrixIndex)
    {
        List<Matrix4x4> list = matrices[matricesIndex];
        List<bool> list2 = clearWhenBaked[matricesIndex];
        list.RemoveAt(matrixIndex);
        list2.RemoveAt(matrixIndex);
    }

    public virtual void loadAsset()
    {
        if (isAssetLoaded)
        {
            return;
        }
        isAssetLoaded = true;
        FoliageInstancedMeshInfoAsset foliageInstancedMeshInfoAsset = assetReference.Find();
        ClientAssetIntegrity.QueueRequest(assetReference.GUID, foliageInstancedMeshInfoAsset, "Foliage");
        if (foliageInstancedMeshInfoAsset == null)
        {
            return;
        }
        if (Level.shouldUseHolidayRedirects)
        {
            AssetReference<FoliageInstancedMeshInfoAsset>? holidayRedirect = foliageInstancedMeshInfoAsset.getHolidayRedirect();
            if (holidayRedirect.HasValue)
            {
                assetReference = holidayRedirect.Value;
                foliageInstancedMeshInfoAsset = assetReference.Find();
                if (foliageInstancedMeshInfoAsset == null)
                {
                    return;
                }
            }
        }
        mesh = Assets.load(foliageInstancedMeshInfoAsset.mesh);
        material = Assets.load(foliageInstancedMeshInfoAsset.material);
        if (material != null && !material.enableInstancing)
        {
            material.enableInstancing = true;
        }
        castShadows = foliageInstancedMeshInfoAsset.castShadows;
        tileDither = foliageInstancedMeshInfoAsset.tileDither;
        if (foliageInstancedMeshInfoAsset.drawDistance == -1)
        {
            sqrDrawDistance = -1;
        }
        else
        {
            sqrDrawDistance = foliageInstancedMeshInfoAsset.drawDistance * foliageInstancedMeshInfoAsset.drawDistance;
        }
    }

    public FoliageInstanceList()
    {
        matrices = new List<List<Matrix4x4>>(1);
        clearWhenBaked = new List<List<bool>>(1);
    }
}
