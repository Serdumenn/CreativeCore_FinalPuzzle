using System.Collections;
using UnityEngine;

/// <summary>
/// Ekranın üzerine binen siyah paneli CanvasGroup alfa ile animler.
/// Aynı GameObject'te bir CanvasGroup bekler.
/// FadeOut : 0 → 1 (karart)   — sahne geçişinden önce
/// FadeIn  : 1 → 0 (aç)       — menü/sahne açılışında
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Header("Defaults")]
    [Min(0.01f)] public float defaultDuration = 0.6f;
    public bool fadeOnStart = true;
    [Min(0f)] public float startDuration = 0.6f;

    private CanvasGroup cg;
    private Coroutine running;

    /// Şu anda bir fade korutini çalışıyor mu?
    public bool IsFading => running != null;

    /// Panel görünür mü (alfa > 0)?
    public bool IsVisible => cg != null && cg.alpha > 0.001f;

    private void Reset()
    {
        CacheCanvasGroup();
        ApplyInitialState();
    }

    private void Awake()
    {
        CacheCanvasGroup();
        ApplyInitialState();
    }

    private void Start()
    {
        if (fadeOnStart)
            StartCoroutine(FadeIn(startDuration > 0f ? startDuration : defaultDuration));
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        CacheCanvasGroup();
        if (cg == null) yield break;

        float d = (duration > 0f) ? duration : defaultDuration;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(targetAlpha: 1f, d));
        yield return running;
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        CacheCanvasGroup();
        if (cg == null) yield break;

        float d = (duration > 0f) ? duration : defaultDuration;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(targetAlpha: 0f, d));
        yield return running;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float start = cg.alpha;

        // Fade süresince UI tıklamalarını engelle.
        cg.blocksRaycasts = true;
        cg.interactable  = false;

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
            t += Time.unscaledDeltaTime; // menüde timeScale etkilenmesin
            cg.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t / duration));
            yield return null;
        }
        cg.alpha = targetAlpha;

        // Tamamen açıldığında raycast engelini kaldır (yalnızca görünürken blocksRaycasts açık kalsın).
        cg.blocksRaycasts = targetAlpha > 0f;
        running = null;
    }

    private void CacheCanvasGroup()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
    }

    private void ApplyInitialState()
    {
        if (cg == null) return;
        cg.interactable   = false;
        cg.alpha          = fadeOnStart ? 1f : 0f;
        cg.blocksRaycasts = fadeOnStart; // startta siyahsa tıklamayı blokla
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheCanvasGroup();
        if (!Application.isPlaying)
            ApplyInitialState();
    }
#endif
}
