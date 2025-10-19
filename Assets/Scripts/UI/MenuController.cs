using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menü butonlarını ve krediler panelini yönetir.
/// Play: FadeOut → MainScene yükle
/// Credits: paneli CanvasGroup ile fade in/out
/// Sound: AudioListener.pause toggle (şimdilik basit)
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Play'e basınca yüklenecek sahne adı")]
    public string mainSceneName = "MainScene";

    [Header("References")]
    public ScreenFader screenFader;          // MainMenuUI > FadePanel üzerinde olmalı
    public CanvasGroup creditsPanel;         // CreditsPanel (CanvasGroup zorunlu)
    public Button playButton;
    public Button creditsButton;
    public Button closeButton;               // Credits içindeki Back
    public Button soundButton;               // İsteğe bağlı

    [Header("Timings")]
    [Min(0.05f)] public float fadeDuration = 0.6f;
    [Min(0.05f)] public float creditsFade = 0.35f;

    // iç durum
    bool creditsVisible = false;

    private void Reset()
    {
        // Inspector’da eklerken küçük kalite-of-life: CreditsPanel’de CanvasGroup olsun
        if (creditsPanel == null)
        {
            var go = GameObject.Find("CreditsPanel");
            if (go) creditsPanel = go.GetComponent<CanvasGroup>();
        }
        if (screenFader == null)
        {
            var go = GameObject.Find("FadePanel");
            if (go) screenFader = go.GetComponent<ScreenFader>();
        }
    }

    private void Awake()
    {
        // Credits paneli görünmez başlasın
        if (creditsPanel != null)
        {
            creditsPanel.alpha = 0f;
            creditsPanel.interactable = false;
            creditsPanel.blocksRaycasts = false;
        }

        // Butonları otomatik bağla (Inspector’da bağlıysa yine de üstüne yazmaz)
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayGame);
            playButton.onClick.AddListener(PlayGame);
        }
        if (creditsButton != null)
        {
            creditsButton.onClick.RemoveListener(ToggleCredits);
            creditsButton.onClick.AddListener(ToggleCredits);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HideCredits);
            closeButton.onClick.AddListener(HideCredits);
        }
        if (soundButton != null)
        {
            soundButton.onClick.RemoveListener(ToggleSound);
            soundButton.onClick.AddListener(ToggleSound);
        }
    }

    // ============== Public API (Inspector OnClick’te görünsün diye) ==============

    public void PlayGame()
    {
        // Çift tıklama vs. için tekrar tetiklenmesin
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(PlayRoutine());
    }

    public void ToggleCredits()
    {
        if (creditsVisible) HideCredits();
        else ShowCredits();
    }

    public void ShowCredits()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(CreditsRoutine(show: true));
    }

    public void HideCredits()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(CreditsRoutine(show: false));
    }

    public void ToggleSound()
    {
        AudioListener.pause = !AudioListener.pause;
        // İleride buton ikonunu güncellemek istersen burada yapabilirsin.
    }

    // ========================== Coroutines ==========================

    private IEnumerator PlayRoutine()
    {
        // Menüden oyuna geçerken ekrana siyah bindir
        if (screenFader != null)
            yield return screenFader.FadeOut(fadeDuration);

        // Sahneyi yükle
        SceneManager.LoadScene(mainSceneName);
    }

    private IEnumerator CreditsRoutine(bool show)
    {
        if (creditsPanel == null)
            yield break;

        float start = creditsPanel.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        // Etkileşim kilidi
        creditsPanel.interactable = false;
        creditsPanel.blocksRaycasts = true;  // arka planı kilitle

        while (t < creditsFade)
        {
            t += Time.unscaledDeltaTime;
            creditsPanel.alpha = Mathf.Lerp(start, end, t / creditsFade);
            yield return null;
        }
        creditsPanel.alpha = end;

        creditsVisible = show;
        creditsPanel.interactable = show;
        creditsPanel.blocksRaycasts = show;
    }
}
