using System;
using UnityEngine;

namespace SDG.Unturned;

public class LevelStructures
{
    private static Transform _models;

    [Obsolete("Was the parent of all structures in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Structures";
                _models.parent = Level.spawns;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelStructures.models which has been deprecated.");
            }
            return _models;
        }
    }
}
