using System;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned;

/// <summary>
/// "Single-Render" scope as opposed to "Dual-Render" (rendering the scene a second time with a zoomed-in camera).
/// Blits middle square of the player's view into the viewmodel scope material's render target.
/// </summary>
[Serializable]
[PostProcess(typeof(SrScopeRenderer), PostProcessEvent.AfterStack, "Custom/Scope", true)]
public sealed class SrScope : PostProcessEffectSettings
{
    public FloatParameter standardDeviation = new FloatParameter();

    public TextureParameter renderTarget = new TextureParameter();
}
