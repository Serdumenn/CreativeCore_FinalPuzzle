using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KnightQuestionUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button Button_WhoAreYou;
    public Button Button_WhatHappened;
    public Button Button_HowToLeave;
    public Button Button_Close;

    [Header("Panel Root (Optional)")]
    public CanvasGroup panelGroup; // varsa: alpha/interactable/raycast kontrol eder

    [Header("Events")]
    public UnityEvent<int> OnQuestionSelected;   // 0,1,2
    public UnityEvent OnClosePanel;

    private void Awake()
    {
        // Sorular
        if (Button_WhoAreYou != null)     Button_WhoAreYou.onClick.AddListener(() => OnQuestionSelected?.Invoke(0));
        if (Button_WhatHappened != null)  Button_WhatHappened.onClick.AddListener(() => OnQuestionSelected?.Invoke(1));
        if (Button_HowToLeave != null)    Button_HowToLeave.onClick.AddListener(() => OnQuestionSelected?.Invoke(2));

        // Close
        if (Button_Close != null)         Button_Close.onClick.AddListener(() => OnClosePanel?.Invoke());
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (panelGroup != null)
        {
            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    public void SetQuestions(KnightAnswer[] answers)
    {
        // İstersen soruların varlığına göre butonları kapat/aç
        SetButtonActive(Button_WhoAreYou,    answers, 0);
        SetButtonActive(Button_WhatHappened, answers, 1);
        SetButtonActive(Button_HowToLeave,   answers, 2);
    }

    private void SetButtonActive(Button b, KnightAnswer[] answers, int index)
    {
        if (b == null) return;
        bool has = answers != null && index >= 0 && index < answers.Length;

        b.gameObject.SetActive(has);
        // buton textlerini değiştirmek istemiyorsan burada bırak.
        // İstersen TMP_Text ile label set edebilirsin.
    }
}
