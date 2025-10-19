using System.Collections;
using UnityEngine;

/// <summary>
/// Ekranın üzerine binen siyah paneli alfa ile animler.
/// CanvasGroup'u aynı GameObject üzerinde bekler.
/// 
/// FadeOut  : 0 → 1 (karar)   | sahne geçişinden önce
/// FadeIn   : 1 → 0 (açıl)    | sahne veya menü açılışında
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Header("Defaults")]
    [Min(0.01f)] public float defaultDuration = 0.6f;
    public bool fadeOnStart = true;      // Menü açılırken yumuşak giriş
    [Min(0f)] public float startDuration = 0.6f;

    private CanvasGroup cg;
    private Coroutine running;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        // Fader paneli her zaman tıklamaları bloklamalı.
        cg.interactable = false;
        cg.blocksRaycasts = true;

        // Başlangıçta siyah (1) tutup sonra açacağız ya da direkt görünür yapacağız.
        cg.alpha = fadeOnStart ? 1f : 0f;
    }

    private void Start()
    {
        if (fadeOnStart)
            StartCoroutine(FadeIn(startDuration > 0f ? startDuration : defaultDuration));
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        float d = (duration > 0f) ? duration : defaultDuration;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(targetAlpha: 1f, d));
        yield return running;
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        float d = (duration > 0f) ? duration : defaultDuration;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(targetAlpha: 0f, d));
        yield return running;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;               // menüde timeScale etkilenmesin
            cg.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
        running = null;
    }
}
