using UnityEngine;

namespace SDG.Unturned;

public class CharacterAnimator : MonoBehaviour
{
    public static readonly float BLEND = 0.25f;

    protected Animation anim;

    protected Transform spine;

    protected Transform skull;

    protected Transform leftShoulder;

    protected Transform rightShoulder;

    protected string clip;

    public void sample()
    {
        anim.Sample();
    }

    public void mixAnimation(string name)
    {
        AnimationState animationState = anim[name];
        if (animationState != null)
        {
            animationState.layer = 1;
        }
    }

    public void mixAnimation(string name, bool mixLeftShoulder, bool mixRightShoulder)
    {
        mixAnimation(name, mixLeftShoulder, mixRightShoulder, mixSkull: false);
    }

    public void mixAnimation(string name, bool mixLeftShoulder, bool mixRightShoulder, bool mixSkull)
    {
        AnimationState animationState = anim[name];
        if (!(animationState == null))
        {
            if (mixLeftShoulder)
            {
                animationState.AddMixingTransform(leftShoulder, recursive: true);
            }
            if (mixRightShoulder)
            {
                animationState.AddMixingTransform(rightShoulder, recursive: true);
            }
            if (mixSkull)
            {
                animationState.AddMixingTransform(skull, recursive: true);
            }
            animationState.layer = 1;
        }
    }

    public void addAnimation(AnimationClip clip)
    {
        if (!(clip == null))
        {
            anim.AddClip(clip, clip.name);
            mixAnimation(clip.name, mixLeftShoulder: true, mixRightShoulder: true);
        }
    }

    public void removeAnimation(AnimationClip clip)
    {
        if (!(clip == null) && anim[clip.name] != null)
        {
            anim.RemoveClip(clip);
        }
    }

    public void setAnimationSpeed(string name, float speed)
    {
        AnimationState animationState = anim[name];
        if (animationState != null)
        {
            animationState.speed = speed;
        }
    }

    public float getAnimationLength(string name)
    {
        return GetAnimationLength(name);
    }

    public float GetAnimationLength(string name, bool scaled = true)
    {
        AnimationState animationState = anim[name];
        if (animationState != null)
        {
            if (scaled)
            {
                if (animationState.speed != 0f)
                {
                    return animationState.clip.length / animationState.speed;
                }
                return 0f;
            }
            return animationState.clip.length;
        }
        return 0f;
    }

    public bool getAnimationPlaying()
    {
        if (!string.IsNullOrEmpty(clip))
        {
            return anim.IsPlaying(clip);
        }
        return false;
    }

    public void state(string name)
    {
        if (!(anim[name] == null))
        {
            anim.CrossFade(name, BLEND);
        }
    }

    public bool checkExists(string name)
    {
        return anim[name] != null;
    }

    public bool play(string name, bool smooth)
    {
        if (anim[name] == null)
        {
            return false;
        }
        if (clip != "")
        {
            anim.Stop(clip);
        }
        clip = name;
        if (smooth)
        {
            anim.CrossFade(name, BLEND);
        }
        else
        {
            anim.Play(name);
        }
        return true;
    }

    public void stop(string name)
    {
        if (!(anim[name] == null) && name == clip)
        {
            anim.Stop(name);
            clip = "";
        }
    }

    protected void init()
    {
        clip = "";
        anim = GetComponent<Animation>();
        spine = base.transform.Find("Skeleton").Find("Spine");
        skull = spine.Find("Skull");
        leftShoulder = spine.Find("Left_Shoulder");
        rightShoulder = spine.Find("Right_Shoulder");
    }

    private void Awake()
    {
        init();
    }
}
