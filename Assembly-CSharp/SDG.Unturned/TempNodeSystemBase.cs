using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public abstract class TempNodeSystemBase
{
    internal void Instantiate(Vector3 position)
    {
        DevkitTypeFactory.instantiate(GetComponentType(), position, Quaternion.identity, Vector3.one);
    }

    internal abstract Type GetComponentType();

    internal abstract IEnumerable<GameObject> EnumerateGameObjects();
}
