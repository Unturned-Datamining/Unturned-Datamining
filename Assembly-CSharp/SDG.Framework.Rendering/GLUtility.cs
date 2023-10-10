using System;
using UnityEngine;

namespace SDG.Framework.Rendering;

public class GLUtility
{
    protected static Material _LINE_FLAT_COLOR;

    protected static Material _LINE_CHECKERED_COLOR;

    protected static Material _LINE_DEPTH_CHECKERED_COLOR;

    protected static Material _LINE_CHECKERED_DEPTH_CUTOFF_COLOR;

    protected static Material _LINE_DEPTH_CUTOFF_COLOR;

    protected static Material _TRI_FLAT_COLOR;

    protected static Material _TRI_CHECKERED_COLOR;

    protected static Material _TRI_DEPTH_CHECKERED_COLOR;

    protected static Material _TRI_CHECKERED_DEPTH_CUTOFF_COLOR;

    protected static Material _TRI_DEPTH_CUTOFF_COLOR;

    public static Matrix4x4 matrix;

    public static Material LINE_FLAT_COLOR
    {
        get
        {
            if (_LINE_FLAT_COLOR == null)
            {
                _LINE_FLAT_COLOR = new Material(Shader.Find("GL/LineFlatColor"));
            }
            return _LINE_FLAT_COLOR;
        }
    }

    public static Material LINE_CHECKERED_COLOR
    {
        get
        {
            if (_LINE_CHECKERED_COLOR == null)
            {
                _LINE_CHECKERED_COLOR = new Material(Shader.Find("GL/LineCheckeredColor"));
            }
            return _LINE_CHECKERED_COLOR;
        }
    }

    public static Material LINE_DEPTH_CHECKERED_COLOR
    {
        get
        {
            if (_LINE_DEPTH_CHECKERED_COLOR == null)
            {
                _LINE_DEPTH_CHECKERED_COLOR = new Material(Shader.Find("GL/LineDepthCheckeredColor"));
            }
            return _LINE_DEPTH_CHECKERED_COLOR;
        }
    }

    public static Material LINE_CHECKERED_DEPTH_CUTOFF_COLOR
    {
        get
        {
            if (_LINE_CHECKERED_DEPTH_CUTOFF_COLOR == null)
            {
                _LINE_CHECKERED_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/LineCheckeredDepthCutoffColor"));
            }
            return _LINE_CHECKERED_DEPTH_CUTOFF_COLOR;
        }
    }

    public static Material LINE_DEPTH_CUTOFF_COLOR
    {
        get
        {
            if (_LINE_DEPTH_CUTOFF_COLOR == null)
            {
                _LINE_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/LineDepthCutoffColor"));
            }
            return _LINE_DEPTH_CUTOFF_COLOR;
        }
    }

    public static Material TRI_FLAT_COLOR
    {
        get
        {
            if (_TRI_FLAT_COLOR == null)
            {
                _TRI_FLAT_COLOR = new Material(Shader.Find("GL/TriFlatColor"));
            }
            return _TRI_FLAT_COLOR;
        }
    }

    public static Material TRI_CHECKERED_COLOR
    {
        get
        {
            if (_TRI_CHECKERED_COLOR == null)
            {
                _TRI_CHECKERED_COLOR = new Material(Shader.Find("GL/TriCheckeredColor"));
            }
            return _TRI_CHECKERED_COLOR;
        }
    }

    public static Material TRI_DEPTH_CHECKERED_COLOR
    {
        get
        {
            if (_TRI_DEPTH_CHECKERED_COLOR == null)
            {
                _TRI_DEPTH_CHECKERED_COLOR = new Material(Shader.Find("GL/TriDepthCheckeredColor"));
            }
            return _TRI_DEPTH_CHECKERED_COLOR;
        }
    }

    public static Material TRI_CHECKERED_DEPTH_CUTOFF_COLOR
    {
        get
        {
            if (_TRI_CHECKERED_DEPTH_CUTOFF_COLOR == null)
            {
                _TRI_CHECKERED_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/TriCheckeredDepthCutoffColor"));
            }
            return _TRI_CHECKERED_DEPTH_CUTOFF_COLOR;
        }
    }

    public static Material TRI_DEPTH_CUTOFF_COLOR
    {
        get
        {
            if (_TRI_DEPTH_CUTOFF_COLOR == null)
            {
                _TRI_DEPTH_CUTOFF_COLOR = new Material(Shader.Find("GL/TriDepthCutoffColor"));
            }
            return _TRI_DEPTH_CUTOFF_COLOR;
        }
    }

    public static void line(Vector3 begin, Vector3 end)
    {
        GL.Vertex(matrix.MultiplyPoint3x4(begin));
        GL.Vertex(matrix.MultiplyPoint3x4(end));
    }

    public static void boxSolid(Vector3 center, Vector3 size)
    {
        Vector3 vector = size / 2f;
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, 0f - vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(0f - vector.x, vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, 0f - vector.y, vector.z)));
        GL.Vertex(matrix.MultiplyPoint3x4(center + new Vector3(vector.x, vector.y, vector.z)));
    }

    public static void circle(Vector3 center, float radius, Vector3 horizontalAxis, Vector3 verticalAxis, float steps = 0f)
    {
        float num = MathF.PI * 2f;
        float num2 = 0f;
        if (steps == 0f)
        {
            steps = Mathf.Clamp(4f * radius, 8f, 128f);
        }
        float num3 = num / steps;
        Vector3 v = matrix.MultiplyPoint3x4(center + horizontalAxis * radius);
        while (num2 < num)
        {
            num2 += num3;
            float f = Mathf.Min(num2, num);
            float num4 = Mathf.Cos(f) * radius;
            float num5 = Mathf.Sin(f) * radius;
            Vector3 vector = matrix.MultiplyPoint3x4(center + horizontalAxis * num4 + verticalAxis * num5);
            GL.Vertex(v);
            GL.Vertex(vector);
            v = vector;
        }
    }

    public static void circle(Vector3 center, float radius, Vector3 horizontalAxis, Vector3 verticalAxis, GLCircleOffsetHandler handleGLCircleOffset)
    {
        if (handleGLCircleOffset != null)
        {
            float num = MathF.PI * 2f;
            float num2 = 0f;
            float num3 = num / Mathf.Clamp(4f * radius, 8f, 128f);
            Vector3 point = matrix.MultiplyPoint3x4(center + horizontalAxis * radius);
            handleGLCircleOffset(ref point);
            while (num2 < num)
            {
                num2 += num3;
                float f = Mathf.Min(num2, num);
                float num4 = Mathf.Cos(f) * radius;
                float num5 = Mathf.Sin(f) * radius;
                Vector3 point2 = matrix.MultiplyPoint3x4(center + horizontalAxis * num4 + verticalAxis * num5);
                handleGLCircleOffset(ref point2);
                GL.Vertex(point);
                GL.Vertex(point2);
                point = point2;
            }
        }
    }
}
