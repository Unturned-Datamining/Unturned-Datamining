using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class StructureRegion
{
    private List<StructureDrop> _drops;

    private List<StructureData> _structures;

    public bool isNetworked;

    internal bool isPendingDestroy;

    public List<StructureDrop> drops => _drops;

    [Obsolete("Maintaining two separate lists was error prone, but still kept for backwards compat")]
    public List<StructureData> structures => _structures;

    public StructureDrop FindStructureByRootTransform(Transform transform)
    {
        foreach (StructureDrop drop in _drops)
        {
            if (drop.model == transform)
            {
                return drop;
            }
        }
        return null;
    }

    [Obsolete("Dead code, please contact if you need this and we will make a plan")]
    public StructureData findStructureByInstanceID(uint instanceID)
    {
        foreach (StructureData structure in structures)
        {
            if (structure.instanceID == instanceID)
            {
                return structure;
            }
        }
        return null;
    }

    [Obsolete("Renamed to DestroyAll")]
    public void destroy()
    {
        DestroyAll();
    }

    internal void DestroyTail()
    {
        StructureDrop andRemoveTail = _drops.GetAndRemoveTail();
        try
        {
            andRemoveTail.ReleaseNetId();
            StructureManager.instance.DestroyOrReleaseStructure(andRemoveTail);
            andRemoveTail.model.position = Vector3.zero;
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception destroying structure:");
        }
    }

    internal void DestroyAll()
    {
        foreach (StructureDrop drop in _drops)
        {
            try
            {
                drop.ReleaseNetId();
                StructureManager.instance.DestroyOrReleaseStructure(drop);
                drop.model.position = Vector3.zero;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Exception destroying structure:");
            }
        }
        drops.Clear();
    }

    public StructureRegion()
    {
        _drops = new List<StructureDrop>();
        _structures = new List<StructureData>();
        isNetworked = false;
        isPendingDestroy = false;
    }
}
