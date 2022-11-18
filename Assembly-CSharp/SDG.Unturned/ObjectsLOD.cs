using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ObjectsLOD : MonoBehaviour
{
    private static List<MeshFilter> meshes = new List<MeshFilter>();

    private static List<OcclusionArea> areas = new List<OcclusionArea>();

    public EObjectLOD lod;

    public float bias;

    public Vector3 center;

    public Vector3 size;

    private Vector3 cullCenter;

    private float cullMagnitude;

    private float sqrCullMagnitude;

    private List<Bounds> bounds;

    private List<LevelObject> objects;

    private bool isCulled;

    private int load;

    private void Update()
    {
        if (objects == null || objects.Count == 0 || MainCamera.instance == null)
        {
            return;
        }
        if ((cullCenter - MainCamera.instance.transform.position).sqrMagnitude < sqrCullMagnitude)
        {
            if (isCulled)
            {
                isCulled = false;
                load = 0;
            }
        }
        else if (!isCulled)
        {
            isCulled = true;
            load = 0;
        }
        if (load == -1)
        {
            return;
        }
        if (load >= objects.Count)
        {
            load = -1;
            return;
        }
        if (isCulled)
        {
            if (objects[load].isVisualEnabled)
            {
                objects[load].disableVisual();
            }
        }
        else if (!objects[load].isVisualEnabled)
        {
            objects[load].enableVisual();
        }
        load++;
    }

    private void OnDrawGizmos()
    {
        if (this.bounds != null && this.bounds.Count != 0)
        {
            if (objects.Count == 0)
            {
                Gizmos.color = Color.black;
            }
            else if (isCulled)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Matrix4x4 matrix = Gizmos.matrix;
            Gizmos.matrix = base.transform.localToWorldMatrix;
            for (int i = 0; i < this.bounds.Count; i++)
            {
                Bounds bounds = this.bounds[i];
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            Gizmos.matrix = matrix;
            Gizmos.DrawWireCube(cullCenter, Vector3.one);
        }
    }

    private void OnDisable()
    {
        if (objects == null || objects.Count == 0)
        {
            return;
        }
        isCulled = true;
        load = -1;
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].isVisualEnabled)
            {
                objects[i].disableVisual();
            }
        }
    }

    private void findInBounds(Bounds bound)
    {
        Regions.tryGetCoordinate(base.transform.TransformPoint(bound.min), out var x, out var y);
        Regions.tryGetCoordinate(base.transform.TransformPoint(bound.max), out var x2, out var y2);
        for (byte b = x; b <= x2; b = (byte)(b + 1))
        {
            for (byte b2 = y; b2 <= y2; b2 = (byte)(b2 + 1))
            {
                for (int i = 0; i < LevelObjects.objects[b, b2].Count; i++)
                {
                    LevelObject levelObject = LevelObjects.objects[b, b2][i];
                    if (levelObject.asset != null && !(levelObject.transform == null) && levelObject.asset.type != 0 && !levelObject.isSpeciallyCulled)
                    {
                        Vector3 point = base.transform.InverseTransformPoint(levelObject.transform.position);
                        if (bound.Contains(point))
                        {
                            levelObject.isSpeciallyCulled = true;
                            objects.Add(levelObject);
                        }
                    }
                }
            }
        }
    }

    public void calculateBounds()
    {
        cullMagnitude = 64f * bias;
        sqrCullMagnitude = cullMagnitude * cullMagnitude;
        if (lod == EObjectLOD.MESH)
        {
            meshes.Clear();
            GetComponentsInChildren(includeInactive: true, meshes);
            if (meshes.Count == 0)
            {
                base.enabled = false;
                return;
            }
            Bounds item = default(Bounds);
            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh sharedMesh = meshes[i].sharedMesh;
                if (!(sharedMesh == null))
                {
                    Bounds bounds = sharedMesh.bounds;
                    item.Encapsulate(bounds.min);
                    item.Encapsulate(bounds.max);
                }
            }
            item.Expand(-1f);
            item.center += center;
            item.size += size;
            if (item.size.x < 1f || item.size.y < 1f || item.size.z < 1f)
            {
                base.enabled = false;
                return;
            }
            this.bounds = new List<Bounds>();
            this.bounds.Add(item);
        }
        else if (lod == EObjectLOD.AREA)
        {
            areas.Clear();
            GetComponentsInChildren(includeInactive: true, areas);
            if (areas.Count == 0)
            {
                base.enabled = false;
                return;
            }
            this.bounds = new List<Bounds>();
            for (int j = 0; j < areas.Count; j++)
            {
                OcclusionArea occlusionArea = areas[j];
                Bounds item2 = new Bounds(occlusionArea.transform.localPosition + occlusionArea.center, new Vector3(occlusionArea.size.x, occlusionArea.size.z, occlusionArea.size.y));
                this.bounds.Add(item2);
            }
        }
        objects = new List<LevelObject>();
        for (int k = 0; k < this.bounds.Count; k++)
        {
            cullCenter += this.bounds[k].center;
        }
        cullCenter /= (float)this.bounds.Count;
        cullCenter = base.transform.TransformPoint(cullCenter);
        Regions.tryGetCoordinate(cullCenter, out var x, out var y);
        for (int l = x - 1; l <= x + 1; l++)
        {
            for (int m = y - 1; m <= y + 1; m++)
            {
                if (!Regions.checkSafe(l, m))
                {
                    continue;
                }
                for (int n = 0; n < LevelObjects.objects[l, m].Count; n++)
                {
                    LevelObject levelObject = LevelObjects.objects[l, m][n];
                    if (levelObject.asset == null || levelObject.transform == null || levelObject.asset.type == EObjectType.LARGE || levelObject.isSpeciallyCulled)
                    {
                        continue;
                    }
                    Vector3 point = base.transform.InverseTransformPoint(levelObject.transform.position);
                    bool flag = false;
                    for (int num = 0; num < this.bounds.Count; num++)
                    {
                        if (this.bounds[num].Contains(point))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        levelObject.isSpeciallyCulled = true;
                        objects.Add(levelObject);
                    }
                }
            }
        }
        if (objects.Count == 0)
        {
            base.enabled = false;
        }
    }
}
