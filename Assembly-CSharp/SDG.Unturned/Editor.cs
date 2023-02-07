using UnityEngine;

namespace SDG.Unturned;

public class Editor : MonoBehaviour
{
    public static EditorCreated onEditorCreated;

    private static Editor _editor;

    private EditorArea _area;

    public static Editor editor => _editor;

    public EditorArea area => _area;

    public virtual void init()
    {
        _area = GetComponent<EditorArea>();
        _editor = this;
        onEditorCreated?.Invoke();
    }

    private void Start()
    {
        init();
    }

    public static void save()
    {
        EditorInteract.save();
        EditorObjects.save();
        EditorSpawns.save();
    }
}
