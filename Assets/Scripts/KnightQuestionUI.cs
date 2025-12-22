using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightQuestionUI : MonoBehaviour
{
    [Header("UI References (Optional: Auto-wired if empty)")]
    public CanvasGroup questionPanel;

    [Tooltip("Question buttons. If empty, the script will auto-find buttons under this object whose name contains 'Button_' and is not the Close button.")]
    public Button[] questionButtons;

    [Tooltip("Close button. If empty, the script will auto-find a button whose name contains 'Close'.")]
    public Button closeButton;

    public event Action<int> OnQuestionSelected;
    public event Action OnClose;

    private bool wired;

    private void Awake()
    {
        AutoWireIfNeeded();
        WireButtons();
    }

    private void OnEnable()
    {
        // Some UI setups instantiate/enable late; ensure wiring exists.
        if (!wired)
        {
            AutoWireIfNeeded();
            WireButtons();
        }
    }

    private void AutoWireIfNeeded()
    {
        if (questionPanel == null)
            questionPanel = GetComponentInChildren<CanvasGroup>(true);

        // Auto-find close button if missing
        if (closeButton == null)
        {
            closeButton = FindButtonByNameContains("close");
        }

        // Auto-find question buttons if missing/empty
        if (questionButtons == null || questionButtons.Length == 0)
        {
            var allButtons = GetComponentsInChildren<Button>(true);
            var list = new List<Button>();

            foreach (var b in allButtons)
            {
                if (b == null) continue;

                string n = b.name.ToLowerInvariant();

                // Close button excluded
                if (n.Contains("close")) continue;

                // Pick question buttons by common naming patterns:
                // Button_HowToLeave, Button_WhoAreYou, etc.
                if (n.Contains("button_") || n.Contains("question"))
                    list.Add(b);
            }

            questionButtons = list.ToArray();
        }

        Debug.Log($"[KnightQuestionUI] AutoWire: panel={(questionPanel ? questionPanel.name : "NULL")}, close={(closeButton ? closeButton.name : "NULL")}, questionButtons={(questionButtons == null ? -1 : questionButtons.Length)}");
        if (questionButtons != null)
        {
            for (int i = 0; i < questionButtons.Length; i++)
                Debug.Log($"[KnightQuestionUI] AutoWire: questionButtons[{i}]={(questionButtons[i] ? questionButtons[i].name : "NULL")}");
        }
    }

    private Button FindButtonByNameContains(string containsLower)
    {
        var allButtons = GetComponentsInChildren<Button>(true);
        foreach (var b in allButtons)
        {
            if (b == null) continue;
            if (b.name.ToLowerInvariant().Contains(containsLower))
                return b;
        }
        return null;
    }

    private void WireButtons()
    {
        // Prevent double-wiring
        if (wired) return;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }
        else
        {
            Debug.LogError("[KnightQuestionUI] closeButton is NULL. Close will not work.");
        }

        if (questionButtons == null || questionButtons.Length == 0)
        {
            Debug.LogError("[KnightQuestionUI] questionButtons is empty. No question can be selected.");
        }
        else
        {
            for (int i = 0; i < questionButtons.Length; i++)
            {
                int capturedIndex = i;
                var b = questionButtons[i];
                if (b == null)
                {
                    Debug.LogError($"[KnightQuestionUI] questionButtons[{i}] is NULL.");
                    continue;
                }

                // Remove any previous listener we added (safe even if none)
                b.onClick.RemoveListener(() => HandleQuestionClicked(capturedIndex));
                // Add listener
                b.onClick.AddListener(() => HandleQuestionClicked(capturedIndex));

                Debug.Log($"[KnightQuestionUI] Wired question button '{b.name}' -> index {capturedIndex}");
            }
        }

        wired = true;
    }

    private void HandleQuestionClicked(int index)
    {
        Debug.Log($"[KnightQuestionUI] Question clicked index={index}");
        OnQuestionSelected?.Invoke(index);
    }

    private void HandleCloseClicked()
    {
        Debug.Log("[KnightQuestionUI] Close clicked");
        OnClose?.Invoke();
    }

    public void SetQuestions(KnightAnswer[] data)
    {
        // Optional: If you are not using DB right now, you can ignore this.
        if (questionButtons == null) return;

        for (int i = 0; i < questionButtons.Length; i++)
        {
            bool hasData = data != null && i < data.Length;
            questionButtons[i].gameObject.SetActive(hasData);

            if (!hasData) continue;

            TMP_Text label = questionButtons[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = data[i].question;
        }
    }

    public void Show()
    {
        if (questionPanel != null)
        {
            questionPanel.alpha = 1f;
            questionPanel.interactable = true;
            questionPanel.blocksRaycasts = true;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (questionPanel != null)
        {
            questionPanel.alpha = 0f;
            questionPanel.interactable = false;
            questionPanel.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}