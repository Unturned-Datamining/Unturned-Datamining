using SDG.Unturned;
using UnityEngine;

public class PlanarReflection : MonoBehaviour
{
    private static readonly int CULLING_MASK_LOW = -2146435072;

    private static readonly int CULLING_MASK_MEDIUM = CULLING_MASK_LOW | 0x40000 | 0x8000 | 0x4000 | 0x10000000;

    private static readonly int CULLING_MASK_HIGH = CULLING_MASK_MEDIUM | 0x10000 | 0x8000000 | 0x4000000;

    private static readonly int CULLING_MASK_ULTRA = CULLING_MASK_HIGH | 0x400 | 0x1000 | 0x800000 | 0x1000000;

    public LayerMask reflectionMask;

    public bool reflectSkybox;

    public Color clearColor = Color.grey;

    public string reflectionSampler = "_ReflectionTex";

    public float clipPlaneOffset = 0.07f;

    private Vector3 oldpos = Vector3.zero;

    private Camera reflectionCamera;

    private bool helped;

    public Material sharedMaterial;

    private int settingsUpdateIndex = -1;

    public static event PlanarReflectionPreRenderHandler preRender;

    public static event PlanarReflectionPostRenderHandler postRender;

    public void Start()
    {
        if (sharedMaterial == null)
        {
            sharedMaterial = base.transform.GetChild(0).GetChild(0).GetComponent<Renderer>()
                .material;
        }
    }

    private Camera CreateReflectionCameraFor(Camera cam)
    {
        Camera camera = new GameObject(base.gameObject.name + "Reflection" + cam.name).AddComponent<Camera>();
        camera.nearClipPlane = cam.nearClipPlane;
        camera.farClipPlane = cam.farClipPlane;
        camera.backgroundColor = clearColor;
        camera.clearFlags = (reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Color);
        camera.backgroundColor = Color.black;
        camera.enabled = false;
        if (!camera.targetTexture)
        {
            camera.targetTexture = CreateTextureFor(cam);
        }
        return camera;
    }

    private RenderTexture CreateTextureFor(Camera cam)
    {
        return new RenderTexture(Mathf.RoundToInt((float)cam.pixelWidth * 0.5f), Mathf.RoundToInt((float)cam.pixelHeight * 0.5f), 16)
        {
            name = "PlanarReflection_RT"
        };
    }

    public void RenderHelpCameras(Camera currentCam)
    {
        if (!reflectionCamera)
        {
            reflectionCamera = CreateReflectionCameraFor(currentCam);
        }
        RenderReflectionFor(currentCam, reflectionCamera);
    }

    public void LateUpdate()
    {
        helped = false;
    }

    public void WaterTileBeingRendered(Transform tr, Camera currentCam)
    {
        if (base.enabled && currentCam.CompareTag("MainCamera"))
        {
            if (!helped)
            {
                helped = true;
                RenderHelpCameras(currentCam);
                if (reflectionCamera != null && sharedMaterial != null)
                {
                    sharedMaterial.EnableKeyword("WATER_REFLECTIVE");
                    sharedMaterial.DisableKeyword("WATER_SIMPLE");
                    sharedMaterial.SetTexture(reflectionSampler, reflectionCamera.targetTexture);
                }
            }
        }
        else if (reflectionCamera != null && sharedMaterial != null)
        {
            sharedMaterial.DisableKeyword("WATER_REFLECTIVE");
            sharedMaterial.EnableKeyword("WATER_SIMPLE");
            sharedMaterial.SetTexture(reflectionSampler, null);
        }
    }

    private void RenderReflectionFor(Camera cam, Camera reflectCamera)
    {
        if (!reflectCamera || ((bool)sharedMaterial && !sharedMaterial.HasProperty(reflectionSampler)))
        {
            return;
        }
        if (settingsUpdateIndex < GraphicsSettings.planarReflectionUpdateIndex)
        {
            settingsUpdateIndex = GraphicsSettings.planarReflectionUpdateIndex;
            switch (GraphicsSettings.planarReflectionQuality)
            {
            case EGraphicQuality.LOW:
                reflectCamera.cullingMask = CULLING_MASK_LOW;
                break;
            case EGraphicQuality.MEDIUM:
                reflectCamera.cullingMask = CULLING_MASK_MEDIUM;
                break;
            case EGraphicQuality.HIGH:
                reflectCamera.cullingMask = CULLING_MASK_HIGH;
                break;
            case EGraphicQuality.ULTRA:
                reflectCamera.cullingMask = CULLING_MASK_ULTRA;
                break;
            }
            reflectCamera.layerCullDistances = cam.layerCullDistances;
            reflectCamera.layerCullSpherical = cam.layerCullSpherical;
        }
        reflectCamera.fieldOfView = cam.fieldOfView;
        SaneCameraSettings(reflectCamera);
        GL.invertCulling = true;
        Transform transform = base.transform;
        Vector3 eulerAngles = cam.transform.eulerAngles;
        reflectCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
        reflectCamera.transform.position = cam.transform.position;
        Vector3 position = transform.transform.position;
        position.y = transform.position.y;
        Vector3 up = transform.transform.up;
        float w = 0f - Vector3.Dot(up, position) - clipPlaneOffset;
        Vector4 plane = new Vector4(up.x, up.y, up.z, w);
        Matrix4x4 zero = Matrix4x4.zero;
        zero = CalculateReflectionMatrix(zero, plane);
        oldpos = cam.transform.position;
        Vector3 position2 = zero.MultiplyPoint(oldpos);
        reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * zero;
        Vector4 clipPlane = CameraSpacePlane(reflectCamera, position, up, 1f);
        reflectCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
        reflectCamera.transform.position = position2;
        Vector3 eulerAngles2 = cam.transform.eulerAngles;
        reflectCamera.transform.eulerAngles = new Vector3(0f - eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
        float lodBias = QualitySettings.lodBias;
        QualitySettings.lodBias = 1f;
        PlanarReflection.preRender?.Invoke();
        reflectCamera.Render();
        PlanarReflection.postRender?.Invoke();
        QualitySettings.lodBias = lodBias;
        GL.invertCulling = false;
    }

    private void SaneCameraSettings(Camera helperCam)
    {
        helperCam.renderingPath = RenderingPath.Forward;
        helperCam.allowHDR = true;
    }

    private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
        reflectionMat.m01 = -2f * plane[0] * plane[1];
        reflectionMat.m02 = -2f * plane[0] * plane[2];
        reflectionMat.m03 = -2f * plane[3] * plane[0];
        reflectionMat.m10 = -2f * plane[1] * plane[0];
        reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
        reflectionMat.m12 = -2f * plane[1] * plane[2];
        reflectionMat.m13 = -2f * plane[3] * plane[1];
        reflectionMat.m20 = -2f * plane[2] * plane[0];
        reflectionMat.m21 = -2f * plane[2] * plane[1];
        reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
        reflectionMat.m23 = -2f * plane[3] * plane[2];
        reflectionMat.m30 = 0f;
        reflectionMat.m31 = 0f;
        reflectionMat.m32 = 0f;
        reflectionMat.m33 = 1f;
        return reflectionMat;
    }

    private static float sgn(float a)
    {
        if (a > 0f)
        {
            return 1f;
        }
        if (a < 0f)
        {
            return -1f;
        }
        return 0f;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 point = pos + normal * clipPlaneOffset;
        Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
        Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
        Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
    }
}
