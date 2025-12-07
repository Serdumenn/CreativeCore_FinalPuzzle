using System.Collections;
using UnityEngine;
using TMPro;

public class MessageUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text messageText;

    [Header("Fade Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeDuration = 0.5f;

    [SerializeField] private float totalTimeVisible = 3f;

    private Coroutine showCo;
    private float currentAlpha = 0f;

    private void Awake()
    {
        if (messageText != null)
            messageText.alpha = 0f;
    }

    public void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.LogWarning("MessageUI: No TMP_Text assigned!");
            return;
        }

        messageText.text = message;

        if (showCo != null)
            StopCoroutine(showCo);

        showCo = StartCoroutine(Smooth());
    }

    public void HideMessage()
    {
        if (messageText == null) return;

        // Fade coroutine çalışıyorsa durdur
        if (showCo != null)
        {
            StopCoroutine(showCo);
            showCo = null;
        }

        // Alpha sıfırlanır, mesaj tamamen kaybolur
        currentAlpha = 0f;
        messageText.alpha = 0f;
    }

    private IEnumerator Smooth()
    {
        float t = 0f;
        float halfTime = fadeDuration;

        currentAlpha = 0f;

        // FADE IN
        while (t < halfTime)
        {
            t += Time.deltaTime;
            currentAlpha = Mathf.Lerp(0f, 1f, t / halfTime);
            messageText.alpha = currentAlpha;
            yield return null;
        }

        // GÖRÜNÜR KAL
        yield return new WaitForSeconds(totalTimeVisible);

        // FADE OUT
        t = 0f;
        while (t < halfTime)
        {
            t += Time.deltaTime;
            currentAlpha = Mathf.Lerp(1f, 0f, t / halfTime);
            messageText.alpha = currentAlpha;
            yield return null;
        }

        currentAlpha = 0f;
        messageText.alpha = 0f;
    }
}