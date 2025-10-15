using UnityEngine;

public enum PromptPriority { Info = 0, Interact = 1, Warning = 2 }

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { get; private set; }
    [SerializeField] private MessageUI messageUI;

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
    }

    public void Show(string message, PromptPriority priority = PromptPriority.Info, bool force = false)
    {
        if (messageUI == null)
        {
            Debug.LogWarning("PromptManager: MessageUI reference missing!");
            return;
        }

        if (!force && message == activeMessage && priority == activePriority)
            return;

        if (priority >= activePriority || force)
        {
            activeMessage = message;
            activePriority = priority;
            messageUI.ShowMessage(message, true);
        }
    }

    public void Clear(string message)
    {
        if (message == activeMessage)
        {
            activeMessage = "";
            activePriority = PromptPriority.Info;
        }
    }
}