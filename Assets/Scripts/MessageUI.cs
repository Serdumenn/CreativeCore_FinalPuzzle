using UnityEngine;
using TMPro;
using System.Collections;

public class MessageUI : MonoBehaviour
{
    public TMP_Text label;
    public float holdSeconds = 1.6f;
    private Coroutine co;

    public void ShowOnce(string msg)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Run(msg));
    }

    private IEnumerator Run(string msg)
    {
        label.text = msg;
        label.alpha = 1f;
        yield return new WaitForSeconds(holdSeconds);
        label.alpha = 0f;
    }

    // Instant show/hide (for persistent prompts)
    public void ShowMessage(string msg)
    {
        if (label == null) return;
        label.text = msg;
        label.alpha = 1f;
    }

    public void HideMessage()
    {
        if (label == null) return;
        label.alpha = 0f;
        label.text = "";
    }

    public void Hide()
    {
        HideMessage();
    }
}
