using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerWorkzone : PlayerCaller
{
    public DragStarted onDragStarted;

    public DragStopped onDragStopped;

    public float snapTransform;

    public float snapRotation;

    private bool _isBuilding;

    private Ray ray;

    private RaycastHit worldHit;

    private RaycastHit buildableHit;

    private Vector2 dragStartViewportPoint;

    private Vector2 dragStartScreenPoint;

    private Vector2 dragEndViewportPoint;

    private Vector2 dragEndScreenPoint;

    private bool hasDragStart;

    private bool isDragging;

    private bool isUsingHandle;

    private List<WorkzoneSelection> selection;

    private Vector3 copyPosition;

    private Quaternion copyRotation;

    private bool hasCopiedRotation;

    private TransformHandles handles;

    private EDragMode _dragMode;

    private bool wantsBoundsEditor;

    private EDragCoordinate _dragCoordinate;

    private List<EditorDrag> dragable;

    public bool isBuilding
    {
        get
        {
            return _isBuilding;
        }
        set
        {
            _isBuilding = value;
            if (!_isBuilding)
            {
                clearSelection();
            }
            base.player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Workzone, _isBuilding);
        }
    }

    public EDragMode dragMode
    {
        get
        {
            return _dragMode;
        }
        set
        {
            _dragMode = value;
            wantsBoundsEditor = false;
            calculateHandleOffsets();
        }
    }

    public EDragCoordinate dragCoordinate
    {
        get
        {
            return _dragCoordinate;
        }
        set
        {
            _dragCoordinate = value;
            calculateHandleOffsets();
        }
    }

    public void applySelection()
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (!(selection[i].transform == null))
            {
                Vector3 position = selection[i].transform.position;
                selection[i].transform.position = selection[i].fromPosition;
                Vector3 eulerAngles = selection[i].transform.rotation.eulerAngles;
                if (selection[i].transform.CompareTag("Barricade"))
                {
                    BarricadeManager.transformBarricade(selection[i].transform, position, eulerAngles.x, eulerAngles.y, eulerAngles.z);
                }
                else if (selection[i].transform.CompareTag("Structure"))
                {
                    StructureManager.transformStructure(selection[i].transform, position, eulerAngles.x, eulerAngles.y, eulerAngles.z);
                }
                selection[i].transform.position = position;
            }
        }
    }

    public void pointSelection()
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (!(selection[i].transform == null))
            {
                selection[i].fromPosition = selection[i].transform.position;
            }
        }
    }

    public void addSelection(Transform select)
    {
        HighlighterTool.highlight(select, Color.yellow);
        selection.Add(new WorkzoneSelection(select));
        calculateHandleOffsets();
    }

    public void removeSelection(Transform select)
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (selection[i].transform == select)
            {
                HighlighterTool.unhighlight(select);
                selection.RemoveAt(i);
                break;
            }
        }
        calculateHandleOffsets();
    }

    private void clearSelection()
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (selection[i].transform != null)
            {
                HighlighterTool.unhighlight(selection[i].transform);
            }
        }
        selection.Clear();
        calculateHandleOffsets();
    }

    public bool containsSelection(Transform select)
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (selection[i].transform == select)
            {
                return true;
            }
        }
        return false;
    }

    private void calculateHandleOffsets()
    {
        if (selection.Count == 0)
        {
            return;
        }
        if (dragCoordinate == EDragCoordinate.GLOBAL)
        {
            Vector3 zero = Vector3.zero;
            for (int i = 0; i < selection.Count; i++)
            {
                if (!(selection[i].transform == null))
                {
                    zero += selection[i].transform.position;
                }
            }
            zero /= (float)selection.Count;
            handles.SetPreferredPivot(zero, Quaternion.identity);
            return;
        }
        for (int j = 0; j < selection.Count; j++)
        {
            if (!(selection[j].transform == null))
            {
                handles.SetPreferredPivot(selection[j].transform.position, selection[j].transform.rotation);
                break;
            }
        }
    }

    private void stopDragging()
    {
        dragStartViewportPoint = Vector2.zero;
        dragStartScreenPoint = Vector2.zero;
        dragEndViewportPoint = Vector2.zero;
        dragEndScreenPoint = Vector2.zero;
        isDragging = false;
        onDragStopped?.Invoke();
    }

    private IEnumerable<GameObject> EnumerateSelectedGameObjects()
    {
        foreach (WorkzoneSelection item in selection)
        {
            if (item.transform != null)
            {
                yield return item.transform.gameObject;
            }
        }
    }

    private void Update()
    {
        if (!isBuilding)
        {
            hasDragStart = false;
            isUsingHandle = false;
            if (isDragging)
            {
                stopDragging();
                clearSelection();
            }
            return;
        }
        ray = MainCamera.instance.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out worldHit, 256f, RayMasks.EDITOR_WORLD);
        Physics.Raycast(ray, out buildableHit, 256f, RayMasks.EDITOR_BUILDABLE);
        handles.snapPositionInterval = snapTransform;
        handles.snapRotationIntervalDegrees = snapRotation;
        if (dragMode == EDragMode.TRANSFORM)
        {
            if (wantsBoundsEditor)
            {
                handles.SetPreferredMode(TransformHandles.EMode.PositionBounds);
                handles.UpdateBoundsFromSelection(EnumerateSelectedGameObjects());
            }
            else
            {
                handles.SetPreferredMode(TransformHandles.EMode.Position);
            }
        }
        else
        {
            handles.SetPreferredMode(TransformHandles.EMode.Rotation);
        }
        if (selection.Count > 0)
        {
            handles.Render(ray);
        }
        bool flag = selection.Count > 0 && handles.Raycast(ray);
        if (Glazier.Get().ShouldGameProcessInput)
        {
            if (InputEx.GetKey(ControlsSettings.secondary))
            {
                isUsingHandle = false;
                hasDragStart = false;
                if (isDragging)
                {
                    stopDragging();
                    clearSelection();
                }
                return;
            }
            if (isUsingHandle)
            {
                if (!InputEx.GetKey(ControlsSettings.primary))
                {
                    applySelection();
                    isUsingHandle = false;
                    handles.MouseUp();
                }
                else
                {
                    handles.wantsToSnap = InputEx.GetKey(ControlsSettings.snap);
                    handles.MouseMove(ray);
                }
            }
            if (InputEx.GetKeyDown(ControlsSettings.tool_0))
            {
                if (dragMode != 0)
                {
                    dragMode = EDragMode.TRANSFORM;
                }
                else
                {
                    wantsBoundsEditor = !wantsBoundsEditor;
                }
            }
            if (InputEx.GetKeyDown(ControlsSettings.tool_1))
            {
                dragMode = EDragMode.ROTATE;
            }
            if (InputEx.GetKeyDown(KeyCode.B) && selection.Count > 0 && InputEx.GetKey(KeyCode.LeftControl))
            {
                copyPosition = handles.GetPivotPosition();
                copyRotation = handles.GetPivotRotation();
                hasCopiedRotation = dragCoordinate == EDragCoordinate.LOCAL;
            }
            if (InputEx.GetKeyDown(KeyCode.N) && selection.Count > 0 && copyPosition != Vector3.zero && InputEx.GetKey(KeyCode.LeftControl))
            {
                pointSelection();
                if (selection.Count == 1)
                {
                    selection[0].transform.position = copyPosition;
                    if (hasCopiedRotation)
                    {
                        selection[0].transform.rotation = copyRotation;
                    }
                    calculateHandleOffsets();
                }
                else
                {
                    handles.ExternallyTransformPivot(copyPosition, copyRotation, hasCopiedRotation);
                }
                applySelection();
            }
            if (!isUsingHandle)
            {
                if (InputEx.GetKeyDown(ControlsSettings.primary))
                {
                    if (flag)
                    {
                        pointSelection();
                        isUsingHandle = true;
                        handles.MouseDown(ray);
                    }
                    else
                    {
                        Transform transform = buildableHit.transform;
                        if (transform != null)
                        {
                            transform = (transform.CompareTag("Barricade") ? DamageTool.getBarricadeRootTransform(transform) : ((!transform.CompareTag("Structure")) ? null : DamageTool.getStructureRootTransform(transform)));
                        }
                        if (transform != null)
                        {
                            if (InputEx.GetKey(ControlsSettings.modify))
                            {
                                if (containsSelection(transform))
                                {
                                    removeSelection(transform);
                                }
                                else
                                {
                                    addSelection(transform);
                                }
                            }
                            else if (containsSelection(transform))
                            {
                                clearSelection();
                            }
                            else
                            {
                                clearSelection();
                                addSelection(transform);
                            }
                        }
                        else
                        {
                            if (!isDragging)
                            {
                                hasDragStart = true;
                                dragStartViewportPoint = InputEx.NormalizedMousePosition;
                                dragStartScreenPoint = Input.mousePosition;
                            }
                            if (!InputEx.GetKey(ControlsSettings.modify))
                            {
                                clearSelection();
                            }
                        }
                    }
                }
                else if (InputEx.GetKey(ControlsSettings.primary) && hasDragStart)
                {
                    dragEndViewportPoint = InputEx.NormalizedMousePosition;
                    dragEndScreenPoint = Input.mousePosition;
                    if (isDragging || Mathf.Abs(dragEndScreenPoint.x - dragStartScreenPoint.x) > 50f || Mathf.Abs(dragEndScreenPoint.x - dragStartScreenPoint.x) > 50f)
                    {
                        Vector2 min = dragStartViewportPoint;
                        Vector2 max = dragEndViewportPoint;
                        if (max.x < min.x)
                        {
                            float x = max.x;
                            max.x = min.x;
                            min.x = x;
                        }
                        if (max.y < min.y)
                        {
                            float y = max.y;
                            max.y = min.y;
                            min.y = y;
                        }
                        onDragStarted?.Invoke(min, max);
                        if (!isDragging)
                        {
                            isDragging = true;
                            dragable.Clear();
                            byte region_x = Player.player.movement.region_x;
                            byte region_y = Player.player.movement.region_y;
                            if (Regions.checkSafe(region_x, region_y))
                            {
                                foreach (VehicleBarricadeRegion vehicleRegion in BarricadeManager.vehicleRegions)
                                {
                                    foreach (BarricadeDrop drop in vehicleRegion.drops)
                                    {
                                        if (!(drop.model == null))
                                        {
                                            Vector3 newScreen = MainCamera.instance.WorldToViewportPoint(drop.model.position);
                                            if (!(newScreen.z < 0f))
                                            {
                                                dragable.Add(new EditorDrag(drop.model, newScreen));
                                            }
                                        }
                                    }
                                }
                                for (int i = region_x - 1; i <= region_x + 1; i++)
                                {
                                    for (int j = region_y - 1; j <= region_y + 1; j++)
                                    {
                                        if (!Regions.checkSafe((byte)i, (byte)j))
                                        {
                                            continue;
                                        }
                                        for (int k = 0; k < BarricadeManager.regions[i, j].drops.Count; k++)
                                        {
                                            BarricadeDrop barricadeDrop = BarricadeManager.regions[i, j].drops[k];
                                            if (!(barricadeDrop.model == null))
                                            {
                                                Vector3 newScreen2 = MainCamera.instance.WorldToViewportPoint(barricadeDrop.model.position);
                                                if (!(newScreen2.z < 0f))
                                                {
                                                    dragable.Add(new EditorDrag(barricadeDrop.model, newScreen2));
                                                }
                                            }
                                        }
                                        foreach (StructureDrop drop2 in StructureManager.regions[i, j].drops)
                                        {
                                            Vector3 newScreen3 = MainCamera.instance.WorldToViewportPoint(drop2.model.position);
                                            if (!(newScreen3.z < 0f))
                                            {
                                                dragable.Add(new EditorDrag(drop2.model, newScreen3));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!InputEx.GetKey(ControlsSettings.modify))
                        {
                            for (int l = 0; l < selection.Count; l++)
                            {
                                if (!(selection[l].transform == null))
                                {
                                    Vector3 vector = MainCamera.instance.WorldToViewportPoint(selection[l].transform.position);
                                    if (vector.z < 0f)
                                    {
                                        removeSelection(selection[l].transform);
                                    }
                                    else if (vector.x < min.x || vector.y < min.y || vector.x > max.x || vector.y > max.y)
                                    {
                                        removeSelection(selection[l].transform);
                                    }
                                }
                            }
                        }
                        for (int m = 0; m < dragable.Count; m++)
                        {
                            EditorDrag editorDrag = dragable[m];
                            if (!(editorDrag.transform == null) && !containsSelection(editorDrag.transform) && !(editorDrag.screen.x < min.x) && !(editorDrag.screen.y < min.y) && !(editorDrag.screen.x > max.x) && !(editorDrag.screen.y > max.y))
                            {
                                addSelection(editorDrag.transform);
                            }
                        }
                    }
                }
                if (selection.Count > 0 && InputEx.GetKeyDown(ControlsSettings.tool_2) && worldHit.transform != null)
                {
                    pointSelection();
                    Vector3 point = worldHit.point;
                    if (InputEx.GetKey(ControlsSettings.snap))
                    {
                        point += worldHit.normal * snapTransform;
                    }
                    Quaternion pivotRotation = handles.GetPivotRotation();
                    handles.ExternallyTransformPivot(point, pivotRotation, modifyRotation: false);
                    applySelection();
                }
            }
        }
        if (InputEx.GetKeyUp(ControlsSettings.primary))
        {
            hasDragStart = false;
            if (isDragging)
            {
                stopDragging();
            }
        }
    }

    internal void InitializePlayer()
    {
        _isBuilding = false;
        selection = new List<WorkzoneSelection>();
        handles = new TransformHandles();
        handles.OnPreTransform += OnHandlePreTransform;
        handles.OnTranslatedAndRotated += OnHandleTranslatedAndRotated;
        dragMode = EDragMode.TRANSFORM;
        dragCoordinate = EDragCoordinate.GLOBAL;
        dragable = new List<EditorDrag>();
        snapTransform = 1f;
        snapRotation = 15f;
    }

    private void OnHandlePreTransform(Matrix4x4 worldToPivot)
    {
        foreach (WorkzoneSelection item in selection)
        {
            if (!(item.transform == null))
            {
                item.preTransformPosition = item.transform.position;
                item.preTransformRotation = item.transform.rotation;
            }
        }
    }

    private void OnHandleTranslatedAndRotated(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation)
    {
        foreach (WorkzoneSelection item in selection)
        {
            if (!(item.transform == null))
            {
                Vector3 vector = item.preTransformPosition - pivotPosition;
                if (!vector.IsNearlyZero())
                {
                    item.transform.position = pivotPosition + worldRotationDelta * vector + worldPositionDelta;
                }
                else
                {
                    item.transform.position = item.preTransformPosition + worldPositionDelta;
                }
                if (modifyRotation)
                {
                    item.transform.rotation = worldRotationDelta * item.preTransformRotation;
                }
            }
        }
        calculateHandleOffsets();
    }
}
