using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class BarricadeRegion
{
    private List<BarricadeDrop> _drops;

    private List<BarricadeData> _barricades;

    private Transform _parent;

    public bool isNetworked;

    internal bool isPendingDestroy;

    public List<BarricadeDrop> drops => _drops;

    [Obsolete("Maintaining two separate lists was error prone, but still kept for backwards compat")]
    public List<BarricadeData> barricades => _barricades;

    public Transform parent => _parent;

    /// <summary>
    /// New code should not use this. Only intended for backwards compatibility.
    /// </summary>
    public int IndexOfBarricadeByRootTransform(Transform rootTransform)
    {
        for (int i = 0; i < _drops.Count; i++)
        {
            if (_drops[i].model == rootTransform)
            {
                return i;
            }
        }
        return -1;
    }

    public BarricadeDrop FindBarricadeByRootTransform(Transform transform)
    {
        foreach (BarricadeDrop drop in _drops)
        {
            if (drop.model == transform)
            {
                return drop;
            }
        }
        return null;
    }

    /// <summary>
    /// Ideally the interactable components should have a reference to their barricade, but that will maybe happen
    /// after the NetId rewrites. For the meantime this is to avoid calling FindBarricadeByRootTransform. If we go
    /// the component route then FindBarricadeByRootTransform will do the same as this method.
    /// </summary>
    internal BarricadeDrop FindBarricadeByRootFast(Transform rootTransform)
    {
        return rootTransform.GetComponent<BarricadeRefComponent>().tempNotSureIfBarricadeShouldBeAComponentYet;
    }

    [Obsolete("Dead code, please contact if you need this and we will make a plan")]
    public BarricadeData findBarricadeByInstanceID(uint instanceID)
    {
        foreach (BarricadeData barricade in barricades)
        {
            if (barricade.instanceID == instanceID)
            {
                return barricade;
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
        _drops.GetAndRemoveTail().CustomDestroy();
    }

    internal void DestroyAll()
    {
        foreach (BarricadeDrop drop in _drops)
        {
            drop.CustomDestroy();
        }
        drops.Clear();
    }

    public BarricadeRegion(Transform newParent)
    {
        _drops = new List<BarricadeDrop>();
        _barricades = new List<BarricadeData>();
        _parent = newParent;
        isNetworked = false;
        isPendingDestroy = false;
    }
}
