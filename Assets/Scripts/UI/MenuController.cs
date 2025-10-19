using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ana menü davranışı:
/// - Play: FadeOut → MainScene yükle
/// - Credits: CanvasGroup ile fade in/out
/// - Sound: AudioListener.pause toggle
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Play'e basınca yüklenecek sahnenin adı (Build Settings'e ekli olmalı).")]
    public string mainSceneName = "MainScene";

    [Header("References")]
    [SerializeField] private ScreenFader screenFader;   // MainMenuUI > FadePanel üzerinde
    [SerializeField] private CanvasGroup creditsPanel;  // CreditsPanel (CanvasGroup zorunlu)
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button closeButton;        // Credits içindeki Back/Close
    [SerializeField] private Button soundButton;        // Opsiyonel

    [Header("Timings")]
    [Min(0.05f)] public float fadeDuration = 0.6f;
    [Min(0.05f)] public float creditsFade  = 0.35f;

    // state
    private bool creditsVisible = false;
    private bool isTransitioning = false;
    private Coroutine creditsRoutine;

    private void Reset()
    {
        // KOLAY BAĞLANTI: İsimden bulmayı dener
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
        CacheReferences();
        ValidateConfiguration();

        // Credits paneli görünmez başlat
        SetCreditsState(visible: false, instant: true);

        // Butonları güvenle bağla (önce varsa eski dinleyiciyi kaldır)
        if (playButton)
        {
            playButton.onClick.RemoveListener(PlayGame);
            playButton.onClick.AddListener(PlayGame);
        }
        if (creditsButton)
        {
            creditsButton.onClick.RemoveListener(ToggleCredits);
            creditsButton.onClick.AddListener(ToggleCredits);
        }
        if (closeButton)
        {
            closeButton.onClick.RemoveListener(HideCredits);
            closeButton.onClick.AddListener(HideCredits);
        }
        if (soundButton)
        {
            soundButton.onClick.RemoveListener(ToggleSound);
            soundButton.onClick.AddListener(ToggleSound);
        }
    }

    // -------- Public API (Inspector’dan seçilebilsin) --------
    public void PlayGame()
    {
        if (!isActiveAndEnabled || isTransitioning) return;
        StartCoroutine(PlayRoutine());
    }

    public void ToggleCredits()
    {
        if (!isActiveAndEnabled) return;
        StartCreditsRoutine(show: !creditsVisible);
    }

    public void ShowCredits()
    {
        if (!isActiveAndEnabled) return;
        StartCreditsRoutine(show: true);
    }

    public void HideCredits()
    {
        if (!isActiveAndEnabled) return;
        StartCreditsRoutine(show: false);
    }

    public void ToggleSound()
    {
        AudioListener.pause = !AudioListener.pause;
    }

    // ---------------- Coroutines ----------------

    private IEnumerator PlayRoutine()
    {
        isTransitioning = true;
        SetMenuInteractable(false);

        if (screenFader)
            yield return screenFader.FadeOut(fadeDuration);

        SceneManager.LoadScene(mainSceneName);
    }

    private void StartCreditsRoutine(bool show)
    {
        if (creditsRoutine != null) StopCoroutine(creditsRoutine);
        creditsRoutine = StartCoroutine(CreditsRoutine(show));
    }

    private IEnumerator CreditsRoutine(bool show)
    {
        if (!creditsPanel) yield break;

        float start = creditsPanel.alpha;
        float end   = show ? 1f : 0f;
        float t     = 0f;

        // geçişte arka planı kilitle
        creditsPanel.interactable   = false;
        creditsPanel.blocksRaycasts = true;

        while (t < creditsFade)
        {
            t += Time.unscaledDeltaTime;
            creditsPanel.alpha = Mathf.Lerp(start, end, t / creditsFade);
            yield return null;
        }
        creditsPanel.alpha = end;

        creditsVisible = show;
        creditsPanel.interactable   = show;
        creditsPanel.blocksRaycasts = show;
        creditsRoutine = null;
    }

    // ---------------- Helpers ----------------

    private void SetCreditsState(bool visible, bool instant)
    {
        creditsVisible = visible;
        if (!creditsPanel) return;

        creditsPanel.alpha         = visible ? 1f : 0f;
        creditsPanel.interactable  = visible;
        creditsPanel.blocksRaycasts= visible;

        if (!instant)
            StartCreditsRoutine(visible);
    }

    private void SetMenuInteractable(bool value)
    {
        if (playButton)    playButton.interactable    = value;
        if (creditsButton) creditsButton.interactable = value;
        if (closeButton)   closeButton.interactable   = value;
        if (soundButton)   soundButton.interactable   = value;
    }

    private void CacheReferences()
    {
        if (!screenFader)
            screenFader = GetComponentInChildren<ScreenFader>(includeInactive: true);

        if (!creditsPanel)
        {
            foreach (var g in GetComponentsInChildren<CanvasGroup>(includeInactive: true))
            {
                if (g.gameObject.name.ToLower().Contains("credit"))
                {
                    creditsPanel = g;
                    break;
                }
            }
        }

        if (!playButton || !creditsButton || !closeButton || !soundButton)
        {
            foreach (var b in GetComponentsInChildren<Button>(includeInactive: true))
            {
                var n = b.name.ToLower();
                if (!playButton    && n.Contains("play"))                    playButton = b;
                else if (!creditsButton && n.Contains("credits") && !n.Contains("close")) creditsButton = b;
                else if (!closeButton   && (n.Contains("close") || n.Contains("back")))   closeButton = b;
                else if (!soundButton   && n.Contains("sound"))              soundButton = b;
            }
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(mainSceneName))
            Debug.LogWarning("[MenuController] Main scene name is empty.", this);

        if (!screenFader)
            Debug.LogWarning("[MenuController] ScreenFader reference is missing.", this);
        else if (!screenFader.TryGetComponent(out CanvasGroup _))
            Debug.LogError("[MenuController] ScreenFader requires a CanvasGroup.", screenFader);

        if (!creditsPanel)
            Debug.LogWarning("[MenuController] Credits CanvasGroup is missing.", this);

        if (!playButton || !creditsButton || !closeButton)
            Debug.LogWarning("[MenuController] One or more buttons are not assigned.", this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();
        ValidateConfiguration();

        if (!Application.isPlaying && creditsPanel)
            SetCreditsState(visible: creditsVisible, instant: true);
    }
#endif
}
