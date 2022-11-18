using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class HighlighterShaderGroup
{
    public Material materialTemplate;

    public Dictionary<Texture2D, HighlighterBatch> batchableTextures = new Dictionary<Texture2D, HighlighterBatch>();

    public FilterMode filterMode;
}
