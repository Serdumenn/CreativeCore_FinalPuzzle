using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { get; private set; }

    [Header("UI (Prompt)")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject promptRoot;

    private class Entry
    {
        public string id;
        public string message;
        public PromptPriority priority;
        public float lastSetTime;
    }

    private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ApplyUI(null);
    }

    // Preferred API
    public void Show(string message, string id, PromptPriority priority = PromptPriority.Info, bool force = false)
    {
        if (string.IsNullOrEmpty(id))
            id = message;

        if (string.IsNullOrEmpty(message))
        {
            Clear(id);
            return;
        }

        if (!entries.TryGetValue(id, out var e))
        {
            e = new Entry { id = id };
            entries[id] = e;
        }

        if (!force && e.message == message && e.priority == priority)
        {
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

        if (entries.Remove(id))
            Refresh();
    }

    public void ClearAll()
    {
        entries.Clear();
        ApplyUI(null);
    }

    // Legacy overloads (repo’daki mevcut çağrıları bozmamak için)
    public void Show(string message, PromptPriority priority = PromptPriority.Info, bool force = false)
        => Show(message, message, priority, force);

    public void ClearMessageKey(string message)
        => Clear(message);

    private void Refresh()
    {
        if (entries.Count == 0)
        {
            ApplyUI(null);
            return;
        }

        var best = entries.Values
            .OrderByDescending(x => (int)x.priority)
            .ThenByDescending(x => x.lastSetTime)
            .FirstOrDefault();

        ApplyUI(best);
    }

    private void ApplyUI(Entry e)
    {
        if (promptText == null)
        {
            // UI bağlanmadıysa sessizce çık (Console spam istemiyoruz)
            return;
        }

        if (e == null || string.IsNullOrEmpty(e.message))
        {
            promptText.text = "";
            if (promptRoot != null) promptRoot.SetActive(false);
            return;
        }

        promptText.text = e.message;
        if (promptRoot != null) promptRoot.SetActive(true);
    }
}