using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows foreach loop to iterate renderers defined in lod group.
/// </summary>
public struct LodGroupEnumerator : IEnumerator<Renderer>, IEnumerator, IDisposable, IEnumerable<Renderer>, IEnumerable
{
    private LOD[] lods;

    private int lodIndex;

    private int rendererIndex;

    public Renderer Current => lods[lodIndex].renderers[rendererIndex];

    object IEnumerator.Current => Current;

    public LodGroupEnumerator(LODGroup lodGroup)
    {
        lods = lodGroup.GetLODs();
        lodIndex = 0;
        rendererIndex = -1;
    }

    public void Dispose()
    {
    }

    private bool MoveRendererIndex()
    {
        Renderer[] renderers = lods[lodIndex].renderers;
        if (renderers == null || renderers.Length < 1)
        {
            return false;
        }
        while (++rendererIndex < renderers.Length)
        {
            if (!(renderers[rendererIndex] == null))
            {
                return true;
            }
        }
        return false;
    }

    public bool MoveNext()
    {
        if (lods == null || lods.Length < 1)
        {
            return false;
        }
        if (MoveRendererIndex())
        {
            return true;
        }
        while (++lodIndex < lods.Length)
        {
            rendererIndex = -1;
            if (MoveRendererIndex())
            {
                return true;
            }
        }
        return false;
    }

    public void Reset()
    {
        lodIndex = 0;
        rendererIndex = -1;
    }

    IEnumerator<Renderer> IEnumerable<Renderer>.GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }
}
