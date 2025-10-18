using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Range(0.05f, 3f)] public float fadeDuration = 0.6f;

    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        // Menü sahnesi ilk açılırken siyah ekran görmeyelim
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    public IEnumerator FadeOut()
    {
        cg.blocksRaycasts = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.SmoothStep(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
    }
}
