using UnityEngine;

namespace SDG.Unturned;

public class HumanAnimator : CharacterAnimator
{
    public static readonly float LEAN = 20f;

    private float _lean;

    public float lean;

    private float _pitch;

    public float pitch;

    private float _offset;

    public float offset;

    public void force()
    {
        _lean = Mathf.Clamp(lean, -1f, 1f);
        _pitch = Mathf.Clamp(pitch, 1f, 179f) - 90f;
        _offset = offset;
    }

    public void apply()
    {
        bool animationPlaying = getAnimationPlaying();
        if (animationPlaying)
        {
            leftShoulder.parent = skull;
            rightShoulder.parent = skull;
        }
        spine.Rotate(0f, _pitch * 0.5f, _lean * LEAN);
        skull.Rotate(0f, _pitch * 0.5f, 0f);
        skull.position += skull.forward * offset;
        if (animationPlaying)
        {
            skull.Rotate(0f, 0f - spine.localRotation.eulerAngles.x + _pitch * 0.5f, 0f);
            leftShoulder.parent = spine;
            rightShoulder.parent = spine;
            skull.Rotate(0f, spine.localRotation.eulerAngles.x - _pitch * 0.5f, 0f);
        }
    }

    private void LateUpdate()
    {
        _lean = Mathf.LerpAngle(_lean, Mathf.Clamp(lean, -1f, 1f), 4f * Time.deltaTime);
        _pitch = Mathf.LerpAngle(_pitch, Mathf.Clamp(pitch, 1f, 179f) - 90f, 8f * Time.deltaTime);
        _offset = Mathf.Lerp(_offset, offset, 4f * Time.deltaTime);
        apply();
    }

    private void Awake()
    {
        init();
    }
}
