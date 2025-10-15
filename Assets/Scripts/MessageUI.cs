using UnityEngine;
using TMPro;
using System.Collections;

public class MessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float fadeDuration = 1f;

    private string currentMessage = "";
    private Coroutine fadeCoroutine;

    public void ShowMessage(string newMessage, bool forceRestart = false)
    {
        if (messageText == null)
        {
            Debug.LogWarning("MessageUI: messageText reference is missing!");
            return;
        }

        if (!forceRestart && newMessage == currentMessage)
            return;

        currentMessage = newMessage;
        messageText.text = newMessage;
        messageText.alpha = 1f;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutAfterDelay());
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            messageText.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        messageText.text = "";
        currentMessage = "";
        fadeCoroutine = null;
    }
}