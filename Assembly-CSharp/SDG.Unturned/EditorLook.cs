using UnityEngine;

namespace SDG.Unturned;

public class EditorLook : MonoBehaviour
{
    private static float _pitch;

    private static float _yaw;

    private Camera highlightCamera;

    public static float pitch => _pitch;

    public static float yaw => _yaw;

    private void Update()
    {
        if (EditorInteract.isFlying && Level.isEditor)
        {
            MainCamera.instance.fieldOfView = Mathf.Lerp(MainCamera.instance.fieldOfView, OptionsSettings.DesiredVerticalFieldOfView + (float)((EditorMovement.isMoving && InputEx.GetKey(ControlsSettings.modify)) ? 10 : 0), 8f * Time.deltaTime);
            highlightCamera.fieldOfView = MainCamera.instance.fieldOfView;
            _yaw += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_x");
            if (ControlsSettings.invert)
            {
                _pitch += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y");
            }
            else
            {
                _pitch -= ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y");
            }
            if (pitch > 90f)
            {
                _pitch = 90f;
            }
            else if (pitch < -90f)
            {
                _pitch = -90f;
            }
            MainCamera.instance.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            base.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }

    private void Start()
    {
        MainCamera.instance.fieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
        highlightCamera = MainCamera.instance.transform.Find("HighlightCamera").GetComponent<Camera>();
        highlightCamera.fieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
        highlightCamera.eventMask = 0;
        _pitch = MainCamera.instance.transform.localRotation.eulerAngles.x;
        if (pitch > 90f)
        {
            _pitch = -360f + pitch;
        }
        _yaw = base.transform.rotation.eulerAngles.y;
    }
}
