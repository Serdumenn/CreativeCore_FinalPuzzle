using UnityEngine;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { get; private set; }

    [Header("References")]
    public MessageUI messageUI;

    private string activeMessage = "";
    private PromptPriority activePriority = PromptPriority.Info;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (messageUI == null)
            messageUI = FindFirstObjectByType<MessageUI>();
    }

    /// <summary>Persistent prompt gösterir (flicker engeller). Öncelik yüksekse ezer.</summary>
    public void Show(string message, PromptPriority priority = PromptPriority.Info, bool force = false)
    {
        if (messageUI == null) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        if (!force && message == activeMessage && priority == activePriority)
            return;

        if (force || priority >= activePriority)
        {
            activeMessage = message;
            activePriority = priority;
            messageUI.ShowPrompt(message);
        }
    }

    public void Clear(string message)
    {
        if (messageUI == null) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        if (message == activeMessage)
        {
            messageUI.ClearPrompt(message);
            activeMessage = "";
            activePriority = PromptPriority.Info;
        }
    }
}