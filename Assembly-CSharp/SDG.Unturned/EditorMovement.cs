using UnityEngine;

namespace SDG.Unturned;

public class EditorMovement : MonoBehaviour
{
    private static bool _isMoving;

    private float speed = 32f;

    private Vector3 input;

    public static bool isMoving => _isMoving;

    private void Update()
    {
        if (EditorInteract.isFlying && Level.isEditor)
        {
            if (InputEx.GetKey(ControlsSettings.left))
            {
                input.x = -1f;
            }
            else if (InputEx.GetKey(ControlsSettings.right))
            {
                input.x = 1f;
            }
            else
            {
                input.x = 0f;
            }
            if (InputEx.GetKey(ControlsSettings.up))
            {
                input.z = 1f;
            }
            else if (InputEx.GetKey(ControlsSettings.down))
            {
                input.z = -1f;
            }
            else
            {
                input.z = 0f;
            }
            _isMoving = input.x != 0f || input.z != 0f;
            speed = Mathf.Clamp(speed + Input.GetAxis("mouse_z") * 0.2f * speed, 0.5f, 2048f);
            float num = 0f;
            if (InputEx.GetKey(ControlsSettings.ascend))
            {
                num = 1f;
            }
            else if (InputEx.GetKey(ControlsSettings.descend))
            {
                num = -1f;
            }
            base.transform.position += MainCamera.instance.transform.rotation * input * speed * Time.deltaTime + Vector3.up * num * Time.deltaTime * speed;
        }
    }
}
