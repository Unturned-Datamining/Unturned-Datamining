using System;
using System.Collections.Generic;
using HighlightingSystem;
using UnityEngine;

namespace SDG.Unturned;

public class HighlighterTool
{
    private static List<Renderer> renderers = new List<Renderer>();

    public static void color(Transform target, Color color)
    {
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<Renderer>() != null)
        {
            target.GetComponent<Renderer>().material.color = color;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().material.color = color;
            }
        }
    }

    public static void destroyMaterials(Transform target)
    {
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<Renderer>() != null)
        {
            UnityEngine.Object.DestroyImmediate(target.GetComponent<Renderer>().material);
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                UnityEngine.Object.DestroyImmediate(transform.GetComponent<Renderer>().material);
            }
        }
    }

    public static void help(Transform target, bool isValid)
    {
        help(target, isValid, isRecursive: false);
    }

    public static void help(Transform target, bool isValid, bool isRecursive)
    {
        Material sharedMaterial = (isValid ? ((Material)Resources.Load("Materials/PlacementPreview_Valid")) : ((Material)Resources.Load("Materials/PlacementPreview_Invalid")));
        if (target.GetComponent<Renderer>() != null)
        {
            target.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = ((!isRecursive) ? target.Find("Model_" + i) : target.FindChildRecursive("Model_" + i));
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
            }
        }
    }

    public static void guide(Transform target)
    {
        Material sharedMaterial = (Material)Resources.Load("Materials/Guide");
        renderers.Clear();
        target.GetComponentsInChildren(includeInactive: true, renderers);
        for (int i = 0; i < renderers.Count; i++)
        {
            if (!(renderers[i].transform != target) || renderers[i].name.IndexOf("Model") != -1)
            {
                renderers[i].sharedMaterial = sharedMaterial;
            }
        }
        List<Collider> list = new List<Collider>();
        target.GetComponentsInChildren(list);
        for (int j = 0; j < list.Count; j++)
        {
            UnityEngine.Object.Destroy(list[j]);
        }
    }

    public static void highlight(Transform target, Color color)
    {
        if (!target.CompareTag("Player") && !target.CompareTag("Enemy") && !target.CompareTag("Zombie") && !target.CompareTag("Animal") && !target.CompareTag("Agent"))
        {
            Highlighter highlighter = target.GetComponent<Highlighter>();
            if (highlighter == null)
            {
                highlighter = target.gameObject.AddComponent<Highlighter>();
            }
            highlighter.ConstantOn(color);
        }
    }

    public static void unhighlight(Transform target)
    {
        Highlighter component = target.GetComponent<Highlighter>();
        if (!(component == null))
        {
            UnityEngine.Object.DestroyImmediate(component);
        }
    }

    public static void skin(Transform target, Material skin)
    {
        if (target.GetComponent<Renderer>() != null)
        {
            target.GetComponent<Renderer>().sharedMaterial = skin;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null) && transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().sharedMaterial = skin;
            }
        }
    }

    [Obsolete]
    public static Material getMaterial(Transform target)
    {
        if (target == null)
        {
            return null;
        }
        Renderer component = target.GetComponent<Renderer>();
        if (component != null)
        {
            return component.sharedMaterial;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (transform == null)
            {
                return null;
            }
            component = transform.GetComponent<Renderer>();
            if (component != null)
            {
                return component.sharedMaterial;
            }
        }
        return null;
    }

    public static Material getMaterialInstance(Transform target)
    {
        if (target == null)
        {
            return null;
        }
        Renderer component = target.GetComponent<Renderer>();
        if (component != null)
        {
            return component.material;
        }
        Material material = null;
        Material material2 = null;
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (transform == null)
            {
                break;
            }
            component = transform.GetComponent<Renderer>();
            if (component != null)
            {
                if (material == null)
                {
                    material2 = component.sharedMaterial;
                    material = component.material;
                }
                else if (component.sharedMaterial == material2)
                {
                    component.sharedMaterial = material;
                }
            }
        }
        return material;
    }

    public static void remesh(Transform target, List<Mesh> newMeshes, List<Mesh> outOldMeshes)
    {
        if (newMeshes == null || newMeshes.Count < 1)
        {
            return;
        }
        if (outOldMeshes != null && outOldMeshes != newMeshes)
        {
            outOldMeshes.Clear();
        }
        MeshFilter component = target.GetComponent<MeshFilter>();
        if (component != null)
        {
            Mesh sharedMesh = component.sharedMesh;
            component.sharedMesh = newMeshes[0];
            if (outOldMeshes != null)
            {
                if (outOldMeshes == newMeshes)
                {
                    newMeshes[0] = sharedMesh;
                }
                else
                {
                    outOldMeshes.Add(sharedMesh);
                }
            }
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (transform == null)
            {
                continue;
            }
            component = transform.GetComponent<MeshFilter>();
            if (!(component != null))
            {
                continue;
            }
            Mesh sharedMesh2 = component.sharedMesh;
            component.sharedMesh = ((i < newMeshes.Count) ? newMeshes[i] : newMeshes[0]);
            if (outOldMeshes == null)
            {
                continue;
            }
            if (outOldMeshes == newMeshes)
            {
                if (i < newMeshes.Count)
                {
                    newMeshes[i] = sharedMesh2;
                }
                else
                {
                    newMeshes.Add(sharedMesh2);
                }
            }
            else
            {
                outOldMeshes.Add(sharedMesh2);
            }
        }
    }

    public static void rematerialize(Transform target, Material newMaterial, out Material oldMaterial)
    {
        oldMaterial = null;
        Renderer component = target.GetComponent<Renderer>();
        if (component != null)
        {
            oldMaterial = component.sharedMaterial;
            component.sharedMaterial = newMaterial;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform transform = target.Find("Model_" + i);
            if (!(transform == null))
            {
                component = transform.GetComponent<Renderer>();
                if (component != null)
                {
                    oldMaterial = component.sharedMaterial;
                    component.sharedMaterial = newMaterial;
                }
            }
        }
    }
}
