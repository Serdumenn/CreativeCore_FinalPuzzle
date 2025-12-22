using System.Collections;
using UnityEngine;
using TMPro;

public class MessageUI : MonoBehaviour
{
    private enum DisplayMode { None, Timed, Persistent }

    [Header("UI (Timed Messages)")]
    public TMP_Text messageText;

    [Header("Timing")]
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

    // Keeps your existing API
    public void ShowMessage(string message, bool force = false)
    {
        ShowTimed(message, holdTime, force);
    }

    // Keeps your existing API
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

    /// <summary>
    /// Shows a message and keeps it visible indefinitely (no auto fade-out).
    /// Use this for "final" lines that must remain until scene load.
    /// </summary>
    public void ShowPersistent(string message, bool force = true)
    {
        if (messageText == null || string.IsNullOrEmpty(message)) return;

        bool same = (lastMessage == message);
        if (same && mode == DisplayMode.Persistent && !force) return;

        if (showCo != null)
        {
            StopCoroutine(showCo);
            showCo = null;
        }

        lastMessage = message;
        mode = DisplayMode.Persistent;

        messageText.text = message;
        currentAlpha = 1f;
        messageText.alpha = 1f;
    }

    /// <summary>
    /// Immediately clears any message (timed or persistent).
    /// </summary>
    public void HideImmediate()
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

    // Keeps your existing API name
    public void ClearMessage()
    {
        HideImmediate();
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

        if (holdSeconds > 0f)
            yield return new WaitForSecondsRealtime(holdSeconds);

        // If someone switched us to Persistent while we were waiting, do not fade out.
        if (mode == DisplayMode.Persistent)
        {
            showCo = null;
            yield break;
        }

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