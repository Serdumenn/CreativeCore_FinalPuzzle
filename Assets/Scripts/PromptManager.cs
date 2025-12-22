// Assets/Scripts/PromptManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { get; private set; }

    [Header("UI (Prompt)")]
    [Tooltip("Prompt text object (separate from MessageUI.messageText)")]
    public TMP_Text promptText;

    [Tooltip("Optional: root GameObject to enable/disable")]
    public GameObject promptRoot;

    private class Entry
    {
        public string id;
        public string message;
        public PromptPriority priority;
        public float lastSetTime;
    }

    private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
    private string _activeId = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ApplyToUI(null);
    }

    // --- New / Preferred API ---
    public void Show(string message, string id, PromptPriority priority = PromptPriority.Info, bool force = false)
    {
        if (string.IsNullOrEmpty(id))
            id = message; // fallback

        if (string.IsNullOrEmpty(message))
        {
            Clear(id);
            return;
        }

        if (!_entries.TryGetValue(id, out var e))
        {
            e = new Entry { id = id };
            _entries[id] = e;
        }

        // If same and not forced, don't churn
        if (!force && e.message == message && e.priority == priority)
        {
            // still refresh selection in case active changed elsewhere
            Refresh();
            return;
        }

        e.message = message;
        e.priority = priority;
        e.lastSetTime = Time.unscaledTime;

        Refresh();
    }

    public void Clear(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (_entries.Remove(id))
        {
            if (_activeId == id) _activeId = null;
            Refresh();
        }
    }

    public void ClearAll()
    {
        _entries.Clear();
        _activeId = null;
        ApplyToUI(null);
    }

    // --- Legacy API (keeps older calls compiling) ---
    public void Show(string message, PromptPriority priority = PromptPriority.Info, bool force = false)
    {
        // legacy used message as key
        Show(message, message, priority, force);
    }

    private void Refresh()
    {
        if (_entries.Count == 0)
        {
            ApplyToUI(null);
            return;
        }

        var best = _entries.Values
            .OrderByDescending(x => (int)x.priority)
            .ThenByDescending(x => x.lastSetTime)
            .FirstOrDefault();

        ApplyToUI(best);
    }

    private void ApplyToUI(Entry best)
    {
        if (promptText == null)
        {
            // Fail loudly once; otherwise silent.
            return;
        }

        if (best == null || string.IsNullOrEmpty(best.message))
        {
            promptText.text = "";
            if (promptRoot != null) promptRoot.SetActive(false);
            return;
        }

        _activeId = best.id;
        promptText.text = best.message;
        if (promptRoot != null) promptRoot.SetActive(true);
    }
}