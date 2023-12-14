using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

public class DecalRenderer : MonoBehaviour
{
    private static readonly RenderTargetIdentifier[] DIFFUSE = new RenderTargetIdentifier[2]
    {
        BuiltinRenderTextureType.GBuffer0,
        BuiltinRenderTextureType.CameraTarget
    };

    public Mesh cube;

    private Camera cam;

    private CommandBuffer buffer;

    private int ambientEquatorID;

    private int ambientSkyID;

    private int ambientGroundID;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        if (cam != null && buffer == null)
        {
            buffer = new CommandBuffer();
            buffer.name = "Decals";
            cam.AddCommandBuffer(CameraEvent.BeforeLighting, buffer);
            ambientEquatorID = Shader.PropertyToID("_DecalHackAmbientEquator");
            ambientSkyID = Shader.PropertyToID("_DecalHackAmbientSky");
            ambientGroundID = Shader.PropertyToID("_DecalHackAmbientGround");
        }
    }

    public void OnDisable()
    {
        if (cam != null && buffer != null)
        {
            cam.RemoveCommandBuffer(CameraEvent.BeforeLighting, buffer);
            buffer = null;
        }
    }

    private void OnPreRender()
    {
        if (cam == null || buffer == null || GraphicsSettings.renderMode != 0)
        {
            return;
        }
        buffer.Clear();
        int num = Shader.PropertyToID("_NormalsCopy");
        buffer.GetTemporaryRT(num, -1, -1);
        buffer.Blit(BuiltinRenderTextureType.GBuffer2, num);
        buffer.SetGlobalVector(ambientEquatorID, RenderSettings.ambientEquatorColor.linear);
        buffer.SetGlobalVector(ambientSkyID, RenderSettings.ambientSkyColor.linear);
        buffer.SetGlobalVector(ambientGroundID, RenderSettings.ambientGroundColor.linear);
        float num2 = 128f + GraphicsSettings.normalizedDrawDistance * 128f;
        buffer.SetRenderTarget(DIFFUSE, BuiltinRenderTextureType.CameraTarget);
        foreach (Decal item in DecalSystem.decalsDiffuse)
        {
            if (!(item.material == null))
            {
                float num3 = num2 * item.lodBias;
                float num4 = num3 * num3;
                if (!((item.transform.position - cam.transform.position).sqrMagnitude > num4))
                {
                    buffer.DrawMesh(cube, item.transform.localToWorldMatrix, item.material);
                }
            }
        }
    }
}
