using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Timer Event Hook")]
public class TimerEventHook : MonoBehaviour
{
    public UnityEvent OnTimerTriggered;

    public float DefaultDuration;

    public bool DefaultLooping;

    private IEnumerator coroutine;

    private bool shouldTimerLoop;

    public void SetTimer(float duration)
    {
        SetTimer(duration, looping: false);
    }

    public void SetTimer(float duration, bool looping)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        shouldTimerLoop = looping;
        coroutine = InternalStartTimer(duration);
        StartCoroutine(coroutine);
    }

    public void SetDefaultTimer()
    {
        SetTimer(DefaultDuration, DefaultLooping);
    }

    public void CancelTimer()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        shouldTimerLoop = false;
    }

    private IEnumerator InternalStartTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        coroutine = null;
        OnTimerTriggered.Invoke();
        if (shouldTimerLoop && coroutine == null)
        {
            coroutine = InternalStartTimer(duration);
            StartCoroutine(coroutine);
        }
    }
}
