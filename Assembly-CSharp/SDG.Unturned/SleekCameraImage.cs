using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SDG.Unturned;

public class SleekCameraImage : SleekWrapper
{
    public ISleekImage internalImage;

    private RenderTexture renderTexture;

    private Camera targetCamera;

    public void SetCamera(Camera camera)
    {
        if (targetCamera != null)
        {
            DestroyRenderTexture();
        }
        targetCamera = camera;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (targetCamera == null)
        {
            return;
        }
        Vector2 absoluteSize = GetAbsoluteSize();
        int num = Mathf.CeilToInt(absoluteSize.x);
        int num2 = Mathf.CeilToInt(absoluteSize.y);
        if (num >= 1 && num2 >= 1)
        {
            if (renderTexture != null && (renderTexture.width != num || renderTexture.height != num2))
            {
                DestroyRenderTexture();
            }
            if (renderTexture == null)
            {
                GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB;
                GraphicsFormat depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
                renderTexture = new RenderTexture(num, num2, colorFormat, depthStencilFormat);
                renderTexture.hideFlags = HideFlags.HideAndDontSave;
                renderTexture.filterMode = FilterMode.Point;
                targetCamera.targetTexture = renderTexture;
                internalImage.Texture = renderTexture;
            }
        }
    }

    public override void OnDestroy()
    {
        DestroyRenderTexture();
        base.OnDestroy();
    }

    public SleekCameraImage()
    {
        internalImage = Glazier.Get().CreateImage();
        internalImage.SizeScale_X = 1f;
        internalImage.SizeScale_Y = 1f;
        AddChild(internalImage);
    }

    private void DestroyRenderTexture()
    {
        if (targetCamera != null)
        {
            targetCamera.targetTexture = null;
        }
        if (internalImage != null)
        {
            internalImage.Texture = null;
        }
        if (renderTexture != null)
        {
            Object.Destroy(renderTexture);
            renderTexture = null;
        }
    }
}
