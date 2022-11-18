using System;
using UnityEngine;

namespace SDG.Unturned;

public class LevelBarricades
{
    private static Transform _models;

    [Obsolete("Was the parent of all barricades in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Barricades";
                _models.parent = Level.spawns;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelBarricades.models which has been deprecated.");
            }
            return _models;
        }
    }
}
