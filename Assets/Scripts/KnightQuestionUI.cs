// Assets/Scripts/KnightQuestionUI.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightQuestionUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup questionPanel;
    public Button[] questionButtons;
    public Button closeButton;

    public event Action<int> OnQuestionSelected;
    public event Action OnClose;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => OnClose?.Invoke());

        if (questionButtons != null)
        {
            for (int i = 0; i < questionButtons.Length; i++)
            {
                int capturedIndex = i;
                if (questionButtons[i] != null)
                    questionButtons[i].onClick.AddListener(() => OnQuestionSelected?.Invoke(capturedIndex));
            }
        }
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