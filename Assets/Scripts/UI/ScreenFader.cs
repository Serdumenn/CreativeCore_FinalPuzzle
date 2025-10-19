using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Header("Defaults")]
    [Min(0.01f)] public float defaultDuration = 0.6f;
    public bool fadeOnStart = true;
    [Min(0f)] public float startDuration = 0.6f;

    private CanvasGroup cg;
    private Coroutine running;

    public bool IsFading  => running != null;
    public bool IsVisible => cg != null && cg.alpha > 0.001f;

    private void Awake()
    {
        CacheCanvasGroup();
    }

    private void Start()
    {
        CacheCanvasGroup();
        if (cg == null) return;

        cg.interactable   = false;
        cg.alpha          = fadeOnStart ? 1f : 0f;
        cg.blocksRaycasts = fadeOnStart;

        if (fadeOnStart)
            StartCoroutine(FadeIn(startDuration > 0f ? startDuration : defaultDuration));
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        CacheCanvasGroup();
        if (cg == null) yield break;

        float d = (duration > 0f) ? duration : defaultDuration;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(1f, d));
        yield return running;
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        CacheCanvasGroup();
        if (cg == null) yield break;

        float d = (duration > 0f) ? duration : defaultDuration;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(0f, d));
        yield return running;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float start = cg.alpha;
        cg.blocksRaycasts = true;
        cg.interactable   = false;

        if (duration <= Mathf.Epsilon)
        {
            cg.alpha = targetAlpha;
            cg.blocksRaycasts = targetAlpha > 0f;
            running = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t / duration));
            yield return null;
        }
        cg.alpha = targetAlpha;
        cg.blocksRaycasts = targetAlpha > 0f;
        running = null;
    }

    private void CacheCanvasGroup()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheCanvasGroup();
    }
#endif
}