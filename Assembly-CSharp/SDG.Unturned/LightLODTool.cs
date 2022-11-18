using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LightLODTool
{
    private static List<Light> lightsInChildren = new List<Light>();

    public static void applyLightLOD(Transform transform)
    {
        if (transform == null)
        {
            return;
        }
        lightsInChildren.Clear();
        transform.GetComponentsInChildren(includeInactive: true, lightsInChildren);
        for (int i = 0; i < lightsInChildren.Count; i++)
        {
            Light light = lightsInChildren[i];
            if (light.type != LightType.Area && light.type != LightType.Directional)
            {
                light.gameObject.AddComponent<LightLOD>().targetLight = light;
            }
        }
    }
}
