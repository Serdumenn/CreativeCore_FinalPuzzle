using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class KnightQuestionUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button button_WhoAreYou;
    public Button button_WhatHappened;
    public Button button_HowToLeave;
    public Button button_Close;

    // Event: KnightDialogueController bu eventleri dinleyecek
    public UnityEvent<int> OnQuestionSelected = new UnityEvent<int>();
    public UnityEvent OnClosePanel = new UnityEvent();

    private void Start()
    {
        // Button click bindings
        button_WhoAreYou.onClick.AddListener(() => SelectQuestion(0));
        button_WhatHappened.onClick.AddListener(() => SelectQuestion(1));
        button_HowToLeave.onClick.AddListener(() => SelectQuestion(2));

        // Close button
        button_Close.onClick.AddListener(() => OnClosePanel.Invoke());

        // Başlangıçta kapalı olsun
        HidePanel();
    }

    private void SelectQuestion(int index)
    {
        OnQuestionSelected.Invoke(index);
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}