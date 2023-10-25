using System;
using System.Collections.Generic;
using SDG.Framework.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

/// <summary>
/// In-game debug drawing utility similar to Unity's editor Gizmos.
/// </summary>
public class RuntimeGizmos : MonoBehaviour
{
    private struct BoxData
    {
        public Matrix4x4 matrix;

        /// <summary>
        /// Center relative to matrix.
        /// </summary>
        public Vector3 localCenter;

        public Vector3 size;

        public Vector3 extents;

        public Color color;

        public float expireAfter;

        public BoxData(Matrix4x4 matrix, Vector3 localCenter, Vector3 size, Color color, float lifespan)
        {
            this.matrix = matrix;
            this.localCenter = localCenter;
            this.size = size;
            extents = size * 0.5f;
            this.color = color;
            expireAfter = Time.time + lifespan;
        }
    }

    private struct LineData
    {
        public Vector3 begin;

        public Vector3 end;

        public Color color;

        public float expireAfter;

        public LineData(Vector3 begin, Vector3 end, Color color, float lifespan)
        {
            this.begin = begin;
            this.end = end;
            this.color = color;
            expireAfter = Time.time + lifespan;
        }
    }

    private struct CapsuleData
    {
        public Vector3 begin;

        public Vector3 end;

        public Color color;

        public float expireAfter;

        public float radius;

        public CapsuleData(Vector3 begin, Vector3 end, Color color, float lifespan, float radius)
        {
            this.begin = begin;
            this.end = end;
            this.color = color;
            expireAfter = Time.time + lifespan;
            this.radius = radius;
        }
    }

    private struct SphereData
    {
        public Matrix4x4 matrix;

        /// <summary>
        /// Center relative to matrix.
        /// </summary>
        public Vector3 localCenter;

        public Color color;

        public float expireAfter;

        public float localRadius;

        public int circleResolution;

        public SphereData(Matrix4x4 matrix, Vector3 localCenter, Color color, float lifespan, float localRadius)
        {
            this.matrix = matrix;
            this.localCenter = localCenter;
            this.color = color;
            expireAfter = Time.time + lifespan;
            this.localRadius = localRadius;
            float max = matrix.lossyScale.GetAbs().GetMax();
            circleResolution = Mathf.Clamp(Mathf.RoundToInt(8f * localRadius * max), 8, 64);
        }
    }

    private struct CircleData
    {
        public Vector3 center;

        public Vector3 axisU;

        public Vector3 axisV;

        public Color color;

        public float expireAfter;

        public float radius;

        public int resolution;

        public CircleData(Vector3 center, Vector3 axisU, Vector3 axisV, Color color, float lifespan, float radius, int resolution)
        {
            this.center = center;
            this.axisU = axisU;
            this.axisV = axisV;
            this.color = color;
            expireAfter = Time.time + lifespan;
            this.radius = radius;
            this.resolution = ((resolution > 0) ? resolution : Mathf.Clamp(Mathf.RoundToInt(8f * radius), 8, 64));
        }
    }

    private struct LabelData
    {
        public Vector3 position;

        public string content;

        public Color color;

        public float expireAfter;

        public LabelData(Vector3 position, string content, Color color, float lifespan)
        {
            this.position = position;
            this.content = content;
            this.color = color;
            expireAfter = Time.time + lifespan;
        }
    }

    private List<BoxData>[] boxLayers;

    private List<LineData>[] lineLayers;

    private List<CapsuleData>[] capsuleLayers;

    private List<SphereData>[] sphereLayers;

    private List<CircleData>[] circleLayers;

    private List<LabelData> labelsToRender = new List<LabelData>();

    private float renderTime;

    private float cullDistance;

    private float sqrCullDistance;

    private Material[] materialLayers;

    private int[] lineRendererLayers;

    private int lineRendererLayer;

    private List<LineRenderer> lineRendererPool = new List<LineRenderer>();

    private Dictionary<int, List<AnimationCurve>> animationCurvePool = new Dictionary<int, List<AnimationCurve>>();

    private List<LineRenderer> activeLineRenderers = new List<LineRenderer>();

    private Material lineRendererSharedMaterial;

    private Vector3 mainCameraPosition;

    private Camera lineRendererForegroundCamera;

    private static RuntimeGizmos instance;

    private const int LAYER_COUNT = 2;

    private const float CIRCLE_RESOLUTION_MULTIPLIER = 8f;

    private const int MIN_CIRCLE_RESOLUTION = 8;

    private const int MAX_CIRCLE_RESOLUTION = 64;

    private static CommandLineFlag clUseLineRenderers = new CommandLineFlag(defaultValue: false, "-FallbackGizmos");

    public bool HasQueuedElements
    {
        get
        {
            if (clUseLineRenderers.value)
            {
                return false;
            }
            bool flag = false;
            for (int i = 0; i < 2; i++)
            {
                flag |= boxLayers[i].Count > 0;
                flag |= lineLayers[i].Count > 0;
                flag |= capsuleLayers[i].Count > 0;
                flag |= sphereLayers[i].Count > 0;
                flag |= circleLayers[i].Count > 0;
            }
            return flag;
        }
    }

    public static RuntimeGizmos Get()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("GizmoSingleton");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.DontSave;
            instance = obj.AddComponent<RuntimeGizmos>();
            instance.materialLayers = new Material[2];
            instance.materialLayers[0] = GLUtility.LINE_DEPTH_CHECKERED_COLOR;
            instance.materialLayers[1] = GLUtility.LINE_FLAT_COLOR;
        }
        return instance;
    }

    public void Box(Vector3 center, Vector3 size, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        boxLayers[(int)layer].Add(new BoxData(Matrix4x4.Translate(center), Vector3.zero, size, color, lifespan));
    }

    public void Box(Vector3 center, Quaternion rotation, Vector3 size, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        boxLayers[(int)layer].Add(new BoxData(Matrix4x4.TRS(center, rotation, Vector3.one), Vector3.zero, size, color, lifespan));
    }

    public void Box(Matrix4x4 matrix, Vector3 size, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        boxLayers[(int)layer].Add(new BoxData(matrix, Vector3.zero, size, color, lifespan));
    }

    /// <param name="center">Local space relative to matrix.</param>
    public void Box(Matrix4x4 matrix, Vector3 center, Vector3 size, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        boxLayers[(int)layer].Add(new BoxData(matrix, center, size, color, lifespan));
    }

    public void Cube(Vector3 center, float size, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Box(center, new Vector3(size, size, size), color, lifespan);
    }

    public void Cube(Vector3 center, Quaternion rotation, float size, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Box(center, rotation, new Vector3(size, size, size), color, lifespan);
    }

    public void Line(Vector3 begin, Vector3 end, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        lineLayers[(int)layer].Add(new LineData(begin, end, color, lifespan));
    }

    public void LineToward(Vector3 begin, Vector3 end, float length, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Vector3 normalized = (end - begin).normalized;
        lineLayers[(int)layer].Add(new LineData(begin, begin + normalized * length, color, lifespan));
    }

    public void Arrow(Vector3 origin, Vector3 direction, float length, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Vector3 rhs = ((!(MainCamera.instance != null)) ? Vector3.up : (origin - MainCamera.instance.transform.position).normalized);
        Vector3 vector = Vector3.Cross(direction, rhs).normalized * 0.1f * length;
        Vector3 vector2 = origin + direction * 0.75f * length;
        Vector3 end = origin + direction * length;
        lineLayers[(int)layer].Add(new LineData(origin, end, color, lifespan));
        lineLayers[(int)layer].Add(new LineData(vector2 - vector, end, color, lifespan));
        lineLayers[(int)layer].Add(new LineData(vector2 + vector, end, color, lifespan));
    }

    public void ArrowFromTo(Vector3 begin, Vector3 end, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Vector3 vector = end - begin;
        float magnitude = vector.magnitude;
        if (magnitude > 0.001f)
        {
            Vector3 direction = vector / magnitude;
            Arrow(begin, direction, magnitude, color, lifespan, layer);
        }
    }

    public void Capsule(Vector3 begin, Vector3 end, float radius, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        capsuleLayers[(int)layer].Add(new CapsuleData(begin, end, color, lifespan, radius));
    }

    public void Sphere(Vector3 center, float radius, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        sphereLayers[(int)layer].Add(new SphereData(Matrix4x4.Translate(center), Vector3.zero, color, lifespan, radius));
    }

    public void Sphere(Matrix4x4 matrix, float radius, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        sphereLayers[(int)layer].Add(new SphereData(matrix, Vector3.zero, color, lifespan, radius));
    }

    public void Circle(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, Color color, float lifespan = 0f, int resolution = 0, EGizmoLayer layer = EGizmoLayer.World)
    {
        circleLayers[(int)layer].Add(new CircleData(center, axisU, axisV, color, lifespan, radius, resolution));
    }

    public void Raycast(Ray ray, float maxDistance, RaycastHit hit, Color rayColor, Color hitColor, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Linecast(ray.origin, ray.origin + ray.direction * maxDistance, hit, rayColor, hitColor, lifespan);
    }

    public void Linecast(Vector3 start, Vector3 end, RaycastHit hit, Color rayColor, Color hitColor, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        if (hit.collider == null)
        {
            Line(start, end, rayColor, lifespan);
            return;
        }
        Line(start, hit.point, hitColor, lifespan);
        Line(hit.point, end, rayColor, lifespan);
    }

    public void Spherecast(Ray ray, float radius, float maxDistance, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Capsule(ray.origin, ray.origin + ray.direction * maxDistance, radius, color, lifespan);
    }

    public void Spherecast(Ray ray, float radius, float maxDistance, RaycastHit hit, Color rayColor, Color hitColor, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        if (hit.collider == null)
        {
            Capsule(ray.origin, ray.origin + ray.direction * maxDistance, radius, rayColor, lifespan);
            return;
        }
        Vector3 vector = ray.origin + ray.direction * hit.distance;
        Capsule(ray.origin, vector, radius, hitColor, lifespan);
        Capsule(vector, ray.origin + ray.direction * maxDistance, radius, rayColor, lifespan);
    }

    public void Label(Vector3 position, string content, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        if (!string.IsNullOrEmpty(content))
        {
            labelsToRender.Add(new LabelData(position, content, Color.white, lifespan));
        }
    }

    public void Label(Vector3 position, string content, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        if (!string.IsNullOrEmpty(content))
        {
            labelsToRender.Add(new LabelData(position, content, color, lifespan));
        }
    }

    /// <summary>
    /// Wireframe grid on the XZ plane.
    /// </summary>
    public void GridXZ(Vector3 center, float size, int cells, Color color, float lifespan = 0f, EGizmoLayer layer = EGizmoLayer.World)
    {
        Vector3 vector = center - new Vector3(size * 0.5f, 0f, size * 0.5f);
        float num = size / (float)cells;
        for (int i = 0; i <= cells; i++)
        {
            float num2 = num * (float)i;
            Line(vector + new Vector3(num2, 0f, 0f), vector + new Vector3(num2, 0f, size), color, lifespan);
            Line(vector + new Vector3(0f, 0f, num2), vector + new Vector3(size, 0f, num2), color, lifespan);
        }
    }

    public void Render()
    {
        renderTime = Time.time;
        Camera camera = MainCamera.instance;
        if (camera != null)
        {
            mainCameraPosition = camera.transform.position;
            cullDistance = camera.farClipPlane;
            sqrCullDistance = cullDistance * cullDistance;
        }
        else
        {
            mainCameraPosition = Vector3.zero;
            cullDistance = 0f;
            sqrCullDistance = 0f;
        }
        for (int i = 0; i < 2; i++)
        {
            materialLayers[i].SetPass(0);
            RenderBoxes(boxLayers[i]);
            RenderLines(lineLayers[i]);
            RenderCapsules(capsuleLayers[i]);
            RenderSpheres(sphereLayers[i]);
            RenderCircles(circleLayers[i]);
        }
    }

    private void RenderBoxes(List<BoxData> boxesToRender)
    {
        GL.Begin(1);
        for (int num = boxesToRender.Count - 1; num >= 0; num--)
        {
            BoxData boxData = boxesToRender[num];
            GL.Color(boxData.color);
            Vector3 extents = boxData.extents;
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, 0f - extents.z)));
            GL.Vertex(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, extents.z)));
            if (renderTime >= boxData.expireAfter)
            {
                boxesToRender.RemoveAtFast(num);
            }
        }
        GL.End();
    }

    private void RenderBoxesUsingLineRenderers(List<BoxData> boxesToRender)
    {
        for (int num = boxesToRender.Count - 1; num >= 0; num--)
        {
            BoxData boxData = boxesToRender[num];
            Vector3 extents = boxData.extents;
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, 0f - extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, 0f - extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, 0f - extents.y, extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, 0f - extents.y, extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, 0f - extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(0f - extents.x, extents.y, extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, extents.z)), boxData.color);
            DrawLineUsingLineRenderer(boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, 0f - extents.z)), boxData.matrix.MultiplyPoint3x4(boxData.localCenter + new Vector3(extents.x, extents.y, extents.z)), boxData.color);
            if (renderTime >= boxData.expireAfter)
            {
                boxesToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderLines(List<LineData> linesToRender)
    {
        GL.Begin(1);
        for (int num = linesToRender.Count - 1; num >= 0; num--)
        {
            LineData lineData = linesToRender[num];
            GL.Color(lineData.color);
            GL.Vertex(lineData.begin);
            GL.Vertex(lineData.end);
            if (renderTime >= lineData.expireAfter)
            {
                linesToRender.RemoveAtFast(num);
            }
        }
        GL.End();
    }

    private void RenderLinesUsingLineRenderer(List<LineData> linesToRender)
    {
        for (int num = linesToRender.Count - 1; num >= 0; num--)
        {
            LineData lineData = linesToRender[num];
            DrawLineUsingLineRenderer(lineData.begin, lineData.end, lineData.color);
            if (renderTime >= lineData.expireAfter)
            {
                linesToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderCapsules(List<CapsuleData> capsulesToRender)
    {
        for (int num = capsulesToRender.Count - 1; num >= 0; num--)
        {
            CapsuleData capsuleData = capsulesToRender[num];
            Vector3 vector = capsuleData.end - capsuleData.begin;
            vector.Normalize();
            Vector3 vector2 = Vector3.Cross(vector, (Mathf.Abs(Vector3.Dot(vector, Vector3.up)) > 0.95f) ? Vector3.forward : Vector3.up);
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            int num2 = Mathf.Clamp(Mathf.RoundToInt(8f * capsuleData.radius), 8, 64);
            RenderCircle(capsuleData.begin, vector2, vector3, capsuleData.radius, num2, capsuleData.color);
            RenderCircle(capsuleData.end, vector2, vector3, capsuleData.radius, num2, capsuleData.color);
            int resolution = num2 / 2;
            RenderSemicircle(capsuleData.begin, vector2, -vector, capsuleData.radius, resolution, capsuleData.color);
            RenderSemicircle(capsuleData.begin, vector3, -vector, capsuleData.radius, resolution, capsuleData.color);
            RenderSemicircle(capsuleData.end, vector2, vector, capsuleData.radius, resolution, capsuleData.color);
            RenderSemicircle(capsuleData.end, vector3, vector, capsuleData.radius, resolution, capsuleData.color);
            GL.Begin(1);
            GL.Color(capsuleData.color);
            GL.Vertex(capsuleData.begin + vector2 * capsuleData.radius);
            GL.Vertex(capsuleData.end + vector2 * capsuleData.radius);
            GL.Vertex(capsuleData.begin - vector2 * capsuleData.radius);
            GL.Vertex(capsuleData.end - vector2 * capsuleData.radius);
            GL.Vertex(capsuleData.begin + vector3 * capsuleData.radius);
            GL.Vertex(capsuleData.end + vector3 * capsuleData.radius);
            GL.Vertex(capsuleData.begin - vector3 * capsuleData.radius);
            GL.Vertex(capsuleData.end - vector3 * capsuleData.radius);
            GL.End();
            if (renderTime >= capsuleData.expireAfter)
            {
                capsulesToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderCapsulesUsingLineRenderers(List<CapsuleData> capsulesToRender)
    {
        for (int num = capsulesToRender.Count - 1; num >= 0; num--)
        {
            CapsuleData capsuleData = capsulesToRender[num];
            Vector3 vector = capsuleData.end - capsuleData.begin;
            vector.Normalize();
            Vector3 vector2 = Vector3.Cross(vector, (Mathf.Abs(Vector3.Dot(vector, Vector3.up)) > 0.95f) ? Vector3.forward : Vector3.up);
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            int num2 = Mathf.Clamp(Mathf.RoundToInt(8f * capsuleData.radius), 8, 64);
            DrawCircleUsingLineRenderer(capsuleData.begin, vector2, vector3, capsuleData.radius, num2, capsuleData.color);
            DrawCircleUsingLineRenderer(capsuleData.end, vector2, vector3, capsuleData.radius, num2, capsuleData.color);
            int resolution = num2 / 2;
            DrawSemicircleUsingLineRenderer(capsuleData.begin, vector2, -vector, capsuleData.radius, resolution, capsuleData.color);
            DrawSemicircleUsingLineRenderer(capsuleData.begin, vector3, -vector, capsuleData.radius, resolution, capsuleData.color);
            DrawSemicircleUsingLineRenderer(capsuleData.end, vector2, vector, capsuleData.radius, resolution, capsuleData.color);
            DrawSemicircleUsingLineRenderer(capsuleData.end, vector3, vector, capsuleData.radius, resolution, capsuleData.color);
            DrawLineUsingLineRenderer(capsuleData.begin + vector2 * capsuleData.radius, capsuleData.end + vector2 * capsuleData.radius, capsuleData.color);
            DrawLineUsingLineRenderer(capsuleData.begin - vector2 * capsuleData.radius, capsuleData.end - vector2 * capsuleData.radius, capsuleData.color);
            DrawLineUsingLineRenderer(capsuleData.begin + vector3 * capsuleData.radius, capsuleData.end + vector3 * capsuleData.radius, capsuleData.color);
            DrawLineUsingLineRenderer(capsuleData.begin - vector3 * capsuleData.radius, capsuleData.end - vector3 * capsuleData.radius, capsuleData.color);
            if (renderTime >= capsuleData.expireAfter)
            {
                capsulesToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderSpheres(List<SphereData> spheresToRender)
    {
        for (int num = spheresToRender.Count - 1; num >= 0; num--)
        {
            SphereData sphereData = spheresToRender[num];
            Vector3 vector = sphereData.matrix.MultiplyPoint3x4(sphereData.localCenter);
            float sqrMagnitude = (vector - mainCameraPosition).sqrMagnitude;
            float num2 = sphereData.localRadius * sphereData.localRadius;
            if (sqrMagnitude - num2 < sqrCullDistance)
            {
                Vector3 axisU = sphereData.matrix.MultiplyVector(Vector3.up);
                Vector3 axisV = sphereData.matrix.MultiplyVector(Vector3.forward);
                Vector3 vector2 = sphereData.matrix.MultiplyVector(Vector3.right);
                RenderCircle(vector, axisU, vector2, sphereData.localRadius, sphereData.circleResolution, sphereData.color);
                RenderCircle(vector, axisU, axisV, sphereData.localRadius, sphereData.circleResolution, sphereData.color);
                RenderCircle(vector, vector2, axisV, sphereData.localRadius, sphereData.circleResolution, sphereData.color);
            }
            if (renderTime >= sphereData.expireAfter)
            {
                spheresToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderSpheresUsingLineRenderers(List<SphereData> spheresToRender)
    {
        for (int num = spheresToRender.Count - 1; num >= 0; num--)
        {
            SphereData sphereData = spheresToRender[num];
            Vector3 vector = sphereData.matrix.MultiplyPoint3x4(sphereData.localCenter);
            float sqrMagnitude = (vector - mainCameraPosition).sqrMagnitude;
            float num2 = sphereData.localRadius * sphereData.localRadius;
            if (sqrMagnitude - num2 < sqrCullDistance)
            {
                Vector3 axisU = sphereData.matrix.MultiplyVector(Vector3.up);
                Vector3 axisV = sphereData.matrix.MultiplyVector(Vector3.forward);
                Vector3 vector2 = sphereData.matrix.MultiplyVector(Vector3.right);
                DrawCircleUsingLineRenderer(vector, axisU, vector2, sphereData.localRadius, sphereData.circleResolution, sphereData.color);
                DrawCircleUsingLineRenderer(vector, axisU, axisV, sphereData.localRadius, sphereData.circleResolution, sphereData.color);
                DrawCircleUsingLineRenderer(vector, vector2, axisV, sphereData.localRadius, sphereData.circleResolution, sphereData.color);
            }
            if (renderTime >= sphereData.expireAfter)
            {
                spheresToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderCircles(List<CircleData> circlesToRender)
    {
        for (int num = circlesToRender.Count - 1; num >= 0; num--)
        {
            CircleData circleData = circlesToRender[num];
            float sqrMagnitude = (circleData.center - mainCameraPosition).sqrMagnitude;
            float num2 = circleData.radius * circleData.radius;
            if (sqrMagnitude - num2 < sqrCullDistance)
            {
                RenderCircle(circleData.center, circleData.axisU, circleData.axisV, circleData.radius, circleData.resolution, circleData.color);
            }
            if (renderTime >= circleData.expireAfter)
            {
                circlesToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderCirclesUsingLineRenderers(List<CircleData> circlesToRender)
    {
        for (int num = circlesToRender.Count - 1; num >= 0; num--)
        {
            CircleData circleData = circlesToRender[num];
            float sqrMagnitude = (circleData.center - mainCameraPosition).sqrMagnitude;
            float num2 = circleData.radius * circleData.radius;
            if (sqrMagnitude - num2 < sqrCullDistance)
            {
                DrawCircleUsingLineRenderer(circleData.center, circleData.axisU, circleData.axisV, circleData.radius, circleData.resolution, circleData.color);
            }
            if (renderTime >= circleData.expireAfter)
            {
                circlesToRender.RemoveAtFast(num);
            }
        }
    }

    private void RenderCircle(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
    {
        float num = MathF.PI * 2f / (float)resolution;
        Vector3 v = center + axisU * radius;
        GL.Begin(2);
        GL.Color(color);
        GL.Vertex(v);
        for (int i = 1; i < resolution; i++)
        {
            float f = (float)i * num;
            float num2 = Mathf.Cos(f) * radius;
            float num3 = Mathf.Sin(f) * radius;
            GL.Vertex(center + axisU * num2 + axisV * num3);
        }
        GL.Vertex(v);
        GL.End();
    }

    private void RenderSemicircle(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
    {
        float num = MathF.PI / (float)resolution;
        GL.Begin(2);
        GL.Color(color);
        GL.Vertex(center + axisU * radius);
        for (int i = 1; i < resolution; i++)
        {
            float f = (float)i * num;
            float num2 = Mathf.Cos(f) * radius;
            float num3 = Mathf.Sin(f) * radius;
            GL.Vertex(center + axisU * num2 + axisV * num3);
        }
        GL.Vertex(center - axisU * radius);
        GL.End();
    }

    private void DrawSemicircleUsingLineRenderer(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
    {
        float num = MathF.PI / (float)resolution;
        LineRenderer lineRenderer = ClaimLineRenderer();
        lineRenderer.positionCount = resolution + 1;
        lineRenderer.loop = false;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        Vector3 linePosition = center + axisU * radius;
        AnimationCurve animationCurve = ClaimCurveWithKeyCount(resolution + 1);
        animationCurve.MoveKey(0, new Keyframe(0f, CalculateLineRendererWidth(linePosition)));
        lineRenderer.SetPosition(0, center + axisU * radius);
        for (int i = 1; i < resolution; i++)
        {
            float f = (float)i * num;
            float num2 = Mathf.Cos(f) * radius;
            float num3 = Mathf.Sin(f) * radius;
            Vector3 vector = center + axisU * num2 + axisV * num3;
            animationCurve.MoveKey(i, new Keyframe((float)i / (float)resolution, CalculateLineRendererWidth(vector)));
            lineRenderer.SetPosition(i, vector);
        }
        Vector3 vector2 = center - axisU * radius;
        animationCurve.MoveKey(resolution, new Keyframe(1f, CalculateLineRendererWidth(vector2)));
        lineRenderer.SetPosition(resolution, vector2);
        lineRenderer.widthCurve = animationCurve;
    }

    private LineRenderer ClaimLineRenderer()
    {
        LineRenderer lineRenderer = null;
        while (lineRendererPool.Count > 0 && lineRenderer == null)
        {
            lineRenderer = lineRendererPool.GetAndRemoveTail();
        }
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer = new GameObject("Runtime Gizmo LineRenderer").AddComponent<LineRenderer>();
            lineRenderer.sharedMaterial = lineRendererSharedMaterial;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.numCapVertices = 1;
        }
        lineRenderer.gameObject.layer = lineRendererLayer;
        activeLineRenderers.Add(lineRenderer);
        return lineRenderer;
    }

    private AnimationCurve ClaimCurveWithKeyCount(int count)
    {
        if (!animationCurvePool.TryGetValue(count, out var value))
        {
            value = new List<AnimationCurve>();
            animationCurvePool.Add(count, value);
        }
        if (value.Count > 0)
        {
            return value.GetAndRemoveTail();
        }
        Keyframe[] array = new Keyframe[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = new Keyframe((float)i / (float)(count - 1), 10f);
        }
        return new AnimationCurve(array);
    }

    private float CalculateLineRendererWidth(Vector3 linePosition)
    {
        return (linePosition - mainCameraPosition).magnitude * 0.005f;
    }

    private void DrawLineUsingLineRenderer(Vector3 begin, Vector3 end, Color color)
    {
        LineRenderer lineRenderer = ClaimLineRenderer();
        lineRenderer.positionCount = 2;
        lineRenderer.loop = false;
        lineRenderer.SetPosition(0, begin);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        AnimationCurve animationCurve = ClaimCurveWithKeyCount(2);
        animationCurve.MoveKey(0, new Keyframe(0f, CalculateLineRendererWidth(begin)));
        animationCurve.MoveKey(1, new Keyframe(1f, CalculateLineRendererWidth(end)));
        lineRenderer.widthCurve = animationCurve;
    }

    private void DrawCircleUsingLineRenderer(Vector3 center, Vector3 axisU, Vector3 axisV, float radius, int resolution, Color color)
    {
        float num = MathF.PI * 2f / (float)resolution;
        _ = Vector3.Cross(axisU, axisV).normalized;
        LineRenderer lineRenderer = ClaimLineRenderer();
        lineRenderer.positionCount = resolution;
        lineRenderer.loop = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        Vector3 vector = center + axisU * radius;
        AnimationCurve animationCurve = ClaimCurveWithKeyCount(resolution);
        animationCurve.MoveKey(0, new Keyframe(0f, CalculateLineRendererWidth(vector)));
        lineRenderer.SetPosition(0, vector);
        for (int i = 1; i < resolution; i++)
        {
            float f = (float)i * num;
            float num2 = Mathf.Cos(f) * radius;
            float num3 = Mathf.Sin(f) * radius;
            Vector3 vector2 = center + axisU * num2 + axisV * num3;
            lineRenderer.SetPosition(i, vector2);
            animationCurve.MoveKey(i, new Keyframe((float)i / (float)(resolution - 1), CalculateLineRendererWidth(vector2)));
        }
        lineRenderer.widthCurve = animationCurve;
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }
        Camera camera = MainCamera.instance;
        if (camera == null)
        {
            return;
        }
        Color color = GUI.color;
        float num = camera.pixelWidth;
        float num2 = camera.pixelHeight;
        float time = Time.time;
        for (int num3 = labelsToRender.Count - 1; num3 >= 0; num3--)
        {
            LabelData labelData = labelsToRender[num3];
            Vector3 vector = camera.WorldToViewportPoint(labelData.position);
            if (vector.z > 0f)
            {
                Vector2 vector2 = new Vector2(vector.x * num, (1f - vector.y) * num2);
                Rect position = new Rect(vector2.x - 100f, vector2.y - 100f, 200f, 200f);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.color = Color.black;
                GUI.Label(position, labelData.content);
                position.position -= Vector2.one;
                GUI.color = labelData.color;
                GUI.Label(position, labelData.content);
            }
            if (time >= labelData.expireAfter)
            {
                labelsToRender.RemoveAtFast(num3);
            }
        }
        GUI.color = color;
    }

    private void OnEnable()
    {
        base.useGUILayout = false;
        lineRendererSharedMaterial = new Material(Shader.Find("Sprites/Default"));
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    private void OnDisable()
    {
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Remove(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    /// <summary>
    /// LateUpdate so that the most up-to-date gizmos and main camera position are used.
    /// </summary>
    private void LateUpdate()
    {
        if (!clUseLineRenderers.value)
        {
            return;
        }
        renderTime = Time.time;
        Camera camera = MainCamera.instance;
        if (camera != null)
        {
            mainCameraPosition = camera.transform.position;
            cullDistance = camera.farClipPlane;
            sqrCullDistance = cullDistance * cullDistance;
            if (lineRendererForegroundCamera == null)
            {
                GameObject gameObject = new GameObject("Runtime Gizmo Camera");
                lineRendererForegroundCamera = gameObject.AddComponent<Camera>();
                lineRendererForegroundCamera.clearFlags = CameraClearFlags.Depth;
                lineRendererForegroundCamera.cullingMask = 4;
                lineRendererForegroundCamera.depth = 10f;
                lineRendererForegroundCamera.nearClipPlane = camera.nearClipPlane;
                lineRendererForegroundCamera.farClipPlane = camera.farClipPlane;
            }
            lineRendererForegroundCamera.transform.SetPositionAndRotation(mainCameraPosition, camera.transform.rotation);
            lineRendererForegroundCamera.projectionMatrix = camera.projectionMatrix;
        }
        else
        {
            mainCameraPosition = Vector3.zero;
            cullDistance = 0f;
            sqrCullDistance = 0f;
        }
        foreach (LineRenderer activeLineRenderer in activeLineRenderers)
        {
            if (!(activeLineRenderer == null))
            {
                activeLineRenderer.enabled = false;
                lineRendererPool.Add(activeLineRenderer);
                AnimationCurve widthCurve = activeLineRenderer.widthCurve;
                if (animationCurvePool.TryGetValue(widthCurve.length, out var value))
                {
                    value.Add(widthCurve);
                }
            }
        }
        activeLineRenderers.Clear();
        for (int i = 0; i < 2; i++)
        {
            lineRendererLayer = lineRendererLayers[i];
            RenderBoxesUsingLineRenderers(boxLayers[i]);
            RenderLinesUsingLineRenderer(lineLayers[i]);
            RenderCapsulesUsingLineRenderers(capsuleLayers[i]);
            RenderSpheresUsingLineRenderers(sphereLayers[i]);
            RenderCirclesUsingLineRenderers(circleLayers[i]);
        }
    }

    private RuntimeGizmos()
    {
        boxLayers = new List<BoxData>[2];
        lineLayers = new List<LineData>[2];
        capsuleLayers = new List<CapsuleData>[2];
        sphereLayers = new List<SphereData>[2];
        circleLayers = new List<CircleData>[2];
        for (int i = 0; i < 2; i++)
        {
            boxLayers[i] = new List<BoxData>();
            lineLayers[i] = new List<LineData>();
            capsuleLayers[i] = new List<CapsuleData>();
            sphereLayers[i] = new List<SphereData>();
            circleLayers[i] = new List<CircleData>();
        }
        lineRendererLayers = new int[2];
        lineRendererLayers[0] = 18;
        lineRendererLayers[1] = 2;
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Runtime gizmos line renderer pool size: {lineRendererPool.Count}");
        results.Add($"Runtime gizmos animation curve pool size: {animationCurvePool.Count}");
        results.Add($"Runtime gizmos active line renderers: {activeLineRenderers.Count}");
        results.Add($"Runtime gizmos pending labels: {labelsToRender.Count}");
        int num = 0;
        List<LineData>[] array = lineLayers;
        foreach (List<LineData> list in array)
        {
            num += list.Count;
        }
        results.Add($"Runtime gizmos pending lines: {num}");
        int num2 = 0;
        List<SphereData>[] array2 = sphereLayers;
        foreach (List<SphereData> list2 in array2)
        {
            num2 += list2.Count;
        }
        results.Add($"Runtime gizmos pending spheres: {num2}");
        int num3 = 0;
        List<CircleData>[] array3 = circleLayers;
        foreach (List<CircleData> list3 in array3)
        {
            num3 += list3.Count;
        }
        results.Add($"Runtime gizmos pending circles: {num3}");
        int num4 = 0;
        List<CapsuleData>[] array4 = capsuleLayers;
        foreach (List<CapsuleData> list4 in array4)
        {
            num4 += list4.Count;
        }
        results.Add($"Runtime gizmos pending capsules: {num4}");
        int num5 = 0;
        List<BoxData>[] array5 = boxLayers;
        foreach (List<BoxData> list5 in array5)
        {
            num5 += list5.Count;
        }
        results.Add($"Runtime gizmos pending boxes: {num5}");
    }
}
