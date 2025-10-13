using UnityEngine;
using TMPro;
using System.Collections;

public class MessageUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI label;
    public float fadeDuration = 0.4f;
    public float holdSeconds = 1.6f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (label != null)
        {
            Color c = label.color;
            c.a = 0;
            label.color = c;
        }
    }

    public void ShowMessage(string text)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeMessage(text));
    }

    private IEnumerator FadeMessage(string text)
    {
        if (label == null) yield break;

        label.text = text;

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = t / fadeDuration;
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1);

        yield return new WaitForSeconds(holdSeconds);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = 1 - (t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0);
    }

    private void SetAlpha(float alpha)
    {
        if (label != null)
        {
            Color c = label.color;
            c.a = alpha;
            label.color = c;
        }
    }
}
