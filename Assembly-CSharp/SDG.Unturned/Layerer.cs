using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

public class Layerer
{
    public static void relayer(Transform target, int layer)
    {
        if (!(target == null))
        {
            target.gameObject.layer = layer;
            for (int i = 0; i < target.childCount; i++)
            {
                relayer(target.GetChild(i), layer);
            }
        }
    }

    public static void viewmodel(Transform target)
    {
        if (target.GetComponent<Renderer>() != null)
        {
            target.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            target.GetComponent<Renderer>().receiveShadows = false;
            target.tag = "Viewmodel";
            target.gameObject.layer = 11;
            return;
        }
        LODGroup component = target.GetComponent<LODGroup>();
        if (!(component != null))
        {
            return;
        }
        foreach (Renderer item in (IEnumerable<Renderer>)new LodGroupEnumerator(component))
        {
            item.shadowCastingMode = ShadowCastingMode.Off;
            item.receiveShadows = false;
            item.gameObject.tag = "Viewmodel";
            item.gameObject.layer = 11;
        }
    }

    public static void enemy(Transform target)
    {
        if (target.GetComponent<Renderer>() != null)
        {
            target.tag = "Enemy";
            target.gameObject.layer = 10;
            return;
        }
        LODGroup component = target.GetComponent<LODGroup>();
        if (!(component != null))
        {
            return;
        }
        foreach (Renderer item in (IEnumerable<Renderer>)new LodGroupEnumerator(component))
        {
            item.gameObject.tag = "Enemy";
            item.gameObject.layer = 10;
        }
    }
}
