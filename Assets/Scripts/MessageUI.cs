using System.Collections;
using UnityEngine;
using TMPro;

public class MessageUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text messageText;

    [Header("Timing")]
    [Min(0.0f)] public float fadeInTime  = 0.18f;
    [Min(0.05f)] public float holdTime   = 1.20f;
    [Min(0.0f)] public float fadeOutTime = 0.22f;

    private Coroutine showCo;
    private string lastMessage = "";
    private float currentAlpha = 0f;

    void Awake()
    {
        if (messageText != null)
        {
            currentAlpha = 0f;
            messageText.alpha = 0f;
        }
    }

    public void ShowMessage(string message, bool force = false)
    {
        if (messageText == null || string.IsNullOrEmpty(message)) return;

        bool same = (lastMessage == message);
        if (same && showCo != null && !force) return;

        lastMessage = message;

        if (showCo != null) StopCoroutine(showCo);
        showCo = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string msg)
    {
        messageText.text = msg;

        if (fadeInTime <= 0f)
        {
            currentAlpha = 1f;
            messageText.alpha = 1f;
        }
        else
        {
            float t = 0f;
            float start = currentAlpha;
            while (t < fadeInTime)
            {
                t += Time.unscaledDeltaTime;
                currentAlpha = Mathf.Lerp(start, 1f, Smooth(t / fadeInTime));
                messageText.alpha = currentAlpha;
                yield return null;
            }
            currentAlpha = 1f;
            messageText.alpha = 1f;
        }

        float h = 0f;
        while (h < holdTime)
        {
            h += Time.unscaledDeltaTime;
            yield return null;
        }

        if (fadeOutTime <= 0f)
        {
            currentAlpha = 0f;
            messageText.alpha = 0f;
        }
        else
        {
            float t2 = 0f;
            float start2 = currentAlpha;
            while (t2 < fadeOutTime)
            {
                t2 += Time.unscaledDeltaTime;
                currentAlpha = Mathf.Lerp(start2, 0f, Smooth(t2 / fadeOutTime));
                messageText.alpha = currentAlpha;
                yield return null;
            }
            currentAlpha = 0f;
            messageText.alpha = 0f;
        }

        showCo = null;
    }

    private static float Smooth(float x) => x * x * (3f - 2f * x);
}