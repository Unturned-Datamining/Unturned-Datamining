using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class TransformHandlesV3
{
    public enum EMode
    {
        Position,
        Rotation,
        Scale,
        PositionBounds,
        ScaleBounds
    }

    public delegate void PreTransformEventHandler(Matrix4x4 worldToPivot);

    public delegate void TranslatedAndRotatedEventHandler(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation);

    public delegate void TransformedEventHandler(Matrix4x4 pivotToWorld);

    [Flags]
    private enum EComponent
    {
        NONE = 0,
        X = 1,
        Y = 2,
        Z = 4,
        POSITION_AXIS = 8,
        POSITION_PLANE = 0x10,
        ROTATION = 0x20,
        SCALE = 0x40,
        POSITION_BOUNDS = 0x80,
        NEGATIVE = 0x100,
        POSITIVE = 0x200,
        SCALE_BOUNDS = 0x400,
        POSITION_AXIS_X = 9,
        POSITION_AXIS_Y = 0xA,
        POSITION_AXIS_Z = 0xC,
        POSITION_PLANE_X = 0x11,
        POSITION_PLANE_Y = 0x12,
        POSITION_PLANE_Z = 0x14,
        ROTATION_X = 0x21,
        ROTATION_Y = 0x22,
        ROTATION_Z = 0x24,
        SCALE_AXIS_X = 0x41,
        SCALE_AXIS_Y = 0x42,
        SCALE_AXIS_Z = 0x44,
        SCALE_UNIFORM = 0x47,
        POSITION_BOUNDS_NEGATIVE_X = 0x181,
        POSITION_BOUNDS_POSITIVE_X = 0x281,
        POSITION_BOUNDS_NEGATIVE_Y = 0x182,
        POSITION_BOUNDS_POSITIVE_Y = 0x282,
        POSITION_BOUNDS_NEGATIVE_Z = 0x184,
        POSITION_BOUNDS_POSITIVE_Z = 0x284,
        SCALE_BOUNDS_NEGATIVE_X = 0x501,
        SCALE_BOUNDS_POSITIVE_X = 0x601,
        SCALE_BOUNDS_NEGATIVE_Y = 0x502,
        SCALE_BOUNDS_POSITIVE_Y = 0x602,
        SCALE_BOUNDS_NEGATIVE_Z = 0x504,
        SCALE_BOUNDS_POSITIVE_Z = 0x604
    }

    public bool wantsToSnap;

    public float snapPositionInterval = 1f;

    public float snapRotationIntervalDegrees = 15f;

    private static List<Component> workingComponentList = new List<Component>();

    private EMode preferredMode;

    private EMode mode;

    private Vector3 pivotPosition;

    private Quaternion pivotRotation = Quaternion.identity;

    private Vector3 preferredPivotPosition;

    private Quaternion preferredPivotRotation;

    private Bounds pivotBounds;

    private bool hasPivotBounds;

    private EComponent hoverComponent;

    private EComponent dragComponent;

    private Vector3 viewDirection;

    private Vector3 viewRight;

    private Vector3 viewUp;

    private Vector3 cameraForward;

    private float viewScale = 1f;

    private Vector3 viewAxisFlip = Vector3.one;

    private Vector3 dragPreviousPosition;

    private Quaternion dragPreviousRotation;

    private float dragPreviousAngle;

    private Vector3 dragPreviousScale;

    private Vector3 dragAxisOrigin;

    private Vector3 dragAxisDirection;

    private float dragAxisInitialDistance;

    private Vector3 dragPlaneOrigin;

    private Vector3 dragPlaneAxis0;

    private Vector3 dragPlaneAxis1;

    private Vector3 dragPlaneNormal;

    private float dragPlaneInitialDistance0;

    private float dragPlaneInitialDistance1;

    private Quaternion dragRotationOrigin;

    private Vector3 dragRotationAxis;

    private Vector3 dragRotationOutwardDirection;

    private Vector3 dragRotationEdgePoint;

    private Vector3 dragRotationTangent;

    private Vector3 dragScaleOrigin;

    private Vector3 dragScaleLocalDirection;

    private Vector3 dragScaleWorldDirection;

    private float dragScaleInitialDistance;

    private Vector3 dragScaleBoundsCenter;

    private Vector3 dragScaleBoundsSize;

    private float dragScaleBounds;

    public event PreTransformEventHandler OnPreTransform;

    public event TranslatedAndRotatedEventHandler OnTranslatedAndRotated;

    public event TransformedEventHandler OnTransformed;

    public Vector3 GetPivotPosition()
    {
        return pivotPosition;
    }

    public Quaternion GetPivotRotation()
    {
        return pivotRotation;
    }

    public void SetPreferredMode(EMode preferredMode)
    {
        this.preferredMode = preferredMode;
        SyncMode();
    }

    public void SetPreferredPivot(Vector3 position, Quaternion rotation)
    {
        preferredPivotPosition = position;
        preferredPivotRotation = rotation;
        SyncPivot();
    }

    public void ExternallyTransformPivot(Vector3 position, Quaternion rotation, bool modifyRotation)
    {
        if (dragComponent == EComponent.NONE)
        {
            Matrix4x4 inverse = Matrix4x4.TRS(pivotPosition, pivotRotation, Vector3.one).inverse;
            this.OnPreTransform?.Invoke(inverse);
            Vector3 worldPositionDelta = position - pivotPosition;
            Quaternion worldRotationDelta = rotation * Quaternion.Inverse(pivotRotation);
            this.OnTranslatedAndRotated?.Invoke(worldPositionDelta, worldRotationDelta, pivotPosition, modifyRotation);
            SetPreferredPivot(position, rotation);
        }
    }

    public bool Raycast(Ray mouseRay)
    {
        hoverComponent = EComponent.NONE;
        UpdateViewProperties();
        if (mode == EMode.Position)
        {
            if (RaycastPositionPlane(mouseRay, pivotRotation * Vector3.up * viewAxisFlip.y, pivotRotation * Vector3.forward * viewAxisFlip.z, pivotRotation * Vector3.right * viewAxisFlip.x))
            {
                hoverComponent = EComponent.POSITION_PLANE_X;
            }
            else if (RaycastPositionPlane(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.forward * viewAxisFlip.z, pivotRotation * Vector3.up * viewAxisFlip.y))
            {
                hoverComponent = EComponent.POSITION_PLANE_Y;
            }
            else if (RaycastPositionPlane(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.up * viewAxisFlip.y, pivotRotation * Vector3.forward * viewAxisFlip.z))
            {
                hoverComponent = EComponent.POSITION_PLANE_Z;
            }
            else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x))
            {
                hoverComponent = EComponent.POSITION_AXIS_X;
            }
            else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.up * viewAxisFlip.y))
            {
                hoverComponent = EComponent.POSITION_AXIS_Y;
            }
            else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.forward * viewAxisFlip.z))
            {
                hoverComponent = EComponent.POSITION_AXIS_Z;
            }
        }
        else if (mode == EMode.Rotation)
        {
            float num = -1f;
            float hitDistance;
            bool flag = RaycastRotationPlane(mouseRay, pivotRotation * Vector3.right, out hitDistance);
            if (flag)
            {
                num = hitDistance;
                hoverComponent = EComponent.ROTATION_X;
            }
            if (RaycastRotationPlane(mouseRay, pivotRotation * Vector3.up, out hitDistance) && (!flag || hitDistance < num))
            {
                flag = true;
                num = hitDistance;
                hoverComponent = EComponent.ROTATION_Y;
            }
            if (RaycastRotationPlane(mouseRay, pivotRotation * Vector3.forward, out hitDistance) && (!flag || hitDistance < num))
            {
                hoverComponent = EComponent.ROTATION_Z;
            }
        }
        else if (mode == EMode.Scale)
        {
            if (RaycastSphere(mouseRay))
            {
                hoverComponent = EComponent.SCALE_UNIFORM;
            }
            else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x))
            {
                hoverComponent = EComponent.SCALE_AXIS_X;
            }
            else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.up * viewAxisFlip.y))
            {
                hoverComponent = EComponent.SCALE_AXIS_Y;
            }
            else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.forward * viewAxisFlip.z))
            {
                hoverComponent = EComponent.SCALE_AXIS_Z;
            }
        }
        else if (mode == EMode.PositionBounds)
        {
            Vector3 min = pivotBounds.min;
            Vector3 max = pivotBounds.max;
            if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.right, 0f - min.x))
            {
                hoverComponent = EComponent.POSITION_BOUNDS_NEGATIVE_X;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.up, 0f - min.y))
            {
                hoverComponent = EComponent.POSITION_BOUNDS_NEGATIVE_Y;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.forward, 0f - min.z))
            {
                hoverComponent = EComponent.POSITION_BOUNDS_NEGATIVE_Z;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.right, max.x))
            {
                hoverComponent = EComponent.POSITION_BOUNDS_POSITIVE_X;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.up, max.y))
            {
                hoverComponent = EComponent.POSITION_BOUNDS_POSITIVE_Y;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.forward, max.z))
            {
                hoverComponent = EComponent.POSITION_BOUNDS_POSITIVE_Z;
            }
        }
        else if (mode == EMode.ScaleBounds)
        {
            Vector3 min2 = pivotBounds.min;
            Vector3 max2 = pivotBounds.max;
            if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.right, 0f - min2.x))
            {
                hoverComponent = EComponent.SCALE_BOUNDS_NEGATIVE_X;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.up, 0f - min2.y))
            {
                hoverComponent = EComponent.SCALE_BOUNDS_NEGATIVE_Y;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.forward, 0f - min2.z))
            {
                hoverComponent = EComponent.SCALE_BOUNDS_NEGATIVE_Z;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.right, max2.x))
            {
                hoverComponent = EComponent.SCALE_BOUNDS_POSITIVE_X;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.up, max2.y))
            {
                hoverComponent = EComponent.SCALE_BOUNDS_POSITIVE_Y;
            }
            else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.forward, max2.z))
            {
                hoverComponent = EComponent.SCALE_BOUNDS_POSITIVE_Z;
            }
        }
        return hoverComponent != EComponent.NONE;
    }

    public void MouseDown(Ray mouseRay)
    {
        dragComponent = hoverComponent;
        dragPreviousPosition = pivotPosition;
        dragPreviousRotation = pivotRotation;
        dragPreviousAngle = 0f;
        dragPreviousScale = Vector3.one;
        if (dragComponent.HasFlag(EComponent.POSITION_AXIS))
        {
            dragAxisOrigin = pivotPosition;
            if (dragComponent.HasFlag(EComponent.X))
            {
                dragAxisDirection = pivotRotation * Vector3.right * viewAxisFlip.x;
            }
            else if (dragComponent.HasFlag(EComponent.Y))
            {
                dragAxisDirection = pivotRotation * Vector3.up * viewAxisFlip.y;
            }
            else
            {
                dragAxisDirection = pivotRotation * Vector3.forward * viewAxisFlip.z;
            }
            dragAxisInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection);
        }
        else if (dragComponent.HasFlag(EComponent.POSITION_PLANE))
        {
            dragPlaneOrigin = pivotPosition;
            if (dragComponent.HasFlag(EComponent.X))
            {
                dragPlaneAxis0 = pivotRotation * Vector3.up * viewAxisFlip.y;
                dragPlaneAxis1 = pivotRotation * Vector3.forward * viewAxisFlip.z;
                dragPlaneNormal = pivotRotation * Vector3.right * viewAxisFlip.x;
            }
            else if (dragComponent.HasFlag(EComponent.Y))
            {
                dragPlaneAxis0 = pivotRotation * Vector3.right * viewAxisFlip.x;
                dragPlaneAxis1 = pivotRotation * Vector3.forward * viewAxisFlip.z;
                dragPlaneNormal = pivotRotation * Vector3.up * viewAxisFlip.y;
            }
            else
            {
                dragPlaneAxis0 = pivotRotation * Vector3.right * viewAxisFlip.x;
                dragPlaneAxis1 = pivotRotation * Vector3.up * viewAxisFlip.y;
                dragPlaneNormal = pivotRotation * Vector3.forward * viewAxisFlip.z;
            }
            if (new Plane(dragPlaneNormal, dragPlaneOrigin).Raycast(mouseRay, out var enter))
            {
                Vector3 rhs = mouseRay.origin + mouseRay.direction * enter - dragPlaneOrigin;
                dragPlaneInitialDistance0 = Vector3.Dot(dragPlaneAxis0, rhs);
                dragPlaneInitialDistance1 = Vector3.Dot(dragPlaneAxis1, rhs);
            }
        }
        else if (dragComponent.HasFlag(EComponent.ROTATION))
        {
            dragRotationOrigin = pivotRotation;
            if (dragComponent.HasFlag(EComponent.X))
            {
                dragRotationAxis = pivotRotation * Vector3.right * viewAxisFlip.x;
            }
            else if (dragComponent.HasFlag(EComponent.Y))
            {
                dragRotationAxis = pivotRotation * Vector3.up * viewAxisFlip.y;
            }
            else
            {
                dragRotationAxis = pivotRotation * Vector3.forward * viewAxisFlip.z;
            }
            if (new Plane(dragRotationAxis, pivotPosition).Raycast(mouseRay, out var enter2))
            {
                Vector3 vector = mouseRay.origin + mouseRay.direction * enter2;
                dragRotationOutwardDirection = (vector - pivotPosition).normalized;
                dragRotationEdgePoint = vector;
                dragRotationTangent = Vector3.Cross(dragRotationAxis, dragRotationOutwardDirection).normalized;
            }
        }
        else if (dragComponent.HasFlag(EComponent.SCALE))
        {
            dragScaleOrigin = pivotPosition;
            if (dragComponent == EComponent.SCALE_UNIFORM)
            {
                if (new Plane(-cameraForward, dragScaleOrigin).Raycast(mouseRay, out var enter3))
                {
                    Vector3 vector2 = mouseRay.origin + mouseRay.direction * enter3;
                    dragScaleLocalDirection = Vector3.one;
                    dragScaleWorldDirection = (vector2 - dragScaleOrigin).normalized;
                }
            }
            else if (dragComponent.HasFlag(EComponent.X))
            {
                dragScaleLocalDirection = Vector3.right;
                dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection * viewAxisFlip.x;
            }
            else if (dragComponent.HasFlag(EComponent.Y))
            {
                dragScaleLocalDirection = Vector3.up;
                dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection * viewAxisFlip.y;
            }
            else
            {
                dragScaleLocalDirection = Vector3.forward;
                dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection * viewAxisFlip.z;
            }
            dragScaleInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection);
        }
        else if (dragComponent.HasFlag(EComponent.POSITION_BOUNDS))
        {
            dragAxisOrigin = pivotPosition;
            if (dragComponent.HasFlag(EComponent.X))
            {
                dragAxisDirection = pivotRotation * Vector3.right;
            }
            else if (dragComponent.HasFlag(EComponent.Y))
            {
                dragAxisDirection = pivotRotation * Vector3.up;
            }
            else
            {
                dragAxisDirection = pivotRotation * Vector3.forward;
            }
            if (dragComponent.HasFlag(EComponent.NEGATIVE))
            {
                dragAxisDirection *= -1f;
            }
            dragAxisInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection);
        }
        else if (dragComponent.HasFlag(EComponent.SCALE_BOUNDS))
        {
            dragScaleOrigin = pivotPosition;
            if (dragComponent.HasFlag(EComponent.X))
            {
                dragScaleLocalDirection = Vector3.right;
                dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection;
                dragScaleBounds = pivotBounds.size.x;
            }
            else if (dragComponent.HasFlag(EComponent.Y))
            {
                dragScaleLocalDirection = Vector3.up;
                dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection;
                dragScaleBounds = pivotBounds.size.y;
            }
            else
            {
                dragScaleLocalDirection = Vector3.forward;
                dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection;
                dragScaleBounds = pivotBounds.size.z;
            }
            dragScaleBoundsCenter = pivotPosition + pivotRotation * pivotBounds.center;
            dragPreviousPosition = dragScaleBoundsCenter;
            dragScaleBoundsSize = pivotBounds.size;
            if (dragComponent.HasFlag(EComponent.NEGATIVE))
            {
                dragScaleWorldDirection *= -1f;
            }
            dragScaleInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection);
            dragAxisOrigin = dragScaleOrigin;
            dragAxisDirection = dragScaleWorldDirection;
            dragAxisInitialDistance = dragScaleInitialDistance;
        }
        Matrix4x4 inverse = Matrix4x4.TRS(dragPreviousPosition, dragPreviousRotation, dragPreviousScale).inverse;
        this.OnPreTransform?.Invoke(inverse);
    }

    public void MouseMove(Ray mouseRay)
    {
        if (dragComponent.HasFlag(EComponent.POSITION_AXIS) || dragComponent.HasFlag(EComponent.POSITION_BOUNDS))
        {
            float num = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection) - dragAxisInitialDistance;
            if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
            {
                num = (float)Mathf.RoundToInt(num / snapPositionInterval) * snapPositionInterval;
            }
            Vector3 vector = dragAxisDirection * num;
            Vector3 vector2 = dragAxisOrigin + vector;
            if ((vector2 - dragPreviousPosition).magnitude > Mathf.Epsilon)
            {
                pivotPosition = vector2;
                this.OnTranslatedAndRotated?.Invoke(vector, Quaternion.identity, pivotPosition, modifyRotation: false);
                dragPreviousPosition = vector2;
            }
        }
        else if (dragComponent.HasFlag(EComponent.POSITION_PLANE))
        {
            if (new Plane(dragPlaneNormal, dragPlaneOrigin).Raycast(mouseRay, out var enter))
            {
                Vector3 rhs = mouseRay.origin + mouseRay.direction * enter - dragPlaneOrigin;
                float num2 = Vector3.Dot(dragPlaneAxis0, rhs) - dragPlaneInitialDistance0;
                float num3 = Vector3.Dot(dragPlaneAxis1, rhs) - dragPlaneInitialDistance1;
                if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
                {
                    num2 = (float)Mathf.RoundToInt(num2 / snapPositionInterval) * snapPositionInterval;
                    num3 = (float)Mathf.RoundToInt(num3 / snapPositionInterval) * snapPositionInterval;
                }
                Vector3 vector3 = dragPlaneAxis0 * num2 + dragPlaneAxis1 * num3;
                Vector3 vector4 = dragPlaneOrigin + vector3;
                if ((vector4 - dragPreviousPosition).magnitude > Mathf.Epsilon)
                {
                    pivotPosition = vector4;
                    this.OnTranslatedAndRotated?.Invoke(vector3, Quaternion.identity, pivotPosition, modifyRotation: false);
                    dragPreviousPosition = vector4;
                }
            }
        }
        else if (dragComponent.HasFlag(EComponent.ROTATION))
        {
            float num4 = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragRotationEdgePoint, dragRotationTangent) * 90f / viewScale;
            if (wantsToSnap && snapRotationIntervalDegrees > Mathf.Epsilon)
            {
                num4 = (float)Mathf.RoundToInt(num4 / snapRotationIntervalDegrees) * snapRotationIntervalDegrees;
            }
            if (Mathf.Abs(num4 - dragPreviousAngle) > Mathf.Epsilon)
            {
                Quaternion quaternion = Quaternion.AngleAxis(num4, dragRotationAxis);
                Quaternion quaternion2 = (pivotRotation = quaternion * dragRotationOrigin);
                this.OnTranslatedAndRotated?.Invoke(Vector3.zero, quaternion, pivotPosition, modifyRotation: true);
                dragPreviousAngle = num4;
                dragPreviousRotation = quaternion2;
            }
        }
        else if (dragComponent.HasFlag(EComponent.SCALE))
        {
            float num5 = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection) - dragScaleInitialDistance;
            num5 /= viewScale;
            if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
            {
                num5 = (float)Mathf.RoundToInt(num5 / snapPositionInterval) * snapPositionInterval;
            }
            if (!MathfEx.IsNearlyEqual(num5, -1f, Mathf.Epsilon))
            {
                Vector3 vector5 = Vector3.one + dragScaleLocalDirection * num5;
                if ((vector5 - dragPreviousScale).magnitude > Mathf.Epsilon)
                {
                    Matrix4x4 pivotToWorld = Matrix4x4.TRS(dragPreviousPosition, dragPreviousRotation, vector5);
                    this.OnTransformed?.Invoke(pivotToWorld);
                    dragPreviousScale = vector5;
                }
            }
        }
        else if (dragComponent.HasFlag(EComponent.SCALE_BOUNDS))
        {
            float num6 = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection) - dragScaleInitialDistance;
            if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
            {
                num6 = (float)Mathf.RoundToInt(num6 / snapPositionInterval) * snapPositionInterval;
            }
            Vector3 vector6 = dragScaleBoundsCenter + dragScaleWorldDirection * num6 * 0.5f;
            Vector3 vector7 = Vector3.one + dragScaleLocalDirection * (num6 / dragScaleBounds);
            if ((vector6 - dragPreviousPosition).magnitude > Mathf.Epsilon && (vector7 - dragPreviousScale).magnitude > Mathf.Epsilon)
            {
                pivotPosition = dragScaleOrigin + dragScaleWorldDirection * num6 * 0.5f;
                pivotBounds.size = dragScaleBoundsSize + dragScaleLocalDirection * num6;
                Matrix4x4 pivotToWorld2 = Matrix4x4.TRS(vector6, dragPreviousRotation, vector7);
                this.OnTransformed?.Invoke(pivotToWorld2);
                dragPreviousPosition = vector6;
                dragPreviousScale = vector7;
            }
        }
    }

    public void MouseUp()
    {
        dragComponent = EComponent.NONE;
        wantsToSnap = false;
        SyncMode();
        SyncPivot();
    }

    public void Render(Ray mouseRay)
    {
        UpdateViewProperties();
        if ((mode == EMode.PositionBounds || mode == EMode.ScaleBounds) && hasPivotBounds)
        {
            Color yellow = Color.yellow;
            yellow.a = 0.25f;
            RuntimeGizmos.Get().Box(Matrix4x4.TRS(pivotPosition, pivotRotation, Vector3.one), pivotBounds.center, pivotBounds.size, yellow, 0f, EGizmoLayer.Foreground);
        }
        if (mode == EMode.Position)
        {
            if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
            {
                if (dragComponent.HasFlag(EComponent.POSITION_AXIS))
                {
                    DrawPositionAxisSnap(mouseRay);
                }
                else if (dragComponent.HasFlag(EComponent.POSITION_PLANE))
                {
                    DrawPositionPlaneSnap(mouseRay);
                }
            }
            DrawPositionPlane(pivotRotation * Vector3.up * viewAxisFlip.y, pivotRotation * Vector3.forward * viewAxisFlip.z, Color.red, EComponent.POSITION_PLANE_X);
            DrawPositionPlane(pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.forward * viewAxisFlip.z, Color.green, EComponent.POSITION_PLANE_Y);
            DrawPositionPlane(pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.up * viewAxisFlip.y, Color.blue, EComponent.POSITION_PLANE_Z);
            DrawPositionAxis(pivotRotation * Vector3.right * viewAxisFlip.x, Color.red, EComponent.POSITION_AXIS_X);
            DrawPositionAxis(pivotRotation * Vector3.up * viewAxisFlip.y, Color.green, EComponent.POSITION_AXIS_Y);
            DrawPositionAxis(pivotRotation * Vector3.forward * viewAxisFlip.z, Color.blue, EComponent.POSITION_AXIS_Z);
        }
        else if (mode == EMode.Rotation)
        {
            if (dragComponent == EComponent.NONE)
            {
                DrawRotationCircle(pivotRotation * Vector3.up, pivotRotation * Vector3.forward, EComponent.ROTATION_X, Color.red);
                DrawRotationCircle(pivotRotation * Vector3.right, pivotRotation * Vector3.forward, EComponent.ROTATION_Y, Color.green);
                DrawRotationCircle(pivotRotation * Vector3.right, pivotRotation * Vector3.up, EComponent.ROTATION_Z, Color.blue);
            }
            else
            {
                DrawDragCircle();
            }
        }
        else if (mode == EMode.Scale)
        {
            Color color = ((dragComponent == EComponent.SCALE_UNIFORM) ? Color.white : ((hoverComponent == EComponent.SCALE_UNIFORM) ? Color.yellow : Color.gray));
            RuntimeGizmos.Get().Circle(pivotPosition, viewRight, viewUp, 0.25f * viewScale, color, 0f, 16, EGizmoLayer.Foreground);
            DrawScaleAxis(pivotRotation * Vector3.right * viewAxisFlip.x, Color.red, EComponent.SCALE_AXIS_X);
            DrawScaleAxis(pivotRotation * Vector3.up * viewAxisFlip.y, Color.green, EComponent.SCALE_AXIS_Y);
            DrawScaleAxis(pivotRotation * Vector3.forward * viewAxisFlip.z, Color.blue, EComponent.SCALE_AXIS_Z);
        }
        else if (mode == EMode.PositionBounds)
        {
            if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
            {
                DrawPositionAxisSnap(mouseRay);
            }
            Vector3 min = pivotBounds.min;
            Vector3 max = pivotBounds.max;
            DrawPositionBoundsAxis(pivotRotation * -Vector3.right, 0f - min.x, Color.red, EComponent.POSITION_BOUNDS_NEGATIVE_X);
            DrawPositionBoundsAxis(pivotRotation * -Vector3.up, 0f - min.y, Color.green, EComponent.POSITION_BOUNDS_NEGATIVE_Y);
            DrawPositionBoundsAxis(pivotRotation * -Vector3.forward, 0f - min.z, Color.blue, EComponent.POSITION_BOUNDS_NEGATIVE_Z);
            DrawPositionBoundsAxis(pivotRotation * Vector3.right, max.x, Color.red, EComponent.POSITION_BOUNDS_POSITIVE_X);
            DrawPositionBoundsAxis(pivotRotation * Vector3.up, max.y, Color.green, EComponent.POSITION_BOUNDS_POSITIVE_Y);
            DrawPositionBoundsAxis(pivotRotation * Vector3.forward, max.z, Color.blue, EComponent.POSITION_BOUNDS_POSITIVE_Z);
        }
        else if (mode == EMode.ScaleBounds)
        {
            if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
            {
                DrawPositionAxisSnap(mouseRay);
            }
            Vector3 min2 = pivotBounds.min;
            Vector3 max2 = pivotBounds.max;
            DrawScaleBoundsAxis(pivotRotation * -Vector3.right, 0f - min2.x, Color.red, EComponent.SCALE_BOUNDS_NEGATIVE_X);
            DrawScaleBoundsAxis(pivotRotation * -Vector3.up, 0f - min2.y, Color.green, EComponent.SCALE_BOUNDS_NEGATIVE_Y);
            DrawScaleBoundsAxis(pivotRotation * -Vector3.forward, 0f - min2.z, Color.blue, EComponent.SCALE_BOUNDS_NEGATIVE_Z);
            DrawScaleBoundsAxis(pivotRotation * Vector3.right, max2.x, Color.red, EComponent.SCALE_BOUNDS_POSITIVE_X);
            DrawScaleBoundsAxis(pivotRotation * Vector3.up, max2.y, Color.green, EComponent.SCALE_BOUNDS_POSITIVE_Y);
            DrawScaleBoundsAxis(pivotRotation * Vector3.forward, max2.z, Color.blue, EComponent.SCALE_BOUNDS_POSITIVE_Z);
        }
    }

    public void UpdateBoundsFromSelection(IEnumerable<GameObject> selection)
    {
        if (dragComponent != 0)
        {
            return;
        }
        pivotBounds = default(Bounds);
        hasPivotBounds = false;
        Matrix4x4 worldToPivot = Matrix4x4.TRS(pivotPosition, pivotRotation, Vector3.one).inverse;
        foreach (GameObject item in selection)
        {
            item.GetComponentsInChildren(workingComponentList);
            foreach (Component workingComponent in workingComponentList)
            {
                if (workingComponent is MeshFilter meshFilter)
                {
                    if (meshFilter.sharedMesh != null)
                    {
                        Bounds bounds = meshFilter.sharedMesh.bounds;
                        EncapsuleBounds(workingComponent.transform, bounds.center, bounds.extents);
                        hasPivotBounds = true;
                    }
                }
                else if (workingComponent is BoxCollider boxCollider)
                {
                    EncapsuleBounds(workingComponent.transform, boxCollider.center, boxCollider.size * 0.5f);
                    hasPivotBounds = true;
                }
                else if (workingComponent is SphereCollider sphereCollider)
                {
                    float radius = sphereCollider.radius;
                    Vector3 extents2 = new Vector3(radius, radius, radius);
                    EncapsuleBounds(workingComponent.transform, sphereCollider.center, extents2);
                    hasPivotBounds = true;
                }
            }
            workingComponentList.Clear();
        }
        SyncMode();
        void EncapsuleBounds(Transform transform, Vector3 center, Vector3 extents)
        {
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z))));
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(0f - extents.x, 0f - extents.y, extents.z))));
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(0f - extents.x, extents.y, 0f - extents.z))));
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(0f - extents.x, extents.y, extents.z))));
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z))));
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, 0f - extents.y, extents.z))));
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, extents.y, 0f - extents.z))));
            pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, extents.y, extents.z))));
        }
    }

    private bool RaycastPositionAxis(Ray mouseRay, Vector3 axisDirection)
    {
        float num = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, pivotPosition, axisDirection);
        float num2 = MathfEx.DistanceBetweenRays(mouseRay.origin, mouseRay.direction, pivotPosition, axisDirection);
        if (num > 0f && num < viewScale)
        {
            return num2 < viewScale * 0.1f;
        }
        return false;
    }

    private bool RaycastPositionPlane(Ray mouseRay, Vector3 axis0, Vector3 axis1, Vector3 planeNormal)
    {
        if (!new Plane(planeNormal, pivotPosition).Raycast(mouseRay, out var enter))
        {
            return false;
        }
        Vector3 rhs = mouseRay.origin + mouseRay.direction * enter - pivotPosition;
        float num = Vector3.Dot(axis0, rhs);
        float num2 = Vector3.Dot(axis1, rhs);
        if (num > 0f && num < viewScale * 0.25f && num2 > 0f)
        {
            return num2 < viewScale * 0.25f;
        }
        return false;
    }

    private bool RaycastRotationPlane(Ray mouseRay, Vector3 planeNormal, out float hitDistance)
    {
        if (!new Plane(planeNormal, pivotPosition).Raycast(mouseRay, out hitDistance))
        {
            return false;
        }
        Vector3 vector = mouseRay.origin + mouseRay.direction * hitDistance - pivotPosition;
        float num = MathfEx.Square(viewScale * 0.9f);
        float num2 = MathfEx.Square(viewScale * 1.1f);
        float sqrMagnitude = vector.sqrMagnitude;
        if (sqrMagnitude > num)
        {
            return sqrMagnitude < num2;
        }
        return false;
    }

    private bool RaycastPositionBoundsAxis(Ray mouseRay, Vector3 axisDirection, float offset)
    {
        Vector3 origin = pivotPosition + axisDirection * offset;
        float num = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, origin, axisDirection);
        float num2 = MathfEx.DistanceBetweenRays(mouseRay.origin, mouseRay.direction, origin, axisDirection);
        if (num > 0f && num < viewScale)
        {
            return num2 < viewScale * 0.1f;
        }
        return false;
    }

    private bool RaycastSphere(Ray mouseRay)
    {
        Vector3 vector = pivotPosition;
        float num = viewScale * 0.25f;
        float num2 = num * num;
        float num3 = Vector3.Dot(vector - mouseRay.origin, mouseRay.direction);
        if (num3 < 0f)
        {
            return false;
        }
        Vector3 vector2 = mouseRay.origin + mouseRay.direction * num3;
        return (vector - vector2).sqrMagnitude <= num2;
    }

    private void DrawPositionAxisSnap(Ray mouseRay)
    {
        Color color = new Color(0f, 0f, 0f, 0.5f);
        Vector3 vector = Vector3.Cross(dragAxisDirection, viewDirection).normalized * 0.1f * viewScale;
        float num = (float)Mathf.RoundToInt((MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection) - dragAxisInitialDistance) / snapPositionInterval) * snapPositionInterval + dragAxisInitialDistance;
        Vector3 vector2 = dragAxisOrigin + dragAxisDirection * num;
        for (int i = -10; i <= 10; i++)
        {
            Vector3 vector3 = vector2 + dragAxisDirection * snapPositionInterval * i;
            RuntimeGizmos.Get().Line(vector3 - vector, vector3 + vector, color, 0f, EGizmoLayer.Foreground);
        }
    }

    private void DrawPositionPlaneSnap(Ray mouseRay)
    {
        if (new Plane(dragPlaneNormal, dragPlaneOrigin).Raycast(mouseRay, out var enter))
        {
            Vector3 rhs = mouseRay.origin + mouseRay.direction * enter - dragPlaneOrigin;
            float num = (float)Mathf.RoundToInt((Vector3.Dot(dragPlaneAxis0, rhs) - dragPlaneInitialDistance0) / snapPositionInterval) * snapPositionInterval + dragPlaneInitialDistance0;
            float num2 = (float)Mathf.RoundToInt((Vector3.Dot(dragPlaneAxis1, rhs) - dragPlaneInitialDistance1) / snapPositionInterval) * snapPositionInterval + dragPlaneInitialDistance1;
            Vector3 vector = dragPlaneOrigin + dragPlaneAxis0 * num + dragPlaneAxis1 * num2;
            Color color = new Color(0f, 0f, 0f, 0.5f);
            Vector3 vector2 = dragPlaneAxis0 * snapPositionInterval * 10f;
            Vector3 vector3 = dragPlaneAxis1 * snapPositionInterval * 10f;
            for (int i = -10; i <= 10; i++)
            {
                Vector3 vector4 = vector + dragPlaneAxis0 * snapPositionInterval * i;
                RuntimeGizmos.Get().Line(vector4 - vector3, vector4 + vector3, color, 0f, EGizmoLayer.Foreground);
            }
            for (int j = -10; j <= 10; j++)
            {
                Vector3 vector5 = vector + dragPlaneAxis1 * snapPositionInterval * j;
                RuntimeGizmos.Get().Line(vector5 - vector2, vector5 + vector2, color, 0f, EGizmoLayer.Foreground);
            }
        }
    }

    private void DrawPositionAxis(Vector3 direction, Color color, EComponent component)
    {
        Color color2 = ((dragComponent == component) ? Color.white : ((hoverComponent == component) ? Color.yellow : color));
        RuntimeGizmos.Get().Arrow(pivotPosition, direction, viewScale, color2, 0f, EGizmoLayer.Foreground);
    }

    private void DrawPositionPlane(Vector3 axis0, Vector3 axis1, Color color, EComponent component)
    {
        Color color2 = ((dragComponent == component) ? Color.white : ((hoverComponent == component) ? Color.yellow : color));
        Vector3 vector = axis0 * 0.25f * viewScale;
        Vector3 vector2 = axis1 * 0.25f * viewScale;
        Vector3 end = pivotPosition + vector + vector2;
        RuntimeGizmos.Get().Line(pivotPosition + vector, end, color2, 0f, EGizmoLayer.Foreground);
        RuntimeGizmos.Get().Line(pivotPosition + vector2, end, color2, 0f, EGizmoLayer.Foreground);
    }

    private void DrawRotationCircle(Vector3 axis0, Vector3 axis1, EComponent component, Color color)
    {
        Color color2 = ((hoverComponent == component) ? Color.yellow : color);
        RuntimeGizmos.Get().Circle(pivotPosition, axis0, axis1, viewScale, color2, 0f, 32, EGizmoLayer.Foreground);
    }

    private void DrawDragCircle()
    {
        if (wantsToSnap)
        {
            Color color = new Color(0f, 0f, 0f, 0.5f);
            float num = (float)Math.PI / 180f * dragPreviousAngle;
            float num2 = (float)Math.PI / 180f * snapRotationIntervalDegrees;
            int num3 = Mathf.Max(1, Mathf.CeilToInt((float)Math.PI / 2f / num2));
            for (int i = -num3; i <= num3; i++)
            {
                if (i != 0)
                {
                    float f = num + (float)i * num2;
                    float num4 = Mathf.Cos(f);
                    float num5 = Mathf.Sin(f);
                    Vector3 vector = dragRotationOutwardDirection * num4 + dragRotationTangent * num5;
                    Vector3 begin = pivotPosition + vector * viewScale * 0.9f;
                    Vector3 end = pivotPosition + vector * viewScale * 1.1f;
                    RuntimeGizmos.Get().Line(begin, end, color, 0f, EGizmoLayer.Foreground);
                }
            }
        }
        Color white = Color.white;
        RuntimeGizmos.Get().Circle(pivotPosition, dragRotationOutwardDirection, dragRotationTangent, viewScale, white, 0f, 32, EGizmoLayer.Foreground);
        float f2 = (float)Math.PI / 180f * dragPreviousAngle;
        float num6 = Mathf.Cos(f2);
        float num7 = Mathf.Sin(f2);
        Vector3 vector2 = dragRotationOutwardDirection * num6 + dragRotationTangent * num7;
        RuntimeGizmos.Get().Line(pivotPosition, pivotPosition + vector2 * viewScale * 1.1f, white, 0f, EGizmoLayer.Foreground);
        white.a = 0.5f;
        Vector3 vector3 = pivotPosition + dragRotationOutwardDirection * viewScale;
        Vector3 vector4 = dragRotationTangent * 0.5f * viewScale;
        RuntimeGizmos.Get().Line(pivotPosition, vector3, white, 0f, EGizmoLayer.Foreground);
        RuntimeGizmos.Get().Line(vector3, vector3 - vector4, white, 0f, EGizmoLayer.Foreground);
        RuntimeGizmos.Get().Line(vector3, vector3 + vector4, white, 0f, EGizmoLayer.Foreground);
    }

    private void DrawScaleAxis(Vector3 direction, Color color, EComponent component)
    {
        Vector3 vector = Vector3.Cross(direction, viewDirection).normalized * 0.1f * viewScale;
        Color color2 = ((dragComponent == component) ? Color.white : ((hoverComponent == component) ? Color.yellow : color));
        Vector3 vector2 = pivotPosition + direction * viewScale;
        RuntimeGizmos.Get().Line(pivotPosition, vector2, color2, 0f, EGizmoLayer.Foreground);
        RuntimeGizmos.Get().Line(vector2 - vector, vector2 + vector, color2, 0f, EGizmoLayer.Foreground);
    }

    private void DrawPositionBoundsAxis(Vector3 direction, float offset, Color color, EComponent component)
    {
        Vector3 vector = Vector3.Cross(direction, viewDirection).normalized * 0.1f * viewScale;
        Color color2 = ((dragComponent == component) ? Color.white : ((hoverComponent == component) ? Color.yellow : color));
        Vector3 vector2 = pivotPosition + direction * offset;
        Vector3 end = vector2 + direction * viewScale;
        Vector3 vector3 = vector2 + direction * 0.75f * viewScale;
        RuntimeGizmos.Get().Line(vector2, end, color2, 0f, EGizmoLayer.Foreground);
        RuntimeGizmos.Get().Line(vector3 - vector, end, color2, 0f, EGizmoLayer.Foreground);
        RuntimeGizmos.Get().Line(vector3 + vector, end, color2, 0f, EGizmoLayer.Foreground);
    }

    private void DrawScaleBoundsAxis(Vector3 direction, float offset, Color color, EComponent component)
    {
        Vector3 vector = Vector3.Cross(direction, viewDirection).normalized * 0.1f * viewScale;
        Color color2 = ((dragComponent == component) ? Color.white : ((hoverComponent == component) ? Color.yellow : color));
        Vector3 vector2 = pivotPosition + direction * offset;
        Vector3 vector3 = vector2 + direction * viewScale;
        RuntimeGizmos.Get().Line(vector2, vector3, color2, 0f, EGizmoLayer.Foreground);
        RuntimeGizmos.Get().Line(vector3 - vector, vector3 + vector, color2, 0f, EGizmoLayer.Foreground);
    }

    private void SyncMode()
    {
        if (dragComponent == EComponent.NONE)
        {
            if (preferredMode == EMode.PositionBounds && !hasPivotBounds)
            {
                mode = EMode.Position;
            }
            else if (preferredMode == EMode.ScaleBounds && !hasPivotBounds)
            {
                mode = EMode.Scale;
            }
            else
            {
                mode = preferredMode;
            }
        }
    }

    private void SyncPivot()
    {
        if (dragComponent == EComponent.NONE)
        {
            pivotPosition = preferredPivotPosition;
            pivotRotation = preferredPivotRotation;
        }
    }

    private void UpdateViewProperties()
    {
        if (MainCamera.instance == null)
        {
            viewDirection = Vector3.forward;
            viewScale = 1f;
            viewAxisFlip = Vector3.one;
            cameraForward = Vector3.forward;
            return;
        }
        cameraForward = MainCamera.instance.transform.forward;
        Vector3 position = MainCamera.instance.transform.position;
        Vector3 vector = pivotPosition - position;
        float magnitude = vector.magnitude;
        if (magnitude < 0.001f)
        {
            viewDirection = Vector3.forward;
            viewScale = 1f;
            viewAxisFlip = Vector3.one;
            return;
        }
        viewDirection = vector / magnitude;
        viewRight = Vector3.Cross(viewDirection, Vector3.up).normalized;
        viewUp = Vector3.Cross(viewDirection, viewRight).normalized;
        viewScale = magnitude * 0.5f;
        viewAxisFlip.x = ((Vector3.Dot(viewDirection, pivotRotation * Vector3.right) < 0f) ? 1 : (-1));
        viewAxisFlip.y = ((Vector3.Dot(viewDirection, pivotRotation * Vector3.up) < 0f) ? 1 : (-1));
        viewAxisFlip.z = ((Vector3.Dot(viewDirection, pivotRotation * Vector3.forward) < 0f) ? 1 : (-1));
    }
}
