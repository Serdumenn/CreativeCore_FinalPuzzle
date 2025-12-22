using System.Collections;
using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    private enum Mode { None, Prompt, Timed }

    [Header("UI")]
    public TMP_Text messageText;

    [Header("Timing")]
    [Min(0f)] public float fadeInTime = 0.15f;
    [Min(0f)] public float holdTime = 1.25f;
    [Min(0f)] public float fadeOutTime = 0.20f;

    private Coroutine co;
    private string current = "";
    private Mode mode = Mode.None;

    private void Awake()
    {
        if (messageText != null)
            messageText.alpha = 0f;
    }

    /// <summary>Timed message (fade in -> hold -> fade out). force=true mesaj aynı bile olsa yeniden gösterir.</summary>
    public void ShowMessage(string msg, bool force = false)
    {
        if (messageText == null) return;
        if (string.IsNullOrWhiteSpace(msg)) return;

        if (!force && mode == Mode.Timed && current == msg && co != null)
            return;

        current = msg;
        mode = Mode.Timed;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(TimedRoutine(msg));
    }

    /// <summary>Persistent prompt (flicker yapmaz). PromptManager bunu kullanır.</summary>
    public void ShowPrompt(string msg)
    {
        if (messageText == null) return;
        if (string.IsNullOrWhiteSpace(msg)) return;

        if (mode == Mode.Prompt && current == msg)
            return;

        current = msg;
        mode = Mode.Prompt;

        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }

        messageText.text = msg;
        messageText.alpha = 1f;
    }

    public void ClearPrompt(string msg)
    {
        if (messageText == null) return;
        if (mode != Mode.Prompt) return;
        if (current != msg) return;

        current = "";
        mode = Mode.None;
        messageText.text = "";
        messageText.alpha = 0f;
    }

    private IEnumerator TimedRoutine(string msg)
    {
        messageText.text = msg;

        // NOTE: Prompt modundayken timed mesaj basılırsa promptu ezer (istenen davranış).
        // Fade in
        if (fadeInTime <= 0f)
        {
            messageText.alpha = 1f;
        }
        else
        {
            float t = 0f;
            float a0 = messageText.alpha;
            while (t < fadeInTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Smooth01(t / fadeInTime);
                messageText.alpha = Mathf.Lerp(a0, 1f, k);
                yield return null;
            }
            messageText.alpha = 1f;
        }

        // Hold
        if (holdTime > 0f)
            yield return new WaitForSecondsRealtime(holdTime);

        // Fade out
        if (fadeOutTime <= 0f)
        {
            messageText.alpha = 0f;
        }
        else
        {
            float t = 0f;
            float a0 = messageText.alpha;
            while (t < fadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Smooth01(t / fadeOutTime);
                messageText.alpha = Mathf.Lerp(a0, 0f, k);
                yield return null;
            }
            messageText.alpha = 0f;
        }

        co = null;
        mode = Mode.None;
        current = "";
    }

    private static float Smooth01(float x) => x * x * (3f - 2f * x);
}