using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightQuestionUI : MonoBehaviour
{
    [Header("Panel Root (CanvasGroup)")]
    public CanvasGroup panel;          // KnightQuestionPanel üzerinde CanvasGroup olmalı

    [Header("Buttons (order = 0,1,2...)")]
    public Button[] questionButtons;   // WhoAreYou, WhatHappened, HowToLeave...
    public Button closeButton;

    public event Action<int> QuestionSelected;
    public event Action CloseRequested;

    private void Awake()
    {
        // Close
        if (closeButton != null)
            closeButton.onClick.AddListener(() => CloseRequested?.Invoke());

        // Questions
        if (questionButtons != null)
        {
            for (int i = 0; i < questionButtons.Length; i++)
            {
                int index = i;
                if (questionButtons[i] != null)
                    questionButtons[i].onClick.AddListener(() => QuestionSelected?.Invoke(index));
            }
        }

        Hide(); // oyun başında kapalı kalsın
    }

    public void SetQuestions(KnightAnswer[] data)
    {
        if (questionButtons == null) return;

        for (int i = 0; i < questionButtons.Length; i++)
        {
            bool hasData = data != null && i < data.Length;

            if (questionButtons[i] == null) continue;
            questionButtons[i].gameObject.SetActive(hasData);

            if (!hasData) continue;

            var label = questionButtons[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = data[i].question;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (panel != null)
        {
            panel.alpha = 1f;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}
