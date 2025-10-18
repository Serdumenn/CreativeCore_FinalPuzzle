using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Refs")]
    public ScreenFader screenFader;   // Hierarchy: ScreenFader (CanvasGroup'lu)
    public GameObject creditsPanel;   // CreditsPanel root (SetActive=false başlar)
    public Button playButton;         // Play UI Button (opsiyonel: null kalabilir)
    public Button creditsButton;      // Credits UI Button (opsiyonel)
    public Button soundButton;        // Sound UI Button (opsiyonel)

    [Header("Config")]
    public string gameSceneName = "MainScene";

    bool soundOn = true;

    void Start()
    {
        // Menüde imleci geri aç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (creditsPanel != null) creditsPanel.SetActive(false);
        // Başlangıçta kısa bir fade-in istersen:
        if (screenFader != null) StartCoroutine(screenFader.FadeIn());
    }

    // --- BUTTON HOOKS ---
    public void PlayGame()
    {
        if (screenFader != null)
            StartCoroutine(LoadGameWithFade());
        else
            SceneManager.LoadScene(gameSceneName);
    }

    public void ToggleCredits()
    {
        if (creditsPanel == null) return;
        bool active = creditsPanel.activeSelf;
        creditsPanel.SetActive(!active);
    }

    public void HideCredits()
    {
        if (creditsPanel == null) return;
        creditsPanel.SetActive(false);
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;
        AudioListener.volume = soundOn ? 1f : 0f;
        // Buton üstündeki yazıyı değiştirmek istiyorsan:
        // var txt = soundButton?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        // if (txt) txt.text = soundOn ? "SOUND" : "MUTED";
    }

    // --- HELPERS ---
    System.Collections.IEnumerator LoadGameWithFade()
    {
        yield return screenFader.FadeOut();
        SceneManager.LoadScene(gameSceneName);
    }
}
