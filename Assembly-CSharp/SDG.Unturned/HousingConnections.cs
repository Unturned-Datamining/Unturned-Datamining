using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class HousingConnections
{
    public const float EDGE_LENGTH = 6f;

    public const float HALF_EDGE_LENGTH = 3f;

    public const float WALL_HEIGHT = 4.25f;

    public const float HALF_WALL_HEIGHT = 2.125f;

    public const float WALL_PIVOT_OFFSET = 2.125f;

    public const float RAMPART_PIVOT_OFFSET = 0.9f;

    private const float FOUNDATION_HEIGHT = 10.25f;

    private const float HALF_FOUNDATION_HEIGHT = 5.125f;

    private const float FOUNDATION_CENTER_OFFSET = -4.875f;

    private const float LINK_TOLERANCE = 0.02f;

    internal const float MAX_PLACEMENT_DISTANCE = 16f;

    internal const float MAX_PLACEMENT_SQR_DISTANCE = 256f;

    private const float MAX_FIND_EMPTY_SLOT_DISTANCE = 8f;

    private const float MAX_FIND_EMPTY_SLOT_SQR_DISTANCE = 64f;

    private const float MIN_FIND_EMPTY_SLOT_COSINE = 0.9f;

    private const float PLACEMENT_OVERLAP_PADDING = 0.02f;

    private const float CHARACTER_OVERLAP_PADDING = 0.25f;

    private const float HALF_CHARACTER_OVERLAP_PADDING = 0.125f;

    private const float TRIANGLE_APEX_PIVOT_OFFSET = 2.1961524f;

    private const float TRIANGLE_INNER_RADIUS = 1.7320508f;

    internal const float TRIANGLE_CENTER_PIVOT_OFFSET = -1.2679492f;

    private const float FOUNDATION_TOP_MARGIN = 0.1f;

    private const float HALF_FOUNDATION_TOP_MARGIN = 0.05f;

    private const float ROOF_THICKNESS = 0.5f;

    private const float HALF_ROOF_THICKNESS = 0.25f;

    private const int HOUSE_OVERLAP_LAYER_MASK = 402653184;

    private const int CHARACTER_OVERLAP_LAYER_MASK = 83887616;

    private readonly Vector3 leftLocalDirection = new Vector3(0.8660254f, -0.5f, 0f);

    private readonly Vector3 rightLocalDirection = new Vector3(-0.8660254f, -0.5f, 0f);

    private RegionList<HousingEdge> edgesGrid;

    private RegionList<HousingVertex> verticesGrid;

    private Collider[] overlapBuffer = new Collider[50];

    private HashSet<StructureDrop> ignoreDrops = new HashSet<StructureDrop>();

    public HousingConnections()
    {
        edgesGrid = new RegionList<HousingEdge>();
        verticesGrid = new RegionList<HousingVertex>();
    }

    internal void LinkConnections(StructureDrop drop)
    {
        switch (drop.asset.construct)
        {
        case EConstruct.FLOOR:
            LinkSquareFloor(drop);
            break;
        case EConstruct.WALL:
            LinkWall(drop, -2.125f);
            break;
        case EConstruct.RAMPART:
            LinkWall(drop, -0.9f);
            break;
        case EConstruct.ROOF:
            LinkSquareFloor(drop);
            break;
        case EConstruct.PILLAR:
            LinkPillar(drop, drop.model.position + new Vector3(0f, -2.125f, 0f));
            break;
        case EConstruct.POST:
            LinkPillar(drop, drop.model.position + new Vector3(0f, -0.9f, 0f));
            break;
        case EConstruct.FLOOR_POLY:
            LinkTriangularFloor(drop);
            break;
        case EConstruct.ROOF_POLY:
            LinkTriangularFloor(drop);
            break;
        default:
            UnturnedLog.error($"Link housing connection unhandled: {drop.asset.construct}");
            break;
        }
    }

    internal void UnlinkConnections(StructureDrop drop)
    {
        switch (drop.asset.construct)
        {
        case EConstruct.FLOOR:
            UnlinkSquareFloor(drop);
            break;
        case EConstruct.WALL:
        case EConstruct.RAMPART:
            UnlinkWall(drop);
            break;
        case EConstruct.ROOF:
            UnlinkSquareFloor(drop);
            break;
        case EConstruct.PILLAR:
        case EConstruct.POST:
            UnlinkPillar(drop);
            break;
        case EConstruct.FLOOR_POLY:
            UnlinkTriangleFloor(drop);
            break;
        case EConstruct.ROOF_POLY:
            UnlinkTriangleFloor(drop);
            break;
        default:
            UnturnedLog.error($"Unlink housing connection unhandled: {drop.asset.construct}");
            break;
        }
    }

    private HousingVertex FindVertex(Vector3 position)
    {
        foreach (HousingVertex item in verticesGrid.EnumerateItemsInSquare(position, 0.02f))
        {
            if (item.position.IsNearlyEqual(position, 0.02f))
            {
                return item;
            }
        }
        return null;
    }

    private HousingEdge FindEdge(Vector3 position)
    {
        foreach (HousingEdge item in edgesGrid.EnumerateItemsInSquare(position, 0.02f))
        {
            if (item.position.IsNearlyEqual(position, 0.02f))
            {
                return item;
            }
        }
        return null;
    }

    private void RemoveVertex(HousingVertex vertex)
    {
        foreach (HousingEdge edge in vertex.edges)
        {
            if (edge.vertex0 == vertex)
            {
                edge.vertex0 = null;
            }
            else if (edge.vertex1 == vertex)
            {
                edge.vertex1 = null;
            }
        }
        vertex.edges.Clear();
        if (vertex.upperVertex != null)
        {
            vertex.upperVertex.lowerVertex = null;
            vertex.upperVertex = null;
        }
        if (vertex.lowerVertex != null)
        {
            vertex.lowerVertex.upperVertex = null;
            vertex.lowerVertex = null;
        }
        verticesGrid.RemoveFast(vertex.position, vertex, 0.02f);
    }

    private void RemoveEdge(HousingEdge edge)
    {
        if (edge.vertex0 != null)
        {
            edge.vertex0.edges.RemoveFast(edge);
            edge.vertex0 = null;
        }
        if (edge.vertex1 != null)
        {
            edge.vertex1.edges.RemoveFast(edge);
            edge.vertex1 = null;
        }
        if (edge.upperEdge != null)
        {
            edge.upperEdge.lowerEdge = null;
            edge.upperEdge = null;
        }
        if (edge.lowerEdge != null)
        {
            edge.lowerEdge.upperEdge = null;
            edge.lowerEdge = null;
        }
        edgesGrid.RemoveFast(edge.position, edge, 0.02f);
    }

    private Vector3 GetFloorCenter(StructureDrop floor)
    {
        if (floor.asset.construct != EConstruct.FLOOR_POLY && floor.asset.construct != EConstruct.ROOF_POLY)
        {
            return floor.model.position;
        }
        return floor.model.TransformPoint(0f, 1.2679492f, 0f);
    }

    private HousingEdge LinkSquareEdge(StructureDrop drop, Vector3 direction, float rotation)
    {
        Vector3 edgePosition = drop.model.position + direction * 3f;
        return LinkFloorEdge(drop, edgePosition, direction, rotation);
    }

    private HousingEdge LinkFloorEdge(StructureDrop floor, Vector3 edgePosition, Vector3 direction, float rotation)
    {
        HousingEdge housingEdge = FindEdge(edgePosition);
        if (housingEdge != null)
        {
            if (housingEdge.forwardFloors.IsEmpty() && housingEdge.backwardFloors.IsEmpty())
            {
                housingEdge.direction = direction;
                housingEdge.rotation = floor.model.rotation.eulerAngles.y + rotation;
                housingEdge.backwardFloors.Add(floor);
            }
            else if (Vector3.Dot(direction, housingEdge.direction) > 0f)
            {
                housingEdge.backwardFloors.Add(floor);
            }
            else
            {
                housingEdge.forwardFloors.Add(floor);
            }
        }
        else
        {
            housingEdge = new HousingEdge();
            housingEdge.position = edgePosition;
            housingEdge.direction = direction;
            housingEdge.rotation = floor.model.rotation.eulerAngles.y + rotation;
            housingEdge.forwardFloors = new List<StructureDrop>(1);
            housingEdge.backwardFloors = new List<StructureDrop>(1) { floor };
            housingEdge.walls = new List<StructureDrop>(1);
            edgesGrid.Add(edgePosition, housingEdge);
        }
        return housingEdge;
    }

    private HousingVertex LinkFloorVertex(StructureDrop floor, Vector3 vertexPosition)
    {
        HousingVertex housingVertex = FindVertex(vertexPosition);
        if (housingVertex != null)
        {
            if (housingVertex.floors.Count < 1)
            {
                housingVertex.rotation = floor.model.rotation.eulerAngles.y;
            }
            housingVertex.floors.Add(floor);
        }
        else
        {
            housingVertex = new HousingVertex();
            housingVertex.position = vertexPosition;
            housingVertex.rotation = floor.model.rotation.eulerAngles.y;
            housingVertex.floors.Add(floor);
            verticesGrid.Add(vertexPosition, housingVertex);
        }
        return housingVertex;
    }

    private void UnlinkFloorEdge(StructureDrop floor, HousingEdge edge)
    {
        if (!edge.forwardFloors.RemoveFast(floor))
        {
            edge.backwardFloors.RemoveFast(floor);
        }
        if (edge.ShouldBeRemoved)
        {
            RemoveEdge(edge);
        }
    }

    private void UnlinkFloorVertex(StructureDrop floor, HousingVertex vertex)
    {
        vertex.floors.RemoveFast(floor);
        if (vertex.ShouldBeRemoved)
        {
            RemoveVertex(vertex);
        }
    }

    private void UnlinkSquareFloor(StructureDrop floor)
    {
        HousingSquareFloorConnections housingSquareFloorConnections = (HousingSquareFloorConnections)floor.housingConnectionData;
        floor.housingConnectionData = null;
        UnlinkFloorEdge(floor, housingSquareFloorConnections.edge0);
        UnlinkFloorEdge(floor, housingSquareFloorConnections.edge1);
        UnlinkFloorEdge(floor, housingSquareFloorConnections.edge2);
        UnlinkFloorEdge(floor, housingSquareFloorConnections.edge3);
        UnlinkFloorVertex(floor, housingSquareFloorConnections.vertex0);
        UnlinkFloorVertex(floor, housingSquareFloorConnections.vertex1);
        UnlinkFloorVertex(floor, housingSquareFloorConnections.vertex2);
        UnlinkFloorVertex(floor, housingSquareFloorConnections.vertex3);
    }

    private void UnlinkTriangleFloor(StructureDrop floor)
    {
        HousingTriangleFloorConnections housingTriangleFloorConnections = (HousingTriangleFloorConnections)floor.housingConnectionData;
        floor.housingConnectionData = null;
        UnlinkFloorEdge(floor, housingTriangleFloorConnections.edge0);
        UnlinkFloorEdge(floor, housingTriangleFloorConnections.edge1);
        UnlinkFloorEdge(floor, housingTriangleFloorConnections.edge2);
        UnlinkFloorVertex(floor, housingTriangleFloorConnections.vertex0);
        UnlinkFloorVertex(floor, housingTriangleFloorConnections.vertex1);
        UnlinkFloorVertex(floor, housingTriangleFloorConnections.vertex2);
    }

    private void LinkEdgeWithVertices(HousingEdge edge, HousingVertex vertex0, HousingVertex vertex1)
    {
        if (edge.vertex0 != vertex0 && edge.vertex1 != vertex0)
        {
            if (edge.vertex0 == null)
            {
                edge.vertex0 = vertex0;
            }
            else
            {
                edge.vertex1 = vertex0;
            }
            vertex0.edges.Add(edge);
        }
        if (edge.vertex0 != vertex1 && edge.vertex1 != vertex1)
        {
            if (edge.vertex0 == null)
            {
                edge.vertex0 = vertex1;
            }
            else
            {
                edge.vertex1 = vertex1;
            }
            vertex1.edges.Add(edge);
        }
    }

    private void LinkSquareFloor(StructureDrop drop)
    {
        HousingSquareFloorConnections housingSquareFloorConnections = (HousingSquareFloorConnections)(drop.housingConnectionData = new HousingSquareFloorConnections());
        housingSquareFloorConnections.edge0 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(1f, 0f, 0f)), 270f);
        housingSquareFloorConnections.edge1 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(-1f, 0f, 0f)), 90f);
        housingSquareFloorConnections.edge2 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(0f, 1f, 0f)), 0f);
        housingSquareFloorConnections.edge3 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(0f, -1f, 0f)), 180f);
        housingSquareFloorConnections.vertex0 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(3f, 3f, 0f)));
        housingSquareFloorConnections.vertex1 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(-3f, 3f, 0f)));
        housingSquareFloorConnections.vertex2 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(3f, -3f, 0f)));
        housingSquareFloorConnections.vertex3 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(-3f, -3f, 0f)));
        LinkEdgeWithVertices(housingSquareFloorConnections.edge0, housingSquareFloorConnections.vertex0, housingSquareFloorConnections.vertex2);
        LinkEdgeWithVertices(housingSquareFloorConnections.edge1, housingSquareFloorConnections.vertex1, housingSquareFloorConnections.vertex3);
        LinkEdgeWithVertices(housingSquareFloorConnections.edge2, housingSquareFloorConnections.vertex0, housingSquareFloorConnections.vertex1);
        LinkEdgeWithVertices(housingSquareFloorConnections.edge3, housingSquareFloorConnections.vertex2, housingSquareFloorConnections.vertex3);
    }

    private void LinkTriangularFloor(StructureDrop drop)
    {
        HousingTriangleFloorConnections housingTriangleFloorConnections = (HousingTriangleFloorConnections)(drop.housingConnectionData = new HousingTriangleFloorConnections());
        Vector3 vector = drop.model.TransformDirection(new Vector3(0f, 1f, 0f));
        housingTriangleFloorConnections.edge0 = LinkFloorEdge(drop, drop.model.position + vector * 3f, vector, 0f);
        Vector3 edgePosition = drop.model.TransformPoint(new Vector3(1.5f, 0.40192378f, 0f));
        Vector3 direction = drop.model.TransformDirection(leftLocalDirection);
        housingTriangleFloorConnections.edge1 = LinkFloorEdge(drop, edgePosition, direction, 240f);
        Vector3 edgePosition2 = drop.model.TransformPoint(new Vector3(-1.5f, 0.40192378f, 0f));
        Vector3 direction2 = drop.model.TransformDirection(rightLocalDirection);
        housingTriangleFloorConnections.edge2 = LinkFloorEdge(drop, edgePosition2, direction2, 120f);
        housingTriangleFloorConnections.vertex0 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(3f, 3f, 0f)));
        housingTriangleFloorConnections.vertex1 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(-3f, 3f, 0f)));
        housingTriangleFloorConnections.vertex2 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(0f, -2.1961524f, 0f)));
        LinkEdgeWithVertices(housingTriangleFloorConnections.edge0, housingTriangleFloorConnections.vertex0, housingTriangleFloorConnections.vertex1);
        LinkEdgeWithVertices(housingTriangleFloorConnections.edge1, housingTriangleFloorConnections.vertex0, housingTriangleFloorConnections.vertex2);
        LinkEdgeWithVertices(housingTriangleFloorConnections.edge2, housingTriangleFloorConnections.vertex1, housingTriangleFloorConnections.vertex2);
    }

    private void LinkWall(StructureDrop wall, float pivotOffset)
    {
        Vector3 vector = wall.model.position + new Vector3(0f, pivotOffset, 0f);
        Vector3 position = vector + new Vector3(0f, 4.25f, 0f);
        HousingEdge housingEdge = FindEdge(vector);
        HousingEdge housingEdge2 = FindEdge(position);
        HousingEdge housingEdge3 = housingEdge;
        if (housingEdge3 == null)
        {
            Vector3 position2 = wall.model.TransformPoint(3f, 0f, 0f) + new Vector3(0f, pivotOffset, 0f);
            Vector3 position3 = wall.model.TransformPoint(-3f, 0f, 0f) + new Vector3(0f, pivotOffset, 0f);
            HousingVertex housingVertex = FindVertex(position2);
            if (housingVertex == null)
            {
                housingVertex = new HousingVertex();
                housingVertex.position = position2;
                housingVertex.rotation = wall.model.rotation.eulerAngles.y;
                verticesGrid.Add(position2, housingVertex);
            }
            HousingVertex housingVertex2 = FindVertex(position3);
            if (housingVertex2 == null)
            {
                housingVertex2 = new HousingVertex();
                housingVertex2.position = position3;
                housingVertex2.rotation = wall.model.rotation.eulerAngles.y;
                verticesGrid.Add(position3, housingVertex2);
            }
            housingEdge3 = new HousingEdge();
            housingEdge3.position = vector;
            housingEdge3.direction = wall.model.TransformDirection(0f, 1f, 0f);
            housingEdge3.rotation = wall.model.rotation.eulerAngles.y;
            housingEdge3.walls = new List<StructureDrop>(1);
            housingEdge3.forwardFloors = new List<StructureDrop>(1);
            housingEdge3.backwardFloors = new List<StructureDrop>(1);
            housingEdge3.vertex0 = housingVertex;
            housingVertex.edges.Add(housingEdge3);
            housingEdge3.vertex1 = housingVertex2;
            housingVertex2.edges.Add(housingEdge3);
            edgesGrid.Add(vector, housingEdge3);
        }
        HousingEdge housingEdge4 = housingEdge2;
        if (housingEdge4 == null)
        {
            Vector3 position4 = wall.model.TransformPoint(3f, 0f, 0f) + new Vector3(0f, pivotOffset + 4.25f, 0f);
            Vector3 position5 = wall.model.TransformPoint(-3f, 0f, 0f) + new Vector3(0f, pivotOffset + 4.25f, 0f);
            HousingVertex housingVertex3 = FindVertex(position4);
            if (housingVertex3 == null)
            {
                housingVertex3 = new HousingVertex();
                housingVertex3.position = position4;
                housingVertex3.rotation = wall.model.rotation.eulerAngles.y;
                verticesGrid.Add(position4, housingVertex3);
            }
            HousingVertex housingVertex4 = FindVertex(position5);
            if (housingVertex4 == null)
            {
                housingVertex4 = new HousingVertex();
                housingVertex4.position = position5;
                housingVertex4.rotation = wall.model.rotation.eulerAngles.y;
                verticesGrid.Add(position5, housingVertex4);
            }
            housingEdge4 = new HousingEdge();
            housingEdge4.position = position;
            housingEdge4.direction = wall.model.TransformDirection(0f, 1f, 0f);
            housingEdge4.rotation = wall.model.rotation.eulerAngles.y;
            housingEdge4.walls = new List<StructureDrop>(1);
            housingEdge4.forwardFloors = new List<StructureDrop>(1);
            housingEdge4.backwardFloors = new List<StructureDrop>(1);
            housingEdge4.vertex0 = housingVertex3;
            housingVertex3.edges.Add(housingEdge4);
            housingEdge4.vertex1 = housingVertex4;
            housingVertex4.edges.Add(housingEdge4);
            edgesGrid.Add(position, housingEdge4);
        }
        housingEdge4.lowerEdge = housingEdge3;
        housingEdge3.upperEdge = housingEdge4;
        HousingWallConnections housingWallConnections = (HousingWallConnections)(wall.housingConnectionData = new HousingWallConnections());
        housingWallConnections.lowerEdge = housingEdge3;
        housingWallConnections.upperEdge = housingEdge4;
        housingEdge3.walls.Add(wall);
    }

    private void UnlinkWall(StructureDrop wall)
    {
        HousingWallConnections housingWallConnections = (HousingWallConnections)wall.housingConnectionData;
        wall.housingConnectionData = null;
        housingWallConnections.lowerEdge.walls.RemoveFast(wall);
        if (housingWallConnections.lowerEdge.ShouldBeRemoved)
        {
            HousingVertex vertex = housingWallConnections.lowerEdge.vertex0;
            HousingVertex vertex2 = housingWallConnections.lowerEdge.vertex1;
            RemoveEdge(housingWallConnections.lowerEdge);
            if (vertex.ShouldBeRemoved)
            {
                RemoveVertex(vertex);
            }
            if (vertex2.ShouldBeRemoved)
            {
                RemoveVertex(vertex2);
            }
        }
        if (housingWallConnections.upperEdge.ShouldBeRemoved)
        {
            HousingVertex vertex3 = housingWallConnections.upperEdge.vertex0;
            HousingVertex vertex4 = housingWallConnections.upperEdge.vertex1;
            RemoveEdge(housingWallConnections.upperEdge);
            if (vertex3.ShouldBeRemoved)
            {
                RemoveVertex(vertex3);
            }
            if (vertex4.ShouldBeRemoved)
            {
                RemoveVertex(vertex4);
            }
        }
    }

    private void LinkPillar(StructureDrop pillar, Vector3 lowerVertexPosition)
    {
        Vector3 position = lowerVertexPosition + new Vector3(0f, 4.25f, 0f);
        HousingVertex housingVertex = FindVertex(lowerVertexPosition);
        if (housingVertex == null)
        {
            housingVertex = new HousingVertex();
            housingVertex.position = lowerVertexPosition;
            housingVertex.rotation = pillar.model.rotation.eulerAngles.y;
            verticesGrid.Add(lowerVertexPosition, housingVertex);
        }
        HousingVertex housingVertex2 = FindVertex(position);
        if (housingVertex2 == null)
        {
            housingVertex2 = new HousingVertex();
            housingVertex2.position = position;
            housingVertex2.rotation = pillar.model.rotation.eulerAngles.y;
            verticesGrid.Add(position, housingVertex2);
        }
        housingVertex.upperVertex = housingVertex2;
        housingVertex2.lowerVertex = housingVertex;
        HousingPillarConnections housingPillarConnections = (HousingPillarConnections)(pillar.housingConnectionData = new HousingPillarConnections());
        housingPillarConnections.lowerVertex = housingVertex;
        housingPillarConnections.upperVertex = housingVertex2;
        housingVertex.pillars.Add(pillar);
    }

    private void UnlinkPillar(StructureDrop pillar)
    {
        HousingPillarConnections housingPillarConnections = (HousingPillarConnections)pillar.housingConnectionData;
        pillar.housingConnectionData = null;
        housingPillarConnections.lowerVertex.pillars.RemoveFast(pillar);
        if (housingPillarConnections.lowerVertex.ShouldBeRemoved)
        {
            RemoveVertex(housingPillarConnections.lowerVertex);
        }
        if (housingPillarConnections.upperVertex.ShouldBeRemoved)
        {
            RemoveVertex(housingPillarConnections.upperVertex);
        }
    }

    internal bool DoesHitCountAsTerrain(RaycastHit hit)
    {
        if (hit.transform == null)
        {
            return false;
        }
        if (hit.transform.CompareTag("Ground"))
        {
            return true;
        }
        return LevelObjects.getAsset(hit.transform)?.allowStructures ?? false;
    }

    private float ScorePosition(Ray ray, Vector3 testPosition)
    {
        float sqrMagnitude = (testPosition - ray.origin).sqrMagnitude;
        if (sqrMagnitude > 64f)
        {
            return 65f;
        }
        float num = Vector3.Dot((testPosition - ray.origin).normalized, ray.direction);
        if (num < 0.9f)
        {
            return 65f;
        }
        return (1f - num) * sqrMagnitude;
    }

    internal bool FindEmptyFloorSlot(Ray ray, out Vector3 position, out float rotation)
    {
        position = default(Vector3);
        rotation = 0f;
        float num = 64f;
        bool result = false;
        foreach (HousingEdge item in edgesGrid.EnumerateItemsInSquare(ray.origin, 8f))
        {
            if (item.forwardFloors.IsEmpty())
            {
                Vector3 testPosition = item.position + item.direction * 3f * 0.5f;
                float num2 = ScorePosition(ray, testPosition);
                if (num2 < num)
                {
                    num = num2;
                    position = item.position + item.direction * 3f;
                    rotation = item.rotation + 180f;
                    result = true;
                }
            }
            if (item.backwardFloors.IsEmpty())
            {
                Vector3 testPosition2 = item.position - item.direction * 3f * 0.5f;
                float num3 = ScorePosition(ray, testPosition2);
                if (num3 < num)
                {
                    num = num3;
                    position = item.position - item.direction * 3f;
                    rotation = item.rotation;
                    result = true;
                }
            }
        }
        return result;
    }

    internal bool FindEmptyWallSlot(Ray ray, out Vector3 position, out float rotation)
    {
        position = default(Vector3);
        rotation = 0f;
        float num = 64f;
        bool result = false;
        foreach (HousingEdge item in edgesGrid.EnumerateItemsInSquare(ray.origin, 8f))
        {
            if (item.walls.IsEmpty())
            {
                Vector3 vector = item.position + new Vector3(0f, 2.125f, 0f);
                float num2 = ScorePosition(ray, vector);
                if (num2 < num)
                {
                    num = num2;
                    position = vector;
                    rotation = item.rotation;
                    result = true;
                }
            }
            if (item.lowerEdge == null || item.lowerEdge.walls.IsEmpty())
            {
                Vector3 vector2 = item.position + new Vector3(0f, -2.125f, 0f);
                float num3 = ScorePosition(ray, vector2);
                if (num3 < num)
                {
                    num = num3;
                    position = vector2;
                    rotation = item.rotation;
                    result = true;
                }
            }
        }
        return result;
    }

    internal bool FindEmptyPillarSlot(Ray ray, out Vector3 position, out float rotation)
    {
        position = default(Vector3);
        rotation = 0f;
        float num = 64f;
        bool result = false;
        foreach (HousingVertex item in verticesGrid.EnumerateItemsInSquare(ray.origin, 8f))
        {
            if (item.pillars.IsEmpty())
            {
                Vector3 vector = item.position + new Vector3(0f, 2.125f, 0f);
                float num2 = ScorePosition(ray, vector);
                if (num2 < num)
                {
                    num = num2;
                    position = vector;
                    rotation = item.rotation;
                    result = true;
                }
            }
            if (item.lowerVertex == null || item.lowerVertex.pillars.IsEmpty())
            {
                Vector3 vector2 = item.position + new Vector3(0f, -2.125f, 0f);
                float num3 = ScorePosition(ray, vector2);
                if (num3 < num)
                {
                    num = num3;
                    position = vector2;
                    rotation = item.rotation;
                    result = true;
                }
            }
        }
        return result;
    }

    private bool SnapFloorPlacementToEdge(ref Vector3 placementPosition)
    {
        foreach (HousingEdge item in edgesGrid.EnumerateItemsInSquare(placementPosition, 3.02f))
        {
            if (item.backwardFloors.IsEmpty() || item.forwardFloors.IsEmpty())
            {
                Vector3 vector = item.position + item.direction * 3f;
                if (vector.IsNearlyEqual(placementPosition, 0.02f))
                {
                    placementPosition = vector;
                    return true;
                }
                Vector3 vector2 = item.position - item.direction * 3f;
                if (vector2.IsNearlyEqual(placementPosition, 0.02f))
                {
                    placementPosition = vector2;
                    return true;
                }
            }
        }
        return false;
    }

    private void IgnoreVertexFloorsExceptNearPosition(HousingVertex vertex, Vector3 overlapCenter, float pendingItemRadius)
    {
        if (vertex == null)
        {
            return;
        }
        foreach (StructureDrop floor in vertex.floors)
        {
            int num;
            Vector3 vector;
            if (floor.asset.construct != EConstruct.FLOOR_POLY)
            {
                num = ((floor.asset.construct == EConstruct.ROOF_POLY) ? 1 : 0);
                if (num == 0)
                {
                    vector = floor.model.position;
                    goto IL_0066;
                }
            }
            else
            {
                num = 1;
            }
            vector = floor.model.TransformPoint(0f, 1.2679492f, 0f);
            goto IL_0066;
            IL_0066:
            Vector3 vector2 = vector;
            float num2 = ((num != 0) ? 1.7320508f : 3f) * 0.95f;
            float num3 = pendingItemRadius + num2;
            float num4 = num3 * num3;
            if ((vector2 - overlapCenter).GetHorizontalSqrMagnitude() > num4)
            {
                ignoreDrops.Add(floor);
            }
        }
    }

    private void IgnoreVertexPillarsFloorsAndWalls(HousingVertex vertex)
    {
        if (vertex == null)
        {
            return;
        }
        ignoreDrops.AddAny(vertex.pillars);
        ignoreDrops.AddAny(vertex.floors);
        foreach (HousingEdge edge in vertex.edges)
        {
            ignoreDrops.AddAny(edge.walls);
        }
    }

    private void IgnoreVertexPillarsAndWalls(HousingVertex vertex)
    {
        if (vertex == null)
        {
            return;
        }
        ignoreDrops.AddAny(vertex.pillars);
        foreach (HousingEdge edge in vertex.edges)
        {
            ignoreDrops.AddAny(edge.walls);
        }
    }

    private void IgnoreVertexFloors(HousingVertex vertex)
    {
        if (vertex != null)
        {
            ignoreDrops.AddAny(vertex.floors);
        }
    }

    private bool CanIgnoreOverlaps(int overlapCount, ref string obstructionHint)
    {
        for (int i = 0; i < overlapCount; i++)
        {
            Transform root = overlapBuffer[i].transform.root;
            StructureDrop structureDrop = StructureDrop.FindByTransformFastMaybeNull(root);
            if (structureDrop == null)
            {
                BarricadeDrop barricadeDrop = BarricadeDrop.FindByTransformFastMaybeNull(root);
                if (barricadeDrop != null && barricadeDrop.asset != null)
                {
                    obstructionHint = barricadeDrop.asset.itemName;
                }
                return false;
            }
            if (!ignoreDrops.Contains(structureDrop))
            {
                obstructionHint = structureDrop.asset.itemName;
                return false;
            }
        }
        return true;
    }

    private bool IsFloorAboveGround(Vector3 center, float testHeight)
    {
        if (Physics.Raycast(center + Vector3.up, Vector3.down, out var hitInfo, testHeight + 1f, 1146880))
        {
            return DoesHitCountAsTerrain(hitInfo);
        }
        return false;
    }

    private bool TestTriangleOverlapsCommon(Vector3 center, float placementRotation, float overlapPositionOffset, float overlapHalfHeight, ref string obstructionHint)
    {
        Vector3 outerHalfExtents = new Vector3(3.02f, overlapHalfHeight, 0.32000002f);
        Vector3 innerHalfExtents = new Vector3(1.9699999f, overlapHalfHeight, 0.59499997f);
        Vector3 characterHalfExtents = new Vector3(3.25f, overlapHalfHeight + 0.25f, 1.15f);
        if (!TestOverlaps(Quaternion.Euler(0f, placementRotation, 0f), ref obstructionHint))
        {
            return false;
        }
        if (!TestOverlaps(Quaternion.Euler(0f, placementRotation + 120f, 0f), ref obstructionHint))
        {
            return false;
        }
        if (!TestOverlaps(Quaternion.Euler(0f, placementRotation - 120f, 0f), ref obstructionHint))
        {
            return false;
        }
        return true;
        bool TestOverlaps(Quaternion edgeRotation, ref string obstructionHint)
        {
            Vector3 vector = edgeRotation * Vector3.forward;
            Vector3 center2 = center + new Vector3(0f, overlapPositionOffset, 0f) + vector * -1.4320507f;
            Vector3 center3 = center + new Vector3(0f, overlapPositionOffset, 0f) + vector * -0.55705076f;
            int overlapCount = Physics.OverlapBoxNonAlloc(center2, outerHalfExtents, overlapBuffer, edgeRotation, 402653184, QueryTriggerInteraction.Collide);
            bool flag = CanIgnoreOverlaps(overlapCount, ref obstructionHint);
            if (flag)
            {
                int overlapCount2 = Physics.OverlapBoxNonAlloc(center3, innerHalfExtents, overlapBuffer, edgeRotation, 402653184, QueryTriggerInteraction.Collide);
                flag &= CanIgnoreOverlaps(overlapCount2, ref obstructionHint);
            }
            if (!flag)
            {
                return false;
            }
            return !Physics.CheckBox(center + new Vector3(0f, overlapPositionOffset, 0f) + vector * -0.8320508f, characterHalfExtents, edgeRotation, 83887616, QueryTriggerInteraction.Collide);
        }
    }

    internal EHousingPlacementResult ValidateSquareFloorPlacement(float terrainTestHeight, ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
    {
        SnapFloorPlacementToEdge(ref placementPosition);
        if (!IsFloorAboveGround(placementPosition, terrainTestHeight))
        {
            return EHousingPlacementResult.MissingGround;
        }
        Vector3 vector = placementPosition + new Vector3(0f, -4.975f, 0f);
        Vector3 vector2 = new Vector3(3.02f, 5.075f, 3.02f);
        Quaternion quaternion = Quaternion.Euler(0f, placementRotation, 0f);
        Vector3 vector3 = placementPosition + quaternion * new Vector3(3f, 0f, 3f);
        Vector3 vector4 = placementPosition + quaternion * new Vector3(-3f, 0f, 3f);
        Vector3 vector5 = placementPosition + quaternion * new Vector3(3f, 0f, -3f);
        Vector3 vector6 = placementPosition + quaternion * new Vector3(-3f, 0f, -3f);
        HousingVertex vertex = FindVertex(vector3);
        HousingVertex vertex2 = FindVertex(vector4);
        HousingVertex vertex3 = FindVertex(vector5);
        HousingVertex vertex4 = FindVertex(vector6);
        ignoreDrops.Clear();
        IgnoreVertexPillarsAndWalls(vertex);
        IgnoreVertexPillarsAndWalls(vertex2);
        IgnoreVertexPillarsAndWalls(vertex3);
        IgnoreVertexPillarsAndWalls(vertex4);
        IgnoreVertexFloorsExceptNearPosition(vertex, vector, 3f);
        IgnoreVertexFloorsExceptNearPosition(vertex2, vector, 3f);
        IgnoreVertexFloorsExceptNearPosition(vertex3, vector, 3f);
        IgnoreVertexFloorsExceptNearPosition(vertex4, vector, 3f);
        HousingVertex vertex5 = FindVertex(vector3 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex6 = FindVertex(vector4 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex7 = FindVertex(vector5 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex8 = FindVertex(vector6 + new Vector3(0f, 4.25f, 0f));
        IgnoreVertexFloors(vertex5);
        IgnoreVertexFloors(vertex6);
        IgnoreVertexFloors(vertex7);
        IgnoreVertexFloors(vertex8);
        int overlapCount = Physics.OverlapBoxNonAlloc(vector, vector2, overlapBuffer, quaternion, 402653184, QueryTriggerInteraction.Collide);
        if (!CanIgnoreOverlaps(overlapCount, ref obstructionHint))
        {
            return EHousingPlacementResult.Obstructed;
        }
        Vector3 halfExtents = vector2 + new Vector3(0.25f, 0.25f, 0.25f);
        if (Physics.CheckBox(vector, halfExtents, quaternion, 83887616, QueryTriggerInteraction.Collide))
        {
            return EHousingPlacementResult.Obstructed;
        }
        return EHousingPlacementResult.Success;
    }

    internal EHousingPlacementResult ValidateTriangleFloorPlacement(float terrainTestHeight, ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
    {
        SnapFloorPlacementToEdge(ref placementPosition);
        if (!IsFloorAboveGround(placementPosition, terrainTestHeight))
        {
            return EHousingPlacementResult.MissingGround;
        }
        Quaternion quaternion = Quaternion.Euler(0f, placementRotation, 0f);
        Vector3 vector = placementPosition + quaternion * new Vector3(0f, 0f, -1.2679492f);
        Vector3 vector2 = placementPosition + quaternion * new Vector3(3f, 0f, -3f);
        Vector3 vector3 = placementPosition + quaternion * new Vector3(-3f, 0f, -3f);
        Vector3 vector4 = placementPosition + quaternion * new Vector3(0f, 0f, 2.1961524f);
        HousingVertex vertex = FindVertex(vector2);
        HousingVertex vertex2 = FindVertex(vector3);
        HousingVertex vertex3 = FindVertex(vector4);
        ignoreDrops.Clear();
        IgnoreVertexPillarsAndWalls(vertex);
        IgnoreVertexPillarsAndWalls(vertex2);
        IgnoreVertexPillarsAndWalls(vertex3);
        IgnoreVertexFloorsExceptNearPosition(vertex, vector, 1.7320508f);
        IgnoreVertexFloorsExceptNearPosition(vertex2, vector, 1.7320508f);
        IgnoreVertexFloorsExceptNearPosition(vertex3, vector, 1.7320508f);
        HousingVertex vertex4 = FindVertex(vector2 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex5 = FindVertex(vector3 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex6 = FindVertex(vector4 + new Vector3(0f, 4.25f, 0f));
        IgnoreVertexFloors(vertex4);
        IgnoreVertexFloors(vertex5);
        IgnoreVertexFloors(vertex6);
        if (!TestTriangleOverlapsCommon(vector, placementRotation, -4.975f, 5.075f, ref obstructionHint))
        {
            return EHousingPlacementResult.Obstructed;
        }
        return EHousingPlacementResult.Success;
    }

    internal EHousingPlacementResult ValidateSquareRoofPlacement(ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
    {
        if (!SnapFloorPlacementToEdge(ref placementPosition))
        {
            return EHousingPlacementResult.MissingSlot;
        }
        Vector3 vector = placementPosition;
        Vector3 vector2 = new Vector3(3.02f, 0.15f, 3.02f);
        Quaternion quaternion = Quaternion.Euler(0f, placementRotation, 0f);
        Vector3 vector3 = placementPosition + quaternion * new Vector3(3f, 0f, 3f);
        Vector3 vector4 = placementPosition + quaternion * new Vector3(-3f, 0f, 3f);
        Vector3 vector5 = placementPosition + quaternion * new Vector3(3f, 0f, -3f);
        Vector3 vector6 = placementPosition + quaternion * new Vector3(-3f, 0f, -3f);
        HousingVertex vertex = FindVertex(vector3);
        HousingVertex vertex2 = FindVertex(vector4);
        HousingVertex vertex3 = FindVertex(vector5);
        HousingVertex vertex4 = FindVertex(vector6);
        HousingVertex vertex5 = FindVertex(vector3 + new Vector3(0f, -4.25f, 0f));
        HousingVertex vertex6 = FindVertex(vector4 + new Vector3(0f, -4.25f, 0f));
        HousingVertex vertex7 = FindVertex(vector5 + new Vector3(0f, -4.25f, 0f));
        HousingVertex vertex8 = FindVertex(vector6 + new Vector3(0f, -4.25f, 0f));
        ignoreDrops.Clear();
        IgnoreVertexPillarsAndWalls(vertex);
        IgnoreVertexPillarsAndWalls(vertex2);
        IgnoreVertexPillarsAndWalls(vertex3);
        IgnoreVertexPillarsAndWalls(vertex4);
        IgnoreVertexFloorsExceptNearPosition(vertex, vector, 3f);
        IgnoreVertexFloorsExceptNearPosition(vertex2, vector, 3f);
        IgnoreVertexFloorsExceptNearPosition(vertex3, vector, 3f);
        IgnoreVertexFloorsExceptNearPosition(vertex4, vector, 3f);
        IgnoreVertexPillarsAndWalls(vertex5);
        IgnoreVertexPillarsAndWalls(vertex6);
        IgnoreVertexPillarsAndWalls(vertex7);
        IgnoreVertexPillarsAndWalls(vertex8);
        HousingVertex vertex9 = FindVertex(vector3 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex10 = FindVertex(vector4 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex11 = FindVertex(vector5 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex12 = FindVertex(vector6 + new Vector3(0f, 4.25f, 0f));
        IgnoreVertexFloors(vertex9);
        IgnoreVertexFloors(vertex10);
        IgnoreVertexFloors(vertex11);
        IgnoreVertexFloors(vertex12);
        int overlapCount = Physics.OverlapBoxNonAlloc(vector, vector2, overlapBuffer, quaternion, 402653184, QueryTriggerInteraction.Collide);
        if (!CanIgnoreOverlaps(overlapCount, ref obstructionHint))
        {
            return EHousingPlacementResult.Obstructed;
        }
        Vector3 halfExtents = vector2 + new Vector3(0.25f, 0.25f, 0.25f);
        if (Physics.CheckBox(vector, halfExtents, quaternion, 83887616, QueryTriggerInteraction.Collide))
        {
            return EHousingPlacementResult.Obstructed;
        }
        return EHousingPlacementResult.Success;
    }

    internal EHousingPlacementResult ValidateTriangleRoofPlacement(ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
    {
        if (!SnapFloorPlacementToEdge(ref placementPosition))
        {
            return EHousingPlacementResult.MissingSlot;
        }
        Quaternion quaternion = Quaternion.Euler(0f, placementRotation, 0f);
        Vector3 vector = placementPosition + quaternion * new Vector3(0f, 0f, -1.2679492f);
        Vector3 vector2 = placementPosition + quaternion * new Vector3(3f, 0f, -3f);
        Vector3 vector3 = placementPosition + quaternion * new Vector3(-3f, 0f, -3f);
        Vector3 vector4 = placementPosition + quaternion * new Vector3(0f, 0f, 2.1961524f);
        HousingVertex vertex = FindVertex(vector2);
        HousingVertex vertex2 = FindVertex(vector3);
        HousingVertex vertex3 = FindVertex(vector4);
        HousingVertex vertex4 = FindVertex(vector2 + new Vector3(0f, -4.25f, 0f));
        HousingVertex vertex5 = FindVertex(vector3 + new Vector3(0f, -4.25f, 0f));
        HousingVertex vertex6 = FindVertex(vector4 + new Vector3(0f, -4.25f, 0f));
        ignoreDrops.Clear();
        IgnoreVertexPillarsAndWalls(vertex);
        IgnoreVertexPillarsAndWalls(vertex2);
        IgnoreVertexPillarsAndWalls(vertex3);
        IgnoreVertexFloorsExceptNearPosition(vertex, vector, 1.7320508f);
        IgnoreVertexFloorsExceptNearPosition(vertex2, vector, 1.7320508f);
        IgnoreVertexFloorsExceptNearPosition(vertex3, vector, 1.7320508f);
        IgnoreVertexPillarsAndWalls(vertex4);
        IgnoreVertexPillarsAndWalls(vertex5);
        IgnoreVertexPillarsAndWalls(vertex6);
        HousingVertex vertex7 = FindVertex(vector2 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex8 = FindVertex(vector3 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex9 = FindVertex(vector4 + new Vector3(0f, 4.25f, 0f));
        IgnoreVertexFloors(vertex7);
        IgnoreVertexFloors(vertex8);
        IgnoreVertexFloors(vertex9);
        if (!TestTriangleOverlapsCommon(vector, placementRotation, 0f, 0.2f, ref obstructionHint))
        {
            return EHousingPlacementResult.Obstructed;
        }
        return EHousingPlacementResult.Success;
    }

    internal EHousingPlacementResult ValidateWallPlacement(ref Vector3 pendingPlacementPosition, float pivotOffset, bool requiresPillars, bool requiresFullHeightPillars, ref string obstructionHint)
    {
        Vector3 vector = pendingPlacementPosition + new Vector3(0f, 0f - pivotOffset, 0f);
        HousingEdge housingEdge = FindEdge(vector);
        Vector3 vector2;
        Vector3 vector3;
        HousingVertex housingVertex;
        HousingVertex housingVertex2;
        float rotation;
        if (housingEdge == null)
        {
            HousingEdge housingEdge2 = FindEdge(vector + new Vector3(0f, 4.25f, 0f));
            if (housingEdge2 == null)
            {
                return EHousingPlacementResult.MissingSlot;
            }
            if (housingEdge2.vertex0 == null || housingEdge2.vertex1 == null)
            {
                return EHousingPlacementResult.MissingPillar;
            }
            vector = housingEdge2.position + new Vector3(0f, -4.25f, 0f);
            vector2 = housingEdge2.vertex0.position + new Vector3(0f, -4.25f, 0f);
            vector3 = housingEdge2.vertex1.position + new Vector3(0f, -4.25f, 0f);
            housingVertex = FindVertex(vector2);
            housingVertex2 = FindVertex(vector3);
            rotation = housingEdge2.rotation;
        }
        else
        {
            if (!housingEdge.walls.IsEmpty())
            {
                return EHousingPlacementResult.Obstructed;
            }
            if (housingEdge.vertex0 == null || housingEdge.vertex1 == null)
            {
                return EHousingPlacementResult.MissingPillar;
            }
            vector = housingEdge.position;
            vector2 = housingEdge.vertex0.position;
            vector3 = housingEdge.vertex1.position;
            housingVertex = housingEdge.vertex0;
            housingVertex2 = housingEdge.vertex1;
            rotation = housingEdge.rotation;
        }
        pendingPlacementPosition = vector + new Vector3(0f, pivotOffset, 0f);
        if (requiresPillars)
        {
            if (housingVertex == null || housingVertex2 == null || housingVertex.pillars.IsEmpty() || housingVertex2.pillars.IsEmpty())
            {
                return EHousingPlacementResult.MissingPillar;
            }
            if (requiresFullHeightPillars && (!housingVertex.HasFullHeightPillar() || !housingVertex2.HasFullHeightPillar()))
            {
                return EHousingPlacementResult.MissingPillar;
            }
        }
        HousingVertex vertex = FindVertex(vector2 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex2 = FindVertex(vector2 + new Vector3(0f, -4.25f, 0f));
        HousingVertex vertex3 = FindVertex(vector3 + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex4 = FindVertex(vector3 + new Vector3(0f, -4.25f, 0f));
        Vector3 center = vector + new Vector3(0f, 2.125f, 0f);
        Vector3 vector4 = new Vector3(3.02f, 2.145f, 0.27f);
        Quaternion orientation = Quaternion.Euler(0f, rotation, 0f);
        ignoreDrops.Clear();
        IgnoreVertexPillarsFloorsAndWalls(housingVertex);
        IgnoreVertexPillarsFloorsAndWalls(housingVertex2);
        IgnoreVertexPillarsFloorsAndWalls(vertex);
        IgnoreVertexPillarsFloorsAndWalls(vertex3);
        IgnoreVertexPillarsAndWalls(vertex2);
        IgnoreVertexPillarsAndWalls(vertex4);
        HousingVertex vertex5 = FindVertex(vector2 + new Vector3(0f, 8.5f, 0f));
        HousingVertex vertex6 = FindVertex(vector3 + new Vector3(0f, 8.5f, 0f));
        IgnoreVertexFloors(vertex5);
        IgnoreVertexFloors(vertex6);
        int overlapCount = Physics.OverlapBoxNonAlloc(center, vector4, overlapBuffer, orientation, 402653184, QueryTriggerInteraction.Collide);
        if (!CanIgnoreOverlaps(overlapCount, ref obstructionHint))
        {
            return EHousingPlacementResult.Obstructed;
        }
        Vector3 halfExtents = vector4 + new Vector3(0.25f, 0.25f, 0.25f);
        if (Physics.CheckBox(center, halfExtents, orientation, 83887616, QueryTriggerInteraction.Collide))
        {
            return EHousingPlacementResult.Obstructed;
        }
        return EHousingPlacementResult.Success;
    }

    internal EHousingPlacementResult ValidatePillarPlacement(ref Vector3 pendingPlacementPosition, float pivotOffset, ref string obstructionHint)
    {
        Vector3 vector = pendingPlacementPosition + new Vector3(0f, 0f - pivotOffset, 0f);
        HousingVertex housingVertex = FindVertex(vector);
        if (housingVertex == null)
        {
            HousingVertex housingVertex2 = FindVertex(vector + new Vector3(0f, 4.25f, 0f));
            if (housingVertex2 == null)
            {
                return EHousingPlacementResult.MissingSlot;
            }
            vector = housingVertex2.position + new Vector3(0f, -4.25f, 0f);
        }
        else
        {
            if (!housingVertex.pillars.IsEmpty())
            {
                return EHousingPlacementResult.Obstructed;
            }
            vector = housingVertex.position;
        }
        pendingPlacementPosition = vector + new Vector3(0f, pivotOffset, 0f);
        HousingVertex vertex = FindVertex(vector + new Vector3(0f, 4.25f, 0f));
        HousingVertex vertex2 = FindVertex(vector + new Vector3(0f, -4.25f, 0f));
        Vector3 center = vector + new Vector3(0f, 2.125f, 0f);
        Vector3 vector2 = new Vector3(0.37f, 2.145f, 0.37f);
        ignoreDrops.Clear();
        IgnoreVertexPillarsFloorsAndWalls(housingVertex);
        IgnoreVertexPillarsFloorsAndWalls(vertex);
        IgnoreVertexPillarsAndWalls(vertex2);
        HousingVertex vertex3 = FindVertex(vector + new Vector3(0f, 8.5f, 0f));
        IgnoreVertexFloors(vertex3);
        int overlapCount = Physics.OverlapBoxNonAlloc(center, vector2, overlapBuffer, Quaternion.identity, 402653184, QueryTriggerInteraction.Collide);
        if (!CanIgnoreOverlaps(overlapCount, ref obstructionHint))
        {
            return EHousingPlacementResult.Obstructed;
        }
        Vector3 halfExtents = vector2 + new Vector3(0.25f, 0.25f, 0.25f);
        if (Physics.CheckBox(center, halfExtents, Quaternion.identity, 83887616, QueryTriggerInteraction.Collide))
        {
            return EHousingPlacementResult.Obstructed;
        }
        return EHousingPlacementResult.Success;
    }

    internal void DrawGizmos()
    {
    }
}
