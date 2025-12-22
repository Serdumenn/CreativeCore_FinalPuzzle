// Assets/Scripts/MessageUI.cs
using System.Collections;
using UnityEngine;
using TMPro;

public class MessageUI : MonoBehaviour
{
    private enum DisplayMode { None, Timed }

    [Header("UI (Timed Messages)")]
    public TMP_Text messageText;

    [Header("Timing (Default)")]
    [Min(0.0f)] public float fadeInTime  = 0.18f;
    [Min(0.05f)] public float holdTime   = 1.20f;
    [Min(0.0f)] public float fadeOutTime = 0.22f;

    private Coroutine showCo;
    private string lastMessage = "";
    private float currentAlpha = 0f;
    private DisplayMode mode = DisplayMode.None;

    void Awake()
    {
        if (messageText != null)
        {
            currentAlpha = 0f;
            messageText.alpha = 0f;
            messageText.text = "";
        }
    }

    /// <summary>
    /// Uses default holdTime.
    /// </summary>
    public void ShowMessage(string message, bool force = false)
    {
        if (messageText == null || string.IsNullOrEmpty(message)) return;

        bool same = (lastMessage == message);
        if (same && showCo != null && !force) return;

        lastMessage = message;
        mode = DisplayMode.Timed;

        if (showCo != null) StopCoroutine(showCo);
        showCo = StartCoroutine(ShowRoutine(message, holdTime));
    }

    /// <summary>
    /// Per-call hold duration override. This is what your Knight end-flow should use.
    /// </summary>
    public void ShowTimed(string message, float seconds, bool force = true)
    {
        if (messageText == null || string.IsNullOrEmpty(message)) return;

        bool same = (lastMessage == message);
        if (same && showCo != null && !force) return;

        lastMessage = message;
        mode = DisplayMode.Timed;

        if (showCo != null) StopCoroutine(showCo);
        showCo = StartCoroutine(ShowRoutine(message, Mathf.Max(0f, seconds)));
    }

    public void ClearMessage()
    {
        if (messageText == null) return;

        if (showCo != null)
        {
            StopCoroutine(showCo);
            showCo = null;
        }

        lastMessage = "";
        mode = DisplayMode.None;
        messageText.text = "";
        currentAlpha = 0f;
        messageText.alpha = 0f;
    }

    private IEnumerator ShowRoutine(string msg, float holdSeconds)
    {
        messageText.text = msg;

        // Fade in
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

        // Hold
        if (holdSeconds > 0f)
            yield return new WaitForSecondsRealtime(holdSeconds);

        // Fade out
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