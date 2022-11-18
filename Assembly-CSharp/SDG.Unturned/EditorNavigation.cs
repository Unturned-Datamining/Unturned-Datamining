using UnityEngine;

namespace SDG.Unturned;

public class EditorNavigation : MonoBehaviour
{
    private static bool _isPathfinding;

    private static Flag _flag;

    private static Transform selection;

    private static Transform marker;

    public static bool isPathfinding
    {
        get
        {
            return _isPathfinding;
        }
        set
        {
            _isPathfinding = value;
            marker.gameObject.SetActive(isPathfinding);
            if (!isPathfinding)
            {
                select(null);
            }
        }
    }

    public static Flag flag => _flag;

    private static void select(Transform select)
    {
        if (selection != null)
        {
            selection.GetComponent<Renderer>().material.color = Color.white;
        }
        if (selection == select || select == null)
        {
            selection = null;
            _flag = null;
        }
        else
        {
            selection = select;
            _flag = LevelNavigation.getFlag(selection);
            selection.GetComponent<Renderer>().material.color = Color.red;
        }
        EditorEnvironmentNavigationUI.updateSelection(flag);
    }

    private void Update()
    {
        if (!isPathfinding || EditorInteract.isFlying || !Glazier.Get().ShouldGameProcessInput)
        {
            return;
        }
        if (EditorInteract.worldHit.transform != null)
        {
            marker.position = EditorInteract.worldHit.point;
        }
        if ((InputEx.GetKeyDown(KeyCode.Delete) || InputEx.GetKeyDown(KeyCode.Backspace)) && selection != null)
        {
            Transform obj = selection;
            select(null);
            LevelNavigation.removeFlag(obj);
        }
        if (InputEx.GetKeyDown(ControlsSettings.tool_2) && EditorInteract.worldHit.transform != null && selection != null)
        {
            Vector3 point = EditorInteract.worldHit.point;
            flag.move(point);
        }
        if (!InputEx.GetKeyDown(ControlsSettings.primary))
        {
            return;
        }
        if (EditorInteract.logicHit.transform != null)
        {
            if (EditorInteract.logicHit.transform.name == "Flag")
            {
                select(EditorInteract.logicHit.transform);
            }
        }
        else if (EditorInteract.worldHit.transform != null)
        {
            select(LevelNavigation.addFlag(EditorInteract.worldHit.point));
        }
    }

    private void Start()
    {
        _isPathfinding = false;
        marker = ((GameObject)Object.Instantiate(Resources.Load("Edit/Marker"))).transform;
        marker.name = "Marker";
        marker.parent = Level.editing;
        marker.gameObject.SetActive(value: false);
        marker.GetComponent<Renderer>().material.color = Color.red;
        Object.Destroy(marker.GetComponent<Collider>());
    }
}
