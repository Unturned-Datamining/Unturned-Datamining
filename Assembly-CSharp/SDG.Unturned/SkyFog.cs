using System;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned;

[Serializable]
[PostProcess(typeof(SkyFogRenderer), PostProcessEvent.BeforeTransparent, "Custom/SkyFog", true)]
public sealed class SkyFog : PostProcessEffectSettings
{
}
