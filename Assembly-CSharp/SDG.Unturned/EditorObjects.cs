using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class EditorObjects : MonoBehaviour
{
    public static readonly byte SAVEDATA_VERSION = 1;

    private static List<Decal> decals = new List<Decal>();

    public static DragStarted onDragStarted;

    public static DragStopped onDragStopped;

    public static float snapTransform;

    public static float snapRotation;

    private static bool _isBuilding;

    private Vector2 dragStartViewportPoint;

    private Vector2 dragStartScreenPoint;

    private Vector2 dragEndViewportPoint;

    private Vector2 dragEndScreenPoint;

    private bool hasDragStart;

    private bool isDragging;

    private bool isUsingHandle;

    public static ObjectAsset selectedObjectAsset;

    public static ItemAsset selectedItemAsset;

    private static List<EditorSelection> selection;

    private static List<EditorCopy> copies = new List<EditorCopy>();

    private static Vector3 copyPosition;

    private static Quaternion copyRotation;

    private static Vector3 copyScale;

    private static bool hasCopyScale;

    private static TransformHandles handles;

    private static EDragMode _dragMode;

    private static bool wantsBoundsEditor;

    private static EDragCoordinate _dragCoordinate;

    private static List<EditorDrag> dragable;

    public static bool isBuilding
    {
        get
        {
            return _isBuilding;
        }
        set
        {
            _isBuilding = value;
            if (!isBuilding)
            {
                clearSelection();
            }
        }
    }

    public static EDragMode dragMode
    {
        get
        {
            return _dragMode;
        }
        set
        {
            if (value == EDragMode.SCALE)
            {
                _dragCoordinate = EDragCoordinate.LOCAL;
            }
            else if (dragMode == EDragMode.SCALE)
            {
                _dragCoordinate = (EDragCoordinate)EditorLevelObjectsUI.coordinateButton.state;
            }
            wantsBoundsEditor = false;
            _dragMode = value;
            calculateHandleOffsets();
        }
    }

    public static EDragCoordinate dragCoordinate
    {
        get
        {
            return _dragCoordinate;
        }
        set
        {
            if (dragMode != EDragMode.SCALE)
            {
                _dragCoordinate = value;
                calculateHandleOffsets();
            }
        }
    }

    public static GameObject GetMostRecentSelectedGameObject()
    {
        if (selection.Count <= 0)
        {
            return null;
        }
        return selection[selection.Count - 1]?.transform?.gameObject;
    }

    public static void applySelection()
    {
        LevelObjects.step++;
        for (int i = 0; i < selection.Count; i++)
        {
            LevelObjects.registerTransformObject(selection[i].transform, selection[i].transform.position, selection[i].transform.rotation, selection[i].transform.localScale, selection[i].fromPosition, selection[i].fromRotation, selection[i].fromScale);
        }
    }

    public static void pointSelection()
    {
        for (int i = 0; i < selection.Count; i++)
        {
            selection[i].fromPosition = selection[i].transform.position;
            selection[i].fromRotation = selection[i].transform.rotation;
            selection[i].fromScale = selection[i].transform.localScale;
        }
    }

    private static void selectDecals(Transform select, bool isSelected)
    {
        decals.Clear();
        select.GetComponentsInChildren(includeInactive: true, decals);
        for (int i = 0; i < decals.Count; i++)
        {
            decals[i].isSelected = isSelected;
        }
    }

    public static void addSelection(Transform select)
    {
        HighlighterTool.highlight(select, Color.yellow);
        selectDecals(select, isSelected: true);
        selection.Add(new EditorSelection(select, select.position, select.rotation, select.localScale));
        calculateHandleOffsets();
    }

    public static void removeSelection(Transform select)
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (selection[i].transform == select)
            {
                HighlighterTool.unhighlight(select);
                selectDecals(select, isSelected: false);
                if (selection[i].transform.CompareTag("Barricade") || selection[i].transform.CompareTag("Structure"))
                {
                    selection[i].transform.localScale = Vector3.one;
                }
                selection.RemoveAt(i);
                break;
            }
        }
        calculateHandleOffsets();
    }

    private static void clearSelection()
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (selection[i].transform != null)
            {
                HighlighterTool.unhighlight(selection[i].transform);
                selectDecals(selection[i].transform, isSelected: false);
                if (selection[i].transform.CompareTag("Barricade") || selection[i].transform.CompareTag("Structure"))
                {
                    selection[i].transform.localScale = Vector3.one;
                }
            }
        }
        selection.Clear();
        calculateHandleOffsets();
    }

    public static bool containsSelection(Transform select)
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

    private static void calculateHandleOffsets()
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
                zero += selection[i].transform.position;
            }
            zero /= (float)selection.Count;
            handles.SetPreferredPivot(zero, Quaternion.identity);
        }
        else
        {
            handles.SetPreferredPivot(selection[0].transform.position, selection[0].transform.rotation);
        }
    }

    private void OnHandlePreTransform(Matrix4x4 worldToPivot)
    {
        foreach (EditorSelection item in selection)
        {
            item.fromPosition = item.transform.position;
            item.fromRotation = item.transform.rotation;
            item.fromScale = item.transform.localScale;
            item.relativeToPivot = worldToPivot * item.transform.localToWorldMatrix;
        }
    }

    private void OnHandleTranslatedAndRotated(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation)
    {
        foreach (EditorSelection item in selection)
        {
            Vector3 vector = item.fromPosition - pivotPosition;
            if (!vector.IsNearlyZero())
            {
                item.transform.position = pivotPosition + worldRotationDelta * vector + worldPositionDelta;
            }
            else
            {
                item.transform.position = item.fromPosition + worldPositionDelta;
            }
            if (modifyRotation)
            {
                item.transform.rotation = worldRotationDelta * item.fromRotation;
            }
        }
        calculateHandleOffsets();
    }

    private void OnHandleTransformed(Matrix4x4 pivotToWorld)
    {
        foreach (EditorSelection item in selection)
        {
            Matrix4x4 matrix = pivotToWorld * item.relativeToPivot;
            item.transform.position = matrix.GetPosition();
            item.transform.SetRotation_RoundIfNearlyAxisAligned(matrix.GetRotation());
            item.transform.SetLocalScale_RoundIfNearlyEqualToOne(matrix.lossyScale);
        }
        calculateHandleOffsets();
    }

    private void releaseHandle()
    {
        applySelection();
        isUsingHandle = false;
        handles.MouseUp();
    }

    private void stopDragging()
    {
        dragStartViewportPoint = Vector2.zero;
        dragStartScreenPoint = Vector2.zero;
        dragEndViewportPoint = Vector2.zero;
        dragEndScreenPoint = Vector2.zero;
        isDragging = false;
        if (onDragStopped != null)
        {
            onDragStopped();
        }
    }

    private IEnumerable<GameObject> EnumerateSelectedGameObjects()
    {
        foreach (EditorSelection item in selection)
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
            return;
        }
        if (Glazier.Get().ShouldGameProcessInput)
        {
            if (EditorInteract.isFlying)
            {
                if (isUsingHandle)
                {
                    releaseHandle();
                }
                hasDragStart = false;
                if (isDragging)
                {
                    stopDragging();
                    clearSelection();
                }
                return;
            }
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
            else if (dragMode == EDragMode.SCALE)
            {
                if (wantsBoundsEditor)
                {
                    handles.SetPreferredMode(TransformHandles.EMode.ScaleBounds);
                    handles.UpdateBoundsFromSelection(EnumerateSelectedGameObjects());
                }
                else
                {
                    handles.SetPreferredMode(TransformHandles.EMode.Scale);
                }
            }
            else
            {
                handles.SetPreferredMode(TransformHandles.EMode.Rotation);
            }
            bool flag = selection.Count > 0 && handles.Raycast(EditorInteract.ray);
            if (selection.Count > 0)
            {
                handles.Render(EditorInteract.ray);
            }
            if (isUsingHandle)
            {
                if (!InputEx.GetKey(ControlsSettings.primary))
                {
                    releaseHandle();
                    return;
                }
                handles.wantsToSnap = InputEx.GetKey(ControlsSettings.snap);
                handles.MouseMove(EditorInteract.ray);
                return;
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
            if (InputEx.GetKeyDown(ControlsSettings.tool_3))
            {
                if (dragMode != EDragMode.SCALE)
                {
                    dragMode = EDragMode.SCALE;
                }
                else
                {
                    wantsBoundsEditor = !wantsBoundsEditor;
                }
            }
            if ((InputEx.GetKeyDown(KeyCode.Delete) || InputEx.GetKeyDown(KeyCode.Backspace)) && selection.Count > 0)
            {
                LevelObjects.step++;
                for (int i = 0; i < selection.Count; i++)
                {
                    LevelObjects.registerRemoveObject(selection[i].transform);
                }
                selection.Clear();
                calculateHandleOffsets();
            }
            if (InputEx.GetKeyDown(KeyCode.Z) && InputEx.GetKey(KeyCode.LeftControl))
            {
                clearSelection();
                LevelObjects.undo();
            }
            if (InputEx.GetKeyDown(KeyCode.X) && InputEx.GetKey(KeyCode.LeftControl))
            {
                clearSelection();
                LevelObjects.redo();
            }
            if (InputEx.GetKeyDown(KeyCode.B) && selection.Count > 0 && InputEx.GetKey(KeyCode.LeftControl))
            {
                copyPosition = handles.GetPivotPosition();
                copyRotation = handles.GetPivotRotation();
                if (selection.Count == 1)
                {
                    copyScale = selection[0].transform.localScale;
                    hasCopyScale = true;
                }
                else
                {
                    copyScale = Vector3.one;
                    hasCopyScale = false;
                }
            }
            if (InputEx.GetKeyDown(KeyCode.N) && selection.Count > 0 && copyPosition != Vector3.zero && InputEx.GetKey(KeyCode.LeftControl))
            {
                pointSelection();
                if (selection.Count == 1)
                {
                    selection[0].transform.position = copyPosition;
                    selection[0].transform.rotation = copyRotation;
                    if (hasCopyScale)
                    {
                        selection[0].transform.localScale = copyScale;
                    }
                    calculateHandleOffsets();
                }
                else
                {
                    handles.ExternallyTransformPivot(copyPosition, copyRotation, modifyRotation: true);
                }
                applySelection();
            }
            if (InputEx.GetKeyDown(KeyCode.C) && selection.Count > 0 && InputEx.GetKey(KeyCode.LeftControl))
            {
                copies.Clear();
                for (int j = 0; j < selection.Count; j++)
                {
                    LevelObjects.getAssetEditor(selection[j].transform, out var objectAsset, out var itemAsset);
                    if (objectAsset != null || itemAsset != null)
                    {
                        copies.Add(new EditorCopy(selection[j].transform.position, selection[j].transform.rotation, selection[j].transform.localScale, objectAsset, itemAsset));
                    }
                }
            }
            if (InputEx.GetKeyDown(KeyCode.V) && copies.Count > 0 && InputEx.GetKey(KeyCode.LeftControl))
            {
                clearSelection();
                LevelObjects.step++;
                for (int k = 0; k < copies.Count; k++)
                {
                    Transform transform = LevelObjects.registerAddObject(copies[k].position, copies[k].rotation, copies[k].scale, copies[k].objectAsset, copies[k].itemAsset);
                    if (transform != null)
                    {
                        addSelection(transform);
                    }
                }
            }
            if (!isUsingHandle)
            {
                if (InputEx.GetKeyDown(ControlsSettings.primary))
                {
                    if (flag)
                    {
                        pointSelection();
                        handles.MouseDown(EditorInteract.ray);
                        isUsingHandle = true;
                    }
                    else if (EditorInteract.objectHit.transform != null)
                    {
                        if (InputEx.GetKey(ControlsSettings.modify))
                        {
                            if (containsSelection(EditorInteract.objectHit.transform))
                            {
                                removeSelection(EditorInteract.objectHit.transform);
                            }
                            else
                            {
                                addSelection(EditorInteract.objectHit.transform);
                            }
                        }
                        else if (containsSelection(EditorInteract.objectHit.transform))
                        {
                            clearSelection();
                        }
                        else
                        {
                            clearSelection();
                            addSelection(EditorInteract.objectHit.transform);
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
                        if (onDragStarted != null)
                        {
                            onDragStarted(min, max);
                        }
                        if (!isDragging)
                        {
                            isDragging = true;
                            dragable.Clear();
                            byte region_x = Editor.editor.area.region_x;
                            byte region_y = Editor.editor.area.region_y;
                            if (Regions.checkSafe(region_x, region_y))
                            {
                                for (int l = region_x - 1; l <= region_x + 1; l++)
                                {
                                    for (int m = region_y - 1; m <= region_y + 1; m++)
                                    {
                                        if (!Regions.checkSafe((byte)l, (byte)m) || !LevelObjects.regions[l, m])
                                        {
                                            continue;
                                        }
                                        for (int n = 0; n < LevelObjects.objects[l, m].Count; n++)
                                        {
                                            LevelObject levelObject = LevelObjects.objects[l, m][n];
                                            if (!(levelObject.transform == null))
                                            {
                                                Vector3 newScreen = MainCamera.instance.WorldToViewportPoint(levelObject.transform.position);
                                                if (!(newScreen.z < 0f))
                                                {
                                                    dragable.Add(new EditorDrag(levelObject.transform, newScreen));
                                                }
                                            }
                                        }
                                        for (int num = 0; num < LevelObjects.buildables[l, m].Count; num++)
                                        {
                                            LevelBuildableObject levelBuildableObject = LevelObjects.buildables[l, m][num];
                                            if (!(levelBuildableObject.transform == null))
                                            {
                                                Vector3 newScreen2 = MainCamera.instance.WorldToViewportPoint(levelBuildableObject.transform.position);
                                                if (!(newScreen2.z < 0f))
                                                {
                                                    dragable.Add(new EditorDrag(levelBuildableObject.transform, newScreen2));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!InputEx.GetKey(ControlsSettings.modify))
                        {
                            for (int num2 = 0; num2 < selection.Count; num2++)
                            {
                                Vector3 vector = MainCamera.instance.WorldToViewportPoint(selection[num2].transform.position);
                                if (vector.z < 0f)
                                {
                                    removeSelection(selection[num2].transform);
                                }
                                else if (vector.x < min.x || vector.y < min.y || vector.x > max.x || vector.y > max.y)
                                {
                                    removeSelection(selection[num2].transform);
                                }
                            }
                        }
                        for (int num3 = 0; num3 < dragable.Count; num3++)
                        {
                            EditorDrag editorDrag = dragable[num3];
                            if (!(editorDrag.transform == null) && !containsSelection(editorDrag.transform) && !(editorDrag.screen.x < min.x) && !(editorDrag.screen.y < min.y) && !(editorDrag.screen.x > max.x) && !(editorDrag.screen.y > max.y))
                            {
                                addSelection(editorDrag.transform);
                            }
                        }
                    }
                }
                if (selection.Count > 0)
                {
                    if (InputEx.GetKeyDown(ControlsSettings.tool_2) && EditorInteract.worldHit.transform != null)
                    {
                        pointSelection();
                        Vector3 point = EditorInteract.worldHit.point;
                        if (InputEx.GetKey(ControlsSettings.snap))
                        {
                            point += EditorInteract.worldHit.normal * snapTransform;
                        }
                        Quaternion pivotRotation = handles.GetPivotRotation();
                        handles.ExternallyTransformPivot(point, pivotRotation, modifyRotation: false);
                        applySelection();
                    }
                    if (InputEx.GetKeyDown(ControlsSettings.focus))
                    {
                        MainCamera.instance.transform.parent.position = handles.GetPivotPosition() - 15f * MainCamera.instance.transform.forward;
                    }
                }
                else if (EditorInteract.worldHit.transform != null)
                {
                    if (EditorInteract.worldHit.transform.CompareTag("Large") || EditorInteract.worldHit.transform.CompareTag("Medium") || EditorInteract.worldHit.transform.CompareTag("Small") || EditorInteract.worldHit.transform.CompareTag("Barricade") || EditorInteract.worldHit.transform.CompareTag("Structure"))
                    {
                        LevelObjects.getAssetEditor(EditorInteract.worldHit.transform, out var objectAsset2, out var itemAsset2);
                        if (objectAsset2 != null)
                        {
                            EditorUI.hint(EEditorMessage.FOCUS, objectAsset2.objectName + "\n" + (objectAsset2.origin?.name ?? "Unknown"));
                        }
                        else if (itemAsset2 != null)
                        {
                            EditorUI.hint(EEditorMessage.FOCUS, itemAsset2.itemName + "\n" + (itemAsset2.origin?.name ?? "Unknown"));
                        }
                    }
                    if (InputEx.GetKeyDown(ControlsSettings.tool_2))
                    {
                        Vector3 point2 = EditorInteract.worldHit.point;
                        if (InputEx.GetKey(ControlsSettings.snap))
                        {
                            point2 += EditorInteract.worldHit.normal * snapTransform;
                        }
                        Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
                        handles.SetPreferredPivot(point2, rotation);
                        if (selectedObjectAsset != null || selectedItemAsset != null)
                        {
                            LevelObjects.step++;
                            Transform transform2 = LevelObjects.registerAddObject(point2, rotation, Vector3.one, selectedObjectAsset, selectedItemAsset);
                            if (transform2 != null)
                            {
                                addSelection(transform2);
                            }
                        }
                    }
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

    private void Start()
    {
        _isBuilding = false;
        selection = new List<EditorSelection>();
        handles = new TransformHandles();
        handles.OnPreTransform += OnHandlePreTransform;
        handles.OnTranslatedAndRotated += OnHandleTranslatedAndRotated;
        handles.OnTransformed += OnHandleTransformed;
        dragMode = EDragMode.TRANSFORM;
        dragCoordinate = EDragCoordinate.GLOBAL;
        dragable = new List<EditorDrag>();
        if (ReadWrite.fileExists(Level.info.path + "/Editor/Objects.dat", useCloud: false, usePath: false))
        {
            Block block = ReadWrite.readBlock(Level.info.path + "/Editor/Objects.dat", useCloud: false, usePath: false, 1);
            snapTransform = block.readSingle();
            snapRotation = block.readSingle();
        }
        else
        {
            snapTransform = 1f;
            snapRotation = 15f;
        }
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeSingle(snapTransform);
        block.writeSingle(snapRotation);
        ReadWrite.writeBlock(Level.info.path + "/Editor/Objects.dat", useCloud: false, usePath: false, block);
    }
}
