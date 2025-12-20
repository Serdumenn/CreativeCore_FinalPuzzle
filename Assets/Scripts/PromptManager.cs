using System.Collections.Generic;
using UnityEngine;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { get; private set; }

    [Header("References")]
    public MessageUI messageUI;

    // We keep prompts by key, and select the highest priority to show.
    private readonly Dictionary<string, (string msg, PromptPriority pr)> prompts = new();
    private string activeKey = null;
    private string activeMsg = "";
    private PromptPriority activePr = PromptPriority.Info;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(string key, string message, PromptPriority priority = PromptPriority.Info)
    {
        if (messageUI == null) return;
        if (string.IsNullOrEmpty(key)) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        prompts[key] = (message, priority);
        RecomputeAndDisplay();
    }

    public void Clear(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (prompts.Remove(key))
            RecomputeAndDisplay();
    }

    // One-off timed override message (does not replace stored prompts)
    public void OverrideTimed(string message, bool force = true)
    {
        if (messageUI == null) return;
        messageUI.ShowTimed(message, force);
    }

    private void RecomputeAndDisplay()
    {
        if (messageUI == null) return;

        string bestKey = null;
        string bestMsg = "";
        PromptPriority bestPr = PromptPriority.Info;
        bool found = false;

        foreach (var kv in prompts)
        {
            var (msg, pr) = kv.Value;
            if (!found || pr > bestPr)
            {
                found = true;
                bestKey = kv.Key;
                bestMsg = msg;
                bestPr = pr;
            }
        }

        if (!found)
        {
            activeKey = null;
            activeMsg = "";
            activePr = PromptPriority.Info;
            messageUI.ClearPrompt();
            return;
        }

        // If nothing changed, do nothing
        if (activeKey == bestKey && activeMsg == bestMsg && activePr == bestPr)
            return;

        activeKey = bestKey;
        activeMsg = bestMsg;
        activePr = bestPr;

        messageUI.ShowPrompt(activeMsg);
    }
}