using UnityEngine;

namespace SDG.Unturned;

public class NonPathfindingZombieMovementComponent : MonoBehaviour, IUnturnedPathfindingMovementComponentInterface
{
    public bool canMove = true;

    public bool canTurn = true;

    public bool canSearch = true;

    public float speed;

    public Transform targetTransform;

    public Vector3 targetDirection;

    private CharacterController characterController;

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
            return targetTransform;
        }
        set
        {
            targetTransform = value;
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

    public void Move(float deltaTime)
    {
        if (canMove)
        {
            Vector3 normalized = (targetTransform.position - base.transform.position).GetHorizontal().normalized;
            float target = 57.29578f * (0f - Mathf.Atan2(normalized.z, normalized.x)) + 90f;
            float y = base.transform.rotation.eulerAngles.y;
            y = Mathf.MoveTowardsAngle(y, target, 720f * deltaTime);
            base.transform.rotation = Quaternion.Euler(0f, y, 0f);
            if (canTurn)
            {
                targetDirection = normalized;
            }
            Vector3 vector = normalized * speed;
            vector.y = Physics.gravity.y * 2f;
            characterController.Move(vector * deltaTime);
        }
    }

    protected void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
}
