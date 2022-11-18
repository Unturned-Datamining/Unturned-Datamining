using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class HighlighterBatch
{
    public Texture2D texture;

    public Dictionary<Mesh, List<MeshFilter>> meshes = new Dictionary<Mesh, List<MeshFilter>>();

    public List<MeshRenderer> renderers = new List<MeshRenderer>();
}
