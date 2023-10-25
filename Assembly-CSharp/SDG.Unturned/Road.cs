using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

public class Road
{
    public byte material;

    private Transform _road;

    private Transform line;

    private LineRenderer lineRenderer;

    private bool _isLoop;

    private List<RoadJoint> _joints;

    private List<RoadSample> samples;

    private List<TrackSample> trackSamples;

    private List<RoadPath> _paths;

    /// <summary>
    /// Only set in play mode for determing if we should cache brute force lengths.
    /// </summary>
    public ushort roadIndex { get; protected set; }

    public Transform road => _road;

    public bool isLoop
    {
        get
        {
            return _isLoop;
        }
        set
        {
            _isLoop = value;
            updatePoints();
        }
    }

    public List<RoadJoint> joints => _joints;

    public float trackSampledLength { get; protected set; }

    public List<RoadPath> paths => _paths;

    public void setEnabled(bool isEnabled)
    {
        line.gameObject.SetActive(isEnabled);
        for (int i = 0; i < paths.Count; i++)
        {
            paths[i].vertex.gameObject.SetActive(isEnabled);
        }
    }

    public void getTrackData(float trackPosition, out Vector3 position, out Vector3 normal, out Vector3 direction)
    {
        if (trackSamples.Count > 1)
        {
            TrackSample trackSample = trackSamples[0];
            for (int i = 1; i < trackSamples.Count; i++)
            {
                TrackSample trackSample2 = trackSamples[i];
                if (trackPosition >= trackSample.distance && trackPosition <= trackSample2.distance)
                {
                    float t = (trackPosition - trackSample.distance) / (trackSample2.distance - trackSample.distance);
                    position = Vector3.Lerp(trackSample.position, trackSample2.position, t);
                    normal = Vector3.Lerp(trackSample.normal, trackSample2.normal, t);
                    direction = Vector3.Lerp(trackSample.direction, trackSample2.direction, t);
                    return;
                }
                trackSample = trackSample2;
            }
            if (isLoop)
            {
                TrackSample trackSample3 = trackSamples[0];
                if (trackSample != trackSample3)
                {
                    float t2 = (trackPosition - trackSample.distance) / (trackSampledLength - trackSample.distance);
                    position = Vector3.Lerp(trackSample.position, trackSample3.position, t2);
                    normal = Vector3.Lerp(trackSample.normal, trackSample3.normal, t2);
                    direction = Vector3.Lerp(trackSample.direction, trackSample3.direction, t2);
                    return;
                }
            }
        }
        position = Vector3.zero;
        normal = Vector3.up;
        direction = Vector3.forward;
    }

    public void getTrackPosition(int index, float t, out Vector3 position, out Vector3 normal)
    {
        position = getPosition(index, t);
        normal = Vector3.up;
        if (!joints[index].ignoreTerrain)
        {
            position.y = LevelGround.getHeight(position);
            normal = LevelGround.getNormal(position);
        }
        position += normal * (LevelRoads.materials[material].depth + LevelRoads.materials[material].offset);
    }

    public void getTrackPosition(float t, out int index, out Vector3 position, out Vector3 normal)
    {
        position = getPosition(t, out index);
        normal = Vector3.up;
        if (!joints[index].ignoreTerrain)
        {
            position.y = LevelGround.getHeight(position);
            normal = LevelGround.getNormal(position);
        }
        position += normal * (LevelRoads.materials[material].depth + LevelRoads.materials[material].offset);
    }

    public Vector3 getPosition(float t)
    {
        int index;
        return getPosition(t, out index);
    }

    public Vector3 getPosition(float t, out int index)
    {
        if (isLoop)
        {
            index = (int)(t * (float)joints.Count);
            t = t * (float)joints.Count - (float)index;
            return getPosition(index, t);
        }
        index = (int)(t * (float)(joints.Count - 1));
        t = t * (float)(joints.Count - 1) - (float)index;
        return getPosition(index, t);
    }

    public Vector3 getPosition(int index, float t)
    {
        index = Mathf.Clamp(index, 0, joints.Count - 1);
        t = Mathf.Clamp01(t);
        RoadJoint roadJoint = joints[index];
        RoadJoint roadJoint2 = ((index != joints.Count - 1) ? joints[index + 1] : joints[0]);
        Vector3 tangent = roadJoint.getTangent(1);
        Vector3 tangent2 = roadJoint2.getTangent(0);
        if (Vector3.Dot(tangent.normalized, tangent2.normalized) < -0.999f)
        {
            return Vector3.Lerp(roadJoint.vertex, roadJoint2.vertex, t);
        }
        return BezierTool.getPosition(roadJoint.vertex, roadJoint.vertex + tangent, roadJoint2.vertex + tangent2, roadJoint2.vertex, t);
    }

    public Vector3 getVelocity(float t)
    {
        if (isLoop)
        {
            int num = (int)(t * (float)joints.Count);
            t = t * (float)joints.Count - (float)num;
            return getVelocity(num, t);
        }
        int num2 = (int)(t * (float)(joints.Count - 1));
        t = t * (float)(joints.Count - 1) - (float)num2;
        return getVelocity(num2, t);
    }

    public Vector3 getVelocity(int index, float t)
    {
        index = Mathf.Clamp(index, 0, joints.Count - 1);
        t = Mathf.Clamp01(t);
        RoadJoint roadJoint = joints[index];
        RoadJoint roadJoint2 = ((index != joints.Count - 1) ? joints[index + 1] : joints[0]);
        return BezierTool.getVelocity(roadJoint.vertex, roadJoint.vertex + roadJoint.getTangent(1), roadJoint2.vertex + roadJoint2.getTangent(0), roadJoint2.vertex, t);
    }

    public float getLengthEstimate()
    {
        double num = 0.0;
        for (int i = 0; i < joints.Count - 1 + (isLoop ? 1 : 0); i++)
        {
            num += (double)getLengthEstimate(i);
        }
        return (float)num;
    }

    public float getLengthEstimate(int index)
    {
        index = Mathf.Clamp(index, 0, joints.Count - 1);
        RoadJoint roadJoint = joints[index];
        RoadJoint roadJoint2 = ((index != joints.Count - 1) ? joints[index + 1] : joints[0]);
        Vector3 tangent = roadJoint.getTangent(1);
        Vector3 tangent2 = roadJoint2.getTangent(0);
        if (Vector3.Dot(tangent.normalized, tangent2.normalized) < -0.999f)
        {
            return (roadJoint2.vertex - roadJoint.vertex).magnitude;
        }
        return BezierTool.getLengthEstimate(roadJoint.vertex, roadJoint.vertex + tangent, roadJoint2.vertex + tangent2, roadJoint2.vertex);
    }

    [Obsolete]
    public Transform addPoint(Transform origin, Vector3 point)
    {
        RoadJoint roadJoint = new RoadJoint(point);
        if (origin == null || origin == paths[paths.Count - 1].vertex)
        {
            if (joints.Count > 0)
            {
                roadJoint.setTangent(0, (joints[joints.Count - 1].vertex - point).normalized * 2.5f);
            }
            joints.Add(roadJoint);
            Transform transform = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Edit/Path"))).transform;
            transform.name = "Path_" + (joints.Count - 1);
            transform.parent = line;
            RoadPath roadPath = new RoadPath(transform);
            paths.Add(roadPath);
            updatePoints();
            return roadPath.vertex;
        }
        if (origin == paths[0].vertex)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                paths[i].vertex.name = "Path_" + (i + 1);
            }
            if (joints.Count > 0)
            {
                roadJoint.setTangent(1, (joints[0].vertex - point).normalized * 2.5f);
            }
            joints.Insert(0, roadJoint);
            Transform transform2 = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Edit/Path"))).transform;
            transform2.name = "Path_0";
            transform2.parent = line;
            RoadPath roadPath2 = new RoadPath(transform2);
            paths.Insert(0, roadPath2);
            updatePoints();
            return roadPath2.vertex;
        }
        return null;
    }

    public Transform addVertex(int vertexIndex, Vector3 point)
    {
        RoadJoint roadJoint = new RoadJoint(point);
        for (int i = vertexIndex; i < joints.Count; i++)
        {
            paths[i].vertex.name = "Path_" + (i + 1);
        }
        if (joints.Count == 1)
        {
            joints[0].setTangent(1, (point - joints[0].vertex).normalized * 2.5f);
            roadJoint.setTangent(0, (joints[0].vertex - point).normalized * 2.5f);
        }
        else if (joints.Count > 1)
        {
            if (vertexIndex == 0)
            {
                if (isLoop)
                {
                    RoadJoint roadJoint2 = joints[joints.Count - 1];
                    RoadJoint roadJoint3 = joints[0];
                    roadJoint.setTangent(1, (roadJoint3.vertex - roadJoint2.vertex).normalized * 2.5f);
                }
                else
                {
                    roadJoint.setTangent(1, (joints[0].vertex - point).normalized * 2.5f);
                }
            }
            else if (vertexIndex == joints.Count)
            {
                if (isLoop)
                {
                    RoadJoint roadJoint4 = joints[joints.Count - 1];
                    RoadJoint roadJoint5 = joints[0];
                    roadJoint.setTangent(1, (roadJoint5.vertex - roadJoint4.vertex).normalized * 2.5f);
                }
                else
                {
                    roadJoint.setTangent(0, (joints[joints.Count - 1].vertex - point).normalized * 2.5f);
                }
            }
            else
            {
                RoadJoint roadJoint6 = joints[vertexIndex - 1];
                RoadJoint roadJoint7 = joints[vertexIndex];
                roadJoint.setTangent(1, (roadJoint7.vertex - roadJoint6.vertex).normalized * 2.5f);
            }
        }
        joints.Insert(vertexIndex, roadJoint);
        Transform transform = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Edit/Path"))).transform;
        transform.name = "Path_" + vertexIndex;
        transform.parent = line;
        RoadPath roadPath = new RoadPath(transform);
        paths.Insert(vertexIndex, roadPath);
        updatePoints();
        return roadPath.vertex;
    }

    [Obsolete]
    public void removePoint(Transform select)
    {
        if (joints.Count < 2)
        {
            LevelRoads.removeRoad(this);
            return;
        }
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].vertex == select)
            {
                for (int j = i + 1; j < paths.Count; j++)
                {
                    paths[j].vertex.name = "Path_" + (j - 1);
                }
                UnityEngine.Object.Destroy(select.gameObject);
                joints.RemoveAt(i);
                paths.RemoveAt(i);
                updatePoints();
                break;
            }
        }
    }

    public void removeVertex(int vertexIndex)
    {
        if (joints.Count < 2)
        {
            LevelRoads.removeRoad(this);
            return;
        }
        for (int i = vertexIndex + 1; i < paths.Count; i++)
        {
            paths[i].vertex.name = "Path_" + (i - 1);
        }
        paths[vertexIndex].remove();
        paths.RemoveAt(vertexIndex);
        joints.RemoveAt(vertexIndex);
        updatePoints();
    }

    public void remove()
    {
        UnityEngine.Object.Destroy(road.gameObject);
        UnityEngine.Object.Destroy(line.gameObject);
    }

    [Obsolete]
    public void movePoint(Transform select, Vector3 point)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].vertex == select)
            {
                joints[i].vertex = point;
                updatePoints();
                break;
            }
        }
    }

    public void moveVertex(int vertexIndex, Vector3 point)
    {
        joints[vertexIndex].vertex = point;
        updatePoints();
    }

    public void moveTangent(int vertexIndex, int tangentIndex, Vector3 point)
    {
        joints[vertexIndex].setTangent(tangentIndex, point);
        updatePoints();
    }

    public void buildMesh()
    {
        for (int i = 0; i < road.childCount; i++)
        {
            UnityEngine.Object.Destroy(road.GetChild(i).gameObject);
        }
        if (joints.Count < 2)
        {
            return;
        }
        updateSamples();
        if (!Level.isEditor)
        {
            bool flag = false;
            foreach (LevelTrainAssociation train in Level.info.configData.Trains)
            {
                if (train.RoadIndex == roadIndex)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                updateTrackSamples();
            }
        }
        Vector3[] array = new Vector3[samples.Count * 4 + ((!isLoop) ? 8 : 0)];
        Vector3[] array2 = new Vector3[samples.Count * 4 + ((!isLoop) ? 8 : 0)];
        Vector2[] array3 = (Dedicator.IsDedicatedServer ? null : new Vector2[samples.Count * 4 + ((!isLoop) ? 8 : 0)]);
        float num = 0f;
        Vector3 vector = Vector3.zero;
        Vector3 vector2 = Vector3.zero;
        Vector3 vector3 = Vector3.zero;
        Vector3 vector4 = Vector3.zero;
        Vector3 vector5 = Vector3.zero;
        _ = Vector2.zero;
        int j;
        for (j = 0; j < samples.Count; j++)
        {
            RoadSample roadSample = samples[j];
            RoadJoint roadJoint = joints[roadSample.index];
            vector2 = getPosition(roadSample.index, roadSample.time);
            if (!roadJoint.ignoreTerrain)
            {
                vector2.y = LevelGround.getHeight(vector2);
            }
            vector3 = getVelocity(roadSample.index, roadSample.time).normalized;
            vector4 = ((!roadJoint.ignoreTerrain) ? LevelGround.getNormal(vector2) : Vector3.up);
            vector5 = Vector3.Cross(vector3, vector4).normalized;
            if (!roadJoint.ignoreTerrain)
            {
                Vector3 point = vector2 + vector5 * LevelRoads.materials[material].width;
                float num2 = LevelGround.getHeight(point) - point.y;
                if (num2 > 0f)
                {
                    vector2.y += num2;
                }
                Vector3 point2 = vector2 - vector5 * LevelRoads.materials[material].width;
                float num3 = LevelGround.getHeight(point2) - point2.y;
                if (num3 > 0f)
                {
                    vector2.y += num3;
                }
            }
            if (roadSample.index < joints.Count - 1)
            {
                vector2.y += Mathf.Lerp(roadJoint.offset, joints[roadSample.index + 1].offset, roadSample.time);
            }
            else if (isLoop)
            {
                vector2.y += Mathf.Lerp(roadJoint.offset, joints[0].offset, roadSample.time);
            }
            else
            {
                vector2.y += roadJoint.offset;
            }
            array[((!isLoop) ? 4 : 0) + j * 4] = vector2 + vector5 * (LevelRoads.materials[material].width + LevelRoads.materials[material].depth * 2f) - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset;
            array[((!isLoop) ? 4 : 0) + j * 4 + 1] = vector2 + vector5 * LevelRoads.materials[material].width + vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset;
            array[((!isLoop) ? 4 : 0) + j * 4 + 2] = vector2 - vector5 * LevelRoads.materials[material].width + vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset;
            array[((!isLoop) ? 4 : 0) + j * 4 + 3] = vector2 - vector5 * (LevelRoads.materials[material].width + LevelRoads.materials[material].depth * 2f) - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset;
            array2[((!isLoop) ? 4 : 0) + j * 4] = vector4;
            array2[((!isLoop) ? 4 : 0) + j * 4 + 1] = vector4;
            array2[((!isLoop) ? 4 : 0) + j * 4 + 2] = vector4;
            array2[((!isLoop) ? 4 : 0) + j * 4 + 3] = vector4;
            if (j == 0)
            {
                if (!isLoop)
                {
                    array[j * 4] = vector2 + vector5 * (LevelRoads.materials[material].width + LevelRoads.materials[material].depth * 2f) - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset - vector3 * LevelRoads.materials[material].depth * 4f;
                    array[j * 4 + 1] = vector2 + vector5 * LevelRoads.materials[material].width - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset - vector3 * LevelRoads.materials[material].depth * 4f;
                    array[j * 4 + 2] = vector2 - vector5 * LevelRoads.materials[material].width - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset - vector3 * LevelRoads.materials[material].depth * 4f;
                    array[j * 4 + 3] = vector2 - vector5 * (LevelRoads.materials[material].width + LevelRoads.materials[material].depth * 2f) - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset - vector3 * LevelRoads.materials[material].depth * 4f;
                    array2[j * 4] = vector4;
                    array2[j * 4 + 1] = vector4;
                    array2[j * 4 + 2] = vector4;
                    array2[j * 4 + 3] = vector4;
                    if (!Dedicator.IsDedicatedServer)
                    {
                        array3[j * 4] = Vector2.zero;
                        array3[j * 4 + 1] = Vector2.zero;
                        array3[j * 4 + 2] = Vector2.right;
                        array3[j * 4 + 3] = Vector2.right;
                    }
                }
                vector = vector2;
                if (!Dedicator.IsDedicatedServer)
                {
                    array3[((!isLoop) ? 4 : 0) + j * 4] = Vector2.zero;
                    array3[((!isLoop) ? 4 : 0) + j * 4 + 1] = Vector2.zero;
                    array3[((!isLoop) ? 4 : 0) + j * 4 + 2] = Vector2.right;
                    array3[((!isLoop) ? 4 : 0) + j * 4 + 3] = Vector2.right;
                }
            }
            else
            {
                num += (vector2 - vector).magnitude;
                vector = vector2;
                if (!Dedicator.IsDedicatedServer)
                {
                    Vector2 vector6 = Vector2.up * num / LevelRoads.materials[material].material.mainTexture.height * LevelRoads.materials[material].height;
                    array3[((!isLoop) ? 4 : 0) + j * 4] = Vector2.zero + vector6;
                    array3[((!isLoop) ? 4 : 0) + j * 4 + 1] = Vector2.zero + vector6;
                    array3[((!isLoop) ? 4 : 0) + j * 4 + 2] = Vector2.right + vector6;
                    array3[((!isLoop) ? 4 : 0) + j * 4 + 3] = Vector2.right + vector6;
                }
            }
        }
        if (!isLoop)
        {
            array[4 + j * 4] = vector2 + vector5 * (LevelRoads.materials[material].width + LevelRoads.materials[material].depth * 2f) - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset + vector3 * LevelRoads.materials[material].depth * 4f;
            array[4 + j * 4 + 1] = vector2 + vector5 * LevelRoads.materials[material].width - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset + vector3 * LevelRoads.materials[material].depth * 4f;
            array[4 + j * 4 + 2] = vector2 - vector5 * LevelRoads.materials[material].width - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset + vector3 * LevelRoads.materials[material].depth * 4f;
            array[4 + j * 4 + 3] = vector2 - vector5 * (LevelRoads.materials[material].width + LevelRoads.materials[material].depth * 2f) - vector4 * LevelRoads.materials[material].depth + vector4 * LevelRoads.materials[material].offset + vector3 * LevelRoads.materials[material].depth * 4f;
            array2[4 + j * 4] = vector4;
            array2[4 + j * 4 + 1] = vector4;
            array2[4 + j * 4 + 2] = vector4;
            array2[4 + j * 4 + 3] = vector4;
            if (!Dedicator.IsDedicatedServer)
            {
                Vector2 vector6 = Vector2.up * num / LevelRoads.materials[material].material.mainTexture.height * LevelRoads.materials[material].height;
                array3[4 + j * 4] = Vector2.zero + vector6;
                array3[4 + j * 4 + 1] = Vector2.zero + vector6;
                array3[4 + j * 4 + 2] = Vector2.right + vector6;
                array3[4 + j * 4 + 3] = Vector2.right + vector6;
            }
        }
        int num4 = 0;
        for (int k = 0; k < samples.Count; k += 20)
        {
            int num5 = Mathf.Min(k + 20, samples.Count - 1);
            int num6 = num5 - k + 1;
            if (!isLoop)
            {
                if (k == 0)
                {
                    num6++;
                }
                if (num5 == samples.Count - 1)
                {
                    num6++;
                }
            }
            Vector3[] array4 = new Vector3[num6 * 4];
            Vector3[] array5 = new Vector3[num6 * 4];
            Vector2[] array6 = (Dedicator.IsDedicatedServer ? null : new Vector2[num6 * 4]);
            int[] array7 = new int[num6 * 18];
            int num7 = k;
            if (!isLoop && k > 0)
            {
                num7++;
            }
            Array.Copy(array, num7 * 4, array4, 0, array4.Length);
            Array.Copy(array2, num7 * 4, array5, 0, array4.Length);
            if (!Dedicator.IsDedicatedServer)
            {
                Array.Copy(array3, num7 * 4, array6, 0, array4.Length);
            }
            for (int l = 0; l < num6 - 1; l++)
            {
                array7[l * 18] = l * 4 + 5;
                array7[l * 18 + 1] = l * 4 + 1;
                array7[l * 18 + 2] = l * 4 + 4;
                array7[l * 18 + 3] = l * 4;
                array7[l * 18 + 4] = l * 4 + 4;
                array7[l * 18 + 5] = l * 4 + 1;
                array7[l * 18 + 6] = l * 4 + 6;
                array7[l * 18 + 7] = l * 4 + 2;
                array7[l * 18 + 8] = l * 4 + 5;
                array7[l * 18 + 9] = l * 4 + 1;
                array7[l * 18 + 10] = l * 4 + 5;
                array7[l * 18 + 11] = l * 4 + 2;
                array7[l * 18 + 12] = l * 4 + 7;
                array7[l * 18 + 13] = l * 4 + 3;
                array7[l * 18 + 14] = l * 4 + 6;
                array7[l * 18 + 15] = l * 4 + 2;
                array7[l * 18 + 16] = l * 4 + 6;
                array7[l * 18 + 17] = l * 4 + 3;
            }
            Transform transform = new GameObject().transform;
            transform.name = "Segment_" + num4;
            transform.parent = road;
            transform.tag = "Environment";
            transform.gameObject.layer = 19;
            transform.gameObject.AddComponent<MeshCollider>();
            if (!Dedicator.IsDedicatedServer)
            {
                transform.gameObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = transform.gameObject.AddComponent<MeshRenderer>();
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Simple;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            if (LevelRoads.materials[material].isConcrete)
            {
                transform.GetComponent<Collider>().sharedMaterial = (PhysicMaterial)Resources.Load("Physics/Concrete_Static");
            }
            else
            {
                transform.GetComponent<Collider>().sharedMaterial = (PhysicMaterial)Resources.Load("Physics/Gravel_Static");
            }
            Mesh mesh = new Mesh();
            mesh.name = "Road_Segment_" + num4;
            mesh.vertices = array4;
            mesh.normals = array5;
            mesh.uv = array6;
            mesh.triangles = array7;
            transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            if (!Dedicator.IsDedicatedServer)
            {
                transform.GetComponent<MeshFilter>().sharedMesh = mesh;
                transform.GetComponent<Renderer>().sharedMaterial = LevelRoads.materials[material].material;
            }
            num4++;
        }
    }

    private void updateSamples()
    {
        samples.Clear();
        float num = 0f;
        for (int i = 0; i < joints.Count - 1 + (isLoop ? 1 : 0); i++)
        {
            float lengthEstimate = getLengthEstimate(i);
            float num2;
            for (num2 = num; num2 < lengthEstimate; num2 += 5f)
            {
                float time = num2 / lengthEstimate;
                RoadSample roadSample = new RoadSample();
                roadSample.index = i;
                roadSample.time = time;
                samples.Add(roadSample);
            }
            num = num2 - lengthEstimate;
        }
        if (isLoop)
        {
            RoadSample roadSample2 = new RoadSample();
            roadSample2.index = 0;
            roadSample2.time = 0f;
            samples.Add(roadSample2);
        }
        else
        {
            RoadSample roadSample3 = new RoadSample();
            roadSample3.index = joints.Count - 2;
            roadSample3.time = 1f;
            samples.Add(roadSample3);
        }
    }

    private void updateTrackSamples()
    {
        trackSamples.Clear();
        if (samples.Count < 2)
        {
            return;
        }
        Vector3 position = Vector3.zero;
        Vector3 normal = Vector3.up;
        double num = 0.0;
        int num2 = (isLoop ? (samples.Count - 1) : samples.Count);
        for (int i = 1; i < num2; i++)
        {
            RoadSample roadSample = samples[i];
            TrackSample trackSample = null;
            if (i == 1)
            {
                RoadSample roadSample2 = samples[0];
                getTrackPosition(roadSample2.index, roadSample2.time, out position, out normal);
                trackSample = new TrackSample();
                trackSample.position = position;
                trackSample.normal = normal;
                trackSamples.Add(trackSample);
            }
            getTrackPosition(roadSample.index, roadSample.time, out var position2, out var normal2);
            Vector3 vector = position2 - position;
            float magnitude = vector.magnitude;
            Vector3 direction = vector / magnitude;
            TrackSample trackSample2 = new TrackSample();
            trackSample2.distance = (float)num;
            trackSample2.position = position2;
            trackSample2.normal = normal2;
            trackSample2.direction = direction;
            trackSamples.Add(trackSample2);
            if (trackSample != null)
            {
                trackSample.direction = direction;
            }
            position = position2;
            num += (double)magnitude;
        }
        if (isLoop)
        {
            num += (double)(trackSamples[0].position - position).magnitude;
        }
        trackSampledLength = (float)num;
    }

    public void updatePoints()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            RoadJoint roadJoint = joints[i];
            if (!roadJoint.ignoreTerrain)
            {
                roadJoint.vertex.y = LevelGround.getHeight(roadJoint.vertex);
            }
        }
        for (int j = 0; j < joints.Count; j++)
        {
            RoadPath roadPath = paths[j];
            roadPath.vertex.position = joints[j].vertex;
            roadPath.tangents[0].gameObject.SetActive(j > 0 || isLoop);
            roadPath.tangents[1].gameObject.SetActive(j < joints.Count - 1 || isLoop);
            roadPath.setTangent(0, joints[j].getTangent(0));
            roadPath.setTangent(1, joints[j].getTangent(1));
        }
        if (joints.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        updateSamples();
        lineRenderer.positionCount = samples.Count;
        for (int k = 0; k < samples.Count; k++)
        {
            RoadSample roadSample = samples[k];
            RoadJoint roadJoint2 = joints[roadSample.index];
            Vector3 position = getPosition(roadSample.index, roadSample.time);
            if (!roadJoint2.ignoreTerrain)
            {
                position.y = LevelGround.getHeight(position);
            }
            if (roadSample.index < joints.Count - 1)
            {
                position.y += Mathf.Lerp(roadJoint2.offset, joints[roadSample.index + 1].offset, roadSample.time);
            }
            else if (isLoop)
            {
                position.y += Mathf.Lerp(roadJoint2.offset, joints[0].offset, roadSample.time);
            }
            else
            {
                position.y += roadJoint2.offset;
            }
            lineRenderer.SetPosition(k, position);
        }
    }

    public Road(byte newMaterial, ushort newRoadIndex)
        : this(newMaterial, newRoadIndex, newLoop: false, new List<RoadJoint>())
    {
    }

    public Road(byte newMaterial, ushort newRoadIndex, bool newLoop, List<RoadJoint> newJoints)
    {
        material = newMaterial;
        roadIndex = newRoadIndex;
        _road = new GameObject().transform;
        road.name = "Road";
        road.tag = "Environment";
        road.gameObject.layer = 19;
        _isLoop = newLoop;
        _joints = newJoints;
        samples = new List<RoadSample>();
        trackSamples = new List<TrackSample>();
        if (Level.isEditor)
        {
            line = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Edit/Road"))).transform;
            line.name = "Line";
            _paths = new List<RoadPath>();
            lineRenderer = line.GetComponent<LineRenderer>();
            for (int i = 0; i < joints.Count; i++)
            {
                Transform transform = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Edit/Path"))).transform;
                transform.name = "Path_" + i;
                transform.parent = line;
                RoadPath item = new RoadPath(transform);
                paths.Add(item);
            }
        }
    }
}
