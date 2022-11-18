using UnityEngine;

namespace SDG.Unturned;

public class EditorRoads : MonoBehaviour
{
    private static bool _isPaving;

    public static byte selected;

    private static Road _road;

    private static RoadPath _path;

    private static RoadJoint _joint;

    private static int vertexIndex;

    private static int tangentIndex;

    private static Transform selection;

    private static Transform highlighter;

    public static bool isPaving
    {
        get
        {
            return _isPaving;
        }
        set
        {
            _isPaving = value;
            highlighter.gameObject.SetActive(isPaving);
            if (!isPaving)
            {
                select(null);
            }
        }
    }

    public static Road road => _road;

    public static RoadPath path => _path;

    public static RoadJoint joint => _joint;

    private static void select(Transform target)
    {
        if (road != null)
        {
            if (tangentIndex > -1)
            {
                path.unhighlightTangent(tangentIndex);
            }
            else if (vertexIndex > -1)
            {
                path.unhighlightVertex();
            }
        }
        if (selection == target || target == null)
        {
            deselect();
        }
        else
        {
            selection = target;
            _road = LevelRoads.getRoad(selection, out vertexIndex, out tangentIndex);
            if (road != null)
            {
                _path = road.paths[vertexIndex];
                _joint = road.joints[vertexIndex];
                if (tangentIndex > -1)
                {
                    path.highlightTangent(tangentIndex);
                }
                else if (vertexIndex > -1)
                {
                    path.highlightVertex();
                }
            }
            else
            {
                _path = null;
                _joint = null;
            }
        }
        EditorEnvironmentRoadsUI.updateSelection(road, joint);
    }

    private static void deselect()
    {
        selection = null;
        _road = null;
        _path = null;
        _joint = null;
        vertexIndex = -1;
        tangentIndex = -1;
    }

    private void Update()
    {
        if (!isPaving || EditorInteract.isFlying || !Glazier.Get().ShouldGameProcessInput)
        {
            return;
        }
        if (EditorInteract.worldHit.transform != null)
        {
            highlighter.position = EditorInteract.worldHit.point;
        }
        if ((InputEx.GetKeyDown(KeyCode.Delete) || InputEx.GetKeyDown(KeyCode.Backspace)) && selection != null && road != null)
        {
            if (InputEx.GetKey(ControlsSettings.other))
            {
                LevelRoads.removeRoad(road);
            }
            else
            {
                road.removeVertex(vertexIndex);
            }
            deselect();
        }
        if (InputEx.GetKeyDown(ControlsSettings.tool_2) && EditorInteract.worldHit.transform != null)
        {
            Vector3 point = EditorInteract.worldHit.point;
            if (road != null)
            {
                if (tangentIndex > -1)
                {
                    road.moveTangent(vertexIndex, tangentIndex, point - joint.vertex);
                }
                else if (vertexIndex > -1)
                {
                    road.moveVertex(vertexIndex, point);
                }
            }
        }
        if (!InputEx.GetKeyDown(ControlsSettings.primary))
        {
            return;
        }
        if (EditorInteract.logicHit.transform != null)
        {
            if (EditorInteract.logicHit.transform.name.IndexOf("Path") != -1 || EditorInteract.logicHit.transform.name.IndexOf("Tangent") != -1)
            {
                select(EditorInteract.logicHit.transform);
            }
        }
        else
        {
            if (!(EditorInteract.worldHit.transform != null))
            {
                return;
            }
            Vector3 point2 = EditorInteract.worldHit.point;
            if (road != null)
            {
                if (tangentIndex > -1)
                {
                    select(road.addVertex(vertexIndex + tangentIndex, point2));
                    return;
                }
                float num = Vector3.Dot(point2 - joint.vertex, joint.getTangent(0));
                float num2 = Vector3.Dot(point2 - joint.vertex, joint.getTangent(1));
                if (num > num2)
                {
                    select(road.addVertex(vertexIndex, point2));
                }
                else
                {
                    select(road.addVertex(vertexIndex + 1, point2));
                }
            }
            else
            {
                select(LevelRoads.addRoad(point2));
            }
        }
    }

    private void Start()
    {
        _isPaving = false;
        highlighter = ((GameObject)Object.Instantiate(Resources.Load("Edit/Highlighter"))).transform;
        highlighter.name = "Highlighter";
        highlighter.parent = Level.editing;
        highlighter.gameObject.SetActive(value: false);
        highlighter.GetComponent<Renderer>().material.color = Color.red;
        deselect();
    }
}
