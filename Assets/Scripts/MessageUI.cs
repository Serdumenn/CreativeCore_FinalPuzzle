using System.Collections;
using UnityEngine;
using TMPro;

public class MessageUI : MonoBehaviour
{
    public TMP_Text messageText;
    [Min(0.1f)] public float holdTime = 1.2f;
    [Min(0.05f)] public float fadeTime = 0.35f;

    private Coroutine fadeCo;
    private string current;

    private void Awake()
    {
        if (messageText != null)
            messageText.alpha = 0f;
    }

    public void ShowMessage(string newMessage, bool forceRestart = false)
    {
        if (messageText == null || string.IsNullOrEmpty(newMessage)) return;

        // Aynı mesajı her frame tekrar başlatma
        if (!forceRestart && newMessage == current && messageText.alpha > 0.9f)
            return;

        current = newMessage;

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(Co_ShowThenFade(newMessage));
    }

    private IEnumerator Co_ShowThenFade(string msg)
    {
        messageText.text = msg;

        // anında görünür
        messageText.alpha = 1f;

        // kısa tut
        yield return new WaitForSecondsRealtime(holdTime);

        float t = 0f;
        float start = messageText.alpha;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            messageText.alpha = Mathf.Lerp(start, 0f, t / fadeTime);
            yield return null;
        }

        messageText.alpha = 0f;
        fadeCo = null;
    }
}
