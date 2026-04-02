using System;
using Pathfinding;
using UnityEngine;

namespace SDG.Unturned;

public class UnturnedPathfinding_ASPFP : IUnturnedPathfindingInterface
{
    public void OnGameLevelInstantiated()
    {
        AstarPath component = new GameObject("A*", typeof(AstarPath)).GetComponent<AstarPath>();
        component.threadCount = ThreadCount.Eight;
        component.scanOnStartup = false;
        component.logPathResults = PathLog.None;
        component.showNavGraphs = false;
    }

    public IUnturnedNavmeshInterface CreateNavmesh()
    {
        return new UnturnedNavmesh_ASPFP();
    }

    public IUnturnedPerNavmeshEditorInterface CreateFlag(Flag owner)
    {
        return new UnturnedNavmeshFlag_ASPFP(owner);
    }

    public IUnturnedNavmeshCutInterface CreateCutForIOBS(InteractableObjectBinaryState iobs)
    {
        Transform transform = iobs.transform.Find("Nav/Blocker");
        if (transform == null)
        {
            return null;
        }
        NavmeshCut navmeshCut = transform.GetComponent<NavmeshCut>();
        if ((UnityEngine.Object)(object)navmeshCut == null)
        {
            BoxCollider component = transform.GetComponent<BoxCollider>();
            if (component != null)
            {
                navmeshCut = InitCutComponentFromBox(component);
            }
        }
        if ((UnityEngine.Object)(object)navmeshCut == null)
        {
            return null;
        }
        Vector3 position = transform.position;
        NavGraph navGraph = null;
        if ((UnityEngine.Object)(object)AstarPath.active != null)
        {
            NavGraph[] graphs = AstarPath.active.graphs;
            foreach (NavGraph navGraph2 in graphs)
            {
                if (navGraph2 is RecastGraph recastGraph)
                {
                    float f = position.x - recastGraph.forcedBoundsCenter.x;
                    float f2 = position.z - recastGraph.forcedBoundsCenter.z;
                    if (Mathf.Abs(f) <= recastGraph.forcedBoundsSize.x / 2f && Mathf.Abs(f2) <= recastGraph.forcedBoundsSize.z / 2f)
                    {
                        navGraph = navGraph2;
                        break;
                    }
                }
            }
        }
        if (navGraph != null && navGraph.graphIndex < 31)
        {
            navmeshCut.graphMask = GraphMask.FromGraph(navGraph);
            ((Behaviour)(object)navmeshCut).enabled = false;
            ((Behaviour)(object)navmeshCut).enabled = true;
        }
        return new UnturnedNavmeshCutInterface_ASPFP(navmeshCut);
    }

    public Type GetCutComponentType()
    {
        return typeof(NavmeshCut);
    }

    public IUnturnedPathfindingMovementComponentInterface CreateMovementComponentForZombie(Zombie zombie)
    {
        GameObject gameObject = zombie.gameObject;
        gameObject.AddComponent<Seeker>().drawGizmos = false;
        gameObject.AddComponent<FunnelModifier>();
        LegacyAIPathNoRedist legacyAIPathNoRedist = gameObject.AddComponent<LegacyAIPathNoRedist>();
        legacyAIPathNoRedist.pickNextWaypointDist = 1f;
        legacyAIPathNoRedist.forwardLook = 4f;
        legacyAIPathNoRedist.endReachedDistance = 0.75f;
        legacyAIPathNoRedist.closestOnPathCheck = false;
        return legacyAIPathNoRedist;
    }

    private NavmeshCut InitCutComponentFromBox(BoxCollider box)
    {
        NavmeshCut navmeshCut = box.gameObject.AddComponent<NavmeshCut>();
        navmeshCut.type = NavmeshCut.MeshType.Rectangle;
        navmeshCut.updateDistance = 0.4f;
        navmeshCut.isDual = false;
        navmeshCut.cutsAddedGeom = true;
        navmeshCut.updateRotationDistance = 10f;
        navmeshCut.useRotationAndScale = true;
        navmeshCut.center = box.center;
        navmeshCut.rectangleSize = new Vector2(box.size.x, box.size.z);
        navmeshCut.height = box.size.y;
        UnityEngine.Object.Destroy(box);
        return navmeshCut;
    }
}
