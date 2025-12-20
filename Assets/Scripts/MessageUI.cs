using System.Collections;
using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    private enum Mode { None, Prompt, Timed }

    [Header("UI")]
    public TMP_Text messageText;

    [Header("Timing (Timed messages)")]
    [Min(0f)] public float fadeInTime = 0.15f;
    [Min(0f)] public float holdTime = 1.20f;
    [Min(0f)] public float fadeOutTime = 0.20f;

    private Coroutine timedCo;
    private float currentAlpha = 0f;
    private string lastPrompt = "";
    private Mode mode = Mode.None;

    private void Awake()
    {
        if (messageText != null)
        {
            messageText.text = "";
            messageText.alpha = 0f;
            currentAlpha = 0f;
            mode = Mode.None;
        }
    }

    // TIMED: fades in, holds, fades out
    public void ShowTimed(string message, bool force = false)
    {
        if (messageText == null) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        // If we're already showing the same timed message and not forced, ignore.
        if (!force && mode == Mode.Timed && messageText.text == message && timedCo != null)
            return;

        mode = Mode.Timed;

        if (timedCo != null) StopCoroutine(timedCo);
        timedCo = StartCoroutine(TimedRoutine(message));
    }

    // PROMPT: persistent (no fade), stays until ClearPrompt()
    public void ShowPrompt(string message)
    {
        if (messageText == null) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        // If already same prompt, do nothing
        if (mode == Mode.Prompt && lastPrompt == message)
            return;

        // Stop timed routine if running
        if (timedCo != null)
        {
            StopCoroutine(timedCo);
            timedCo = null;
        }

        mode = Mode.Prompt;
        lastPrompt = message;

        messageText.text = message;
        currentAlpha = 1f;
        messageText.alpha = 1f;
    }

    public void ClearPrompt()
    {
        if (messageText == null) return;
        if (mode != Mode.Prompt) return;

        lastPrompt = "";
        mode = Mode.None;

        messageText.text = "";
        currentAlpha = 0f;
        messageText.alpha = 0f;
    }

    private IEnumerator TimedRoutine(string msg)
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
                currentAlpha = Mathf.Lerp(start, 1f, Smooth01(t / fadeInTime));
                messageText.alpha = currentAlpha;
                yield return null;
            }
            currentAlpha = 1f;
            messageText.alpha = 1f;
        }

        // Hold
        if (holdTime > 0f)
            yield return new WaitForSecondsRealtime(holdTime);

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
                currentAlpha = Mathf.Lerp(start2, 0f, Smooth01(t2 / fadeOutTime));
                messageText.alpha = currentAlpha;
                yield return null;
            }
            currentAlpha = 0f;
            messageText.alpha = 0f;
        }

        // Clear text after fading out (optional, keeps clean)
        if (mode == Mode.Timed)
        {
            messageText.text = "";
            mode = Mode.None;
        }

        timedCo = null;
    }

    private static float Smooth01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }

    // -------------------------
    // BACKWARD COMPATIBILITY
    // Old scripts call ShowMessage(...)
    // -------------------------
    public void ShowMessage(string message, bool force = false)
    {
        ShowTimed(message, force);
    }

    public void ShowMessage(string message)
    {
        ShowTimed(message, true);
    }

    public void ClearMessage()
    {
        ClearPrompt();
    }
}