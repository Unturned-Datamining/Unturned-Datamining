using System;
using System.Collections.Generic;
using Pathfinding;
using SDG.Unturned;
using UnityEngine;

/// <summary>
/// Nelson 2025-04-30: carrying this over from whatever very old version of the A* Pathfinding Project the game was
/// using because it has a few hacked-in behaviors the newer components don't. Ideally, we should write a custom
/// movement component using the newer AIBase class.
/// </summary>
[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/LegacyAIPath")]
public class LegacyAIPathNoRedist : MonoBehaviour, IUnturnedPathfindingMovementComponentInterface
{
    /// Determines how often it will search for new paths. 
    /// If you have fast moving targets or AIs, you might want to set it to a lower value.
    /// The value is in seconds between path requests.
    public float repathRate = 0.5f;

    /// Target to move towards.
    /// The AI will try to follow/move towards this target.
    /// It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
    public Transform target;

    /// Enables or disables searching for paths.
    /// Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
    /// \see #canMove
    public bool canSearch = true;

    /// Enables or disables movement.
    /// \see #canSearch 
    public bool canMove = true;

    public bool canTurn = true;

    /// Maximum velocity.
    /// This is the maximum speed in world units per second.
    public float speed = 3f;

    /// Rotation speed.
    /// Rotation is calculated using Quaternion.SLerp. This variable represents the damping, the higher, the faster it will be able to rotate.
    public float turningSpeed = 5f;

    /// Distance from the target point where the AI will start to slow down.
    /// 	 * Note that this doesn't only affect the end point of the path
    ///  	 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this
    public float slowdownDistance = 0.6f;

    /// Determines within what range it will switch to target the next waypoint in the path 
    public float pickNextWaypointDist = 2f;

    /// Target point is Interpolated on the current segment in the path so that it has a distance of #forwardLook from the AI.
    /// See the detailed description of AIPath for an illustrative image 
    public float forwardLook = 1f;

    /// Distance to the end point to consider the end of path to be reached.
    /// When this has been reached, the AI will not move anymore until the target changes and OnTargetReached will be called.
    public float endReachedDistance = 0.2f;

    /// Do a closest point on path check when receiving path callback.
    /// Usually the AI has moved a bit between requesting the path, and getting it back, and there is usually a small gap between the AI
    /// and the closest node.
    /// If this option is enabled, it will simulate, when the path callback is received, movement between the closest node and the current
    /// AI position. This helps to reduce the moments when the AI just get a new path back, and thinks it ought to move backwards to the start of the new path
    /// even though it really should just proceed forward.
    public bool closestOnPathCheck = true;

    protected float minMoveScale = 0.05f;

    /// Cached Seeker component 
    protected Seeker seeker;

    /// Cached Transform component 
    protected Transform tr;

    /// Time when the last path request was sent 
    protected float lastRepath = -9999f;

    /// Current path which is followed 
    protected Path path;

    /// Cached CharacterController component 
    protected CharacterController controller;

    /// Current index in the path which is current target 
    protected int currentWaypointIndex;

    /// Holds if the end-of-path is reached
    /// \see TargetReached 
    protected bool targetReached;

    /// Only when the previous path has been returned should be search for a new path 
    protected bool canSearchAgain = true;

    protected Vector3 lastFoundWaypointPosition;

    protected float lastFoundWaypointTime = -9999f;

    /// Point to where the AI is heading.
    /// Filled in by #CalculateVelocity 
    protected Vector3 targetPoint;

    /// Relative direction to where the AI is heading.
    /// Filled in by #CalculateVelocity 
    public Vector3 targetDirection;

    /// Returns if the end-of-path has been reached
    /// \see targetReached 
    public bool TargetReached => targetReached;

    public bool CanMove
    {
        get
        {
            return canMove;
        }
        set
        {
            canMove = value;
        }
    }

    public bool CanTurn
    {
        get
        {
            return canTurn;
        }
        set
        {
            canTurn = value;
        }
    }

    public bool CanSearch
    {
        get
        {
            return canSearch;
        }
        set
        {
            canSearch = value;
        }
    }

    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            speed = value;
        }
    }

    public Transform TargetTransform
    {
        get
        {
            return target;
        }
        set
        {
            target = value;
        }
    }

    public Vector3 TargetDirection
    {
        get
        {
            return targetDirection;
        }
        set
        {
            targetDirection = value;
        }
    }

    /// Initializes reference variables.
    /// 	 * If you override this function you should in most cases call base.Awake () at the start of it.
    /// 	  * 
    protected virtual void Awake()
    {
        seeker = GetComponent<Seeker>();
        tr = base.transform;
        controller = GetComponent<CharacterController>();
    }

    /// Run at start and when reenabled.
    /// Starts RepeatTrySearchPath.
    ///
    /// \see Start
    protected virtual void OnEnable()
    {
        lastRepath = -9999f;
        canSearchAgain = true;
        lastFoundWaypointPosition = GetFeetPosition();
    }

    public void OnDisable()
    {
        if ((UnityEngine.Object)(object)seeker != null && !seeker.IsDone())
        {
            seeker.GetCurrentPath().Error();
        }
        if (path != null)
        {
            path.Release(this);
        }
        path = null;
    }

    /// Requests a path to the target 
    public virtual void SearchPath()
    {
        if (target == null)
        {
            throw new InvalidOperationException("Target is null");
        }
        lastRepath = Time.time;
        Vector3 position = target.position;
        canSearchAgain = false;
        seeker.StartPath(GetFeetPosition(), position, OnPathComplete);
    }

    public virtual void OnTargetReached()
    {
    }

    /// Called when a requested path has finished calculation.
    /// A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
    /// Finally it is returned to the seeker which forwards it to this function.\n
    public virtual void OnPathComplete(Path _p)
    {
        if (!(_p is ABPath aBPath))
        {
            throw new Exception("This function only handles ABPaths, do not use special path types");
        }
        canSearchAgain = true;
        aBPath.Claim(this);
        if (aBPath.error)
        {
            aBPath.Release(this);
            return;
        }
        if (path != null)
        {
            path.Release(this);
        }
        path = aBPath;
        currentWaypointIndex = 0;
        targetReached = false;
        if (closestOnPathCheck)
        {
            Vector3 vector = ((Time.time - lastFoundWaypointTime < 0.3f) ? lastFoundWaypointPosition : aBPath.originalStartPoint);
            Vector3 vector2 = GetFeetPosition() - vector;
            float magnitude = vector2.magnitude;
            vector2 /= magnitude;
            int num = (int)(magnitude / pickNextWaypointDist);
            for (int i = 0; i <= num; i++)
            {
                CalculateVelocity(vector);
                vector += vector2;
            }
        }
    }

    public virtual Vector3 GetFeetPosition()
    {
        return tr.position;
    }

    public void move(float delta)
    {
        if (!canMove)
        {
            return;
        }
        if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch && target != null)
        {
            SearchPath();
        }
        if (path != null)
        {
            Vector3 vector = CalculateVelocity(base.transform.position);
            vector.y = Physics.gravity.y * 2f;
            RotateTowards(targetDirection);
            if (controller != null && controller.enabled)
            {
                controller.Move(vector * delta);
            }
        }
    }

    protected float XZSqrMagnitude(Vector3 a, Vector3 b)
    {
        float num = b.x - a.x;
        float num2 = b.z - a.z;
        return num * num + num2 * num2;
    }

    /// Calculates desired velocity.
    /// Finds the target path segment and returns the forward direction, scaled with speed.
    /// A whole bunch of restrictions on the velocity is applied to make sure it doesn't overshoot, does not look too far ahead,
    /// and slows down when close to the target.
    /// /see speed
    /// /see endReachedDistance
    /// /see slowdownDistance
    /// /see CalculateTargetPoint
    /// /see targetPoint
    /// /see targetDirection
    /// /see currentWaypointIndex
    protected Vector3 CalculateVelocity(Vector3 currentPosition)
    {
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0)
        {
            return Vector3.zero;
        }
        List<Vector3> vectorPath = path.vectorPath;
        if (vectorPath.Count == 1)
        {
            vectorPath.Insert(0, currentPosition);
        }
        if (currentWaypointIndex >= vectorPath.Count)
        {
            currentWaypointIndex = vectorPath.Count - 1;
        }
        if (currentWaypointIndex <= 1)
        {
            currentWaypointIndex = 1;
        }
        while (currentWaypointIndex < vectorPath.Count - 1 && XZSqrMagnitude(vectorPath[currentWaypointIndex], currentPosition) < pickNextWaypointDist * pickNextWaypointDist)
        {
            lastFoundWaypointPosition = currentPosition;
            lastFoundWaypointTime = Time.time;
            currentWaypointIndex++;
        }
        _ = vectorPath[currentWaypointIndex] - vectorPath[currentWaypointIndex - 1];
        Vector3 vector = CalculateTargetPoint(currentPosition, vectorPath[currentWaypointIndex - 1], vectorPath[currentWaypointIndex], currentWaypointIndex == vectorPath.Count - 1);
        Vector3 vector2 = vector - currentPosition;
        vector2.y = 0f;
        float magnitude = vector2.magnitude;
        float num = Mathf.Clamp01(magnitude / slowdownDistance);
        if (canTurn)
        {
            targetDirection = vector2;
        }
        targetPoint = vector;
        if (currentWaypointIndex == vectorPath.Count - 1 && magnitude <= endReachedDistance)
        {
            if (!targetReached)
            {
                targetReached = true;
                OnTargetReached();
            }
            return Vector3.zero;
        }
        Vector3 forward = tr.forward;
        float a = Vector3.Dot(vector2.normalized, forward);
        float num2 = speed * Mathf.Max(a, minMoveScale) * num;
        if (Time.deltaTime > 0f)
        {
            num2 = Mathf.Clamp(num2, 0f, magnitude / (Time.deltaTime * 2f));
        }
        return forward * num2;
    }

    /// Rotates in the specified direction.
    /// Rotates around the Y-axis.
    /// \see turningSpeed
    protected virtual void RotateTowards(Vector3 dir)
    {
        if (!(dir == Vector3.zero))
        {
            Quaternion rotation = tr.rotation;
            Quaternion b = Quaternion.LookRotation(dir);
            Vector3 eulerAngles = Quaternion.Slerp(rotation, b, turningSpeed * Time.deltaTime).eulerAngles;
            eulerAngles.z = 0f;
            eulerAngles.x = 0f;
            rotation = Quaternion.Euler(eulerAngles);
            tr.rotation = rotation;
        }
    }

    /// Calculates target point from the current line segment.
    /// \param p Current position
    /// \param a Line segment start
    /// \param b Line segment end
    /// The returned point will lie somewhere on the line segment.
    /// \see #forwardLook
    /// \todo This function uses .magnitude quite a lot, can it be optimized?
    protected Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b, bool canGoDirectly)
    {
        if (canGoDirectly && (b - target.position).sqrMagnitude < 16f)
        {
            return target.position;
        }
        a.y = p.y;
        b.y = p.y;
        float magnitude = (a - b).magnitude;
        if (magnitude == 0f)
        {
            return a;
        }
        float num = Mathf.Clamp01(NearestPointFactor(a, b, p));
        float magnitude2 = ((b - a) * num + a - p).magnitude;
        float num2 = Mathf.Clamp(forwardLook - magnitude2, 0f, forwardLook) / magnitude;
        num2 = Mathf.Clamp(num2 + num, 0f, 1f);
        return (b - a) * num2 + a;
    }

    protected float NearestPointFactor(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 rhs = lineEnd - lineStart;
        float magnitude = rhs.magnitude;
        rhs /= magnitude;
        return Vector3.Dot(point - lineStart, rhs) / magnitude;
    }

    public void Move(float deltaTime)
    {
        move(deltaTime);
    }
}
