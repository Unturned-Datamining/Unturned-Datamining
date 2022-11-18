using SDG.Framework.Devkit.Interactable;
using UnityEngine;

namespace SDG.Unturned;

public class Decal : MonoBehaviour, IDevkitInteractableBeginSelectionHandler, IDevkitInteractableEndSelectionHandler
{
    public EDecalType type;

    public Material material;

    public bool isSelected;

    public float lodBias = 1f;

    protected BoxCollider box;

    public virtual void beginSelection(InteractionData data)
    {
        isSelected = true;
    }

    public virtual void endSelection(InteractionData data)
    {
        isSelected = false;
    }

    private MeshRenderer getMesh()
    {
        MeshRenderer component = base.transform.parent.GetComponent<MeshRenderer>();
        if (component == null)
        {
            Transform transform = base.transform.parent.Find("Mesh");
            if (transform != null)
            {
                component = transform.GetComponent<MeshRenderer>();
            }
        }
        return component;
    }

    private void onGraphicsSettingsApplied()
    {
        MeshRenderer mesh = getMesh();
        if (mesh != null)
        {
            mesh.enabled = GraphicsSettings.renderMode == ERenderMode.FORWARD;
        }
        if (GraphicsSettings.renderMode == ERenderMode.DEFERRED)
        {
            DecalSystem.add(this);
        }
        else
        {
            DecalSystem.remove(this);
        }
    }

    internal void UpdateEditorVisibility()
    {
        if (box != null)
        {
            if (Level.isEditor)
            {
                box.enabled = DecalSystem.IsVisible;
            }
            else
            {
                box.enabled = false;
            }
        }
    }

    private void Awake()
    {
        box = base.transform.parent.GetComponent<BoxCollider>();
        UpdateEditorVisibility();
    }

    private void Start()
    {
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void DrawGizmo(bool selected)
    {
        Gizmos.color = (selected ? Color.yellow : Color.red);
        Gizmos.matrix = base.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private void OnDrawGizmos()
    {
        DrawGizmo(selected: false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo(selected: true);
    }
}
