using UnityEngine;

namespace SDG.Unturned;

public class RoadPath
{
    public Transform vertex;

    private MeshRenderer meshRenderer;

    public Transform[] tangents;

    private MeshRenderer[] meshRenderers;

    private LineRenderer[] lineRenderers;

    public void highlightVertex()
    {
        meshRenderer.material.color = Color.red;
    }

    public void unhighlightVertex()
    {
        meshRenderer.material.color = Color.white;
    }

    public void highlightTangent(int index)
    {
        meshRenderers[index].material.color = Color.red;
        lineRenderers[index].material.color = Color.red;
    }

    public void unhighlightTangent(int index)
    {
        Color color = ((index != 0) ? Color.blue : Color.yellow);
        meshRenderers[index].material.color = color;
        lineRenderers[index].material.color = color;
    }

    public void setTangent(int index, Vector3 tangent)
    {
        tangents[index].localPosition = tangent;
        lineRenderers[index].SetPosition(1, -tangent);
    }

    public void remove()
    {
        Object.Destroy(vertex.gameObject);
    }

    public RoadPath(Transform vertex)
    {
        this.vertex = vertex;
        meshRenderer = vertex.GetComponent<MeshRenderer>();
        tangents = new Transform[2];
        tangents[0] = vertex.Find("Tangent_0");
        tangents[1] = vertex.Find("Tangent_1");
        meshRenderers = new MeshRenderer[2];
        meshRenderers[0] = tangents[0].GetComponent<MeshRenderer>();
        meshRenderers[1] = tangents[1].GetComponent<MeshRenderer>();
        lineRenderers = new LineRenderer[2];
        lineRenderers[0] = tangents[0].GetComponent<LineRenderer>();
        lineRenderers[1] = tangents[1].GetComponent<LineRenderer>();
        unhighlightVertex();
        unhighlightTangent(0);
        unhighlightTangent(1);
    }
}
