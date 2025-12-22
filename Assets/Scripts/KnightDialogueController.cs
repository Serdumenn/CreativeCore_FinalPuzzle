using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class KnightDialogueController : MonoBehaviour
{
    [Header("References")]
    public KnightQuestionUI questionUI;
    public MessageUI messageUI;
    public PlayerInput playerInput;
    public PlayerController playerController;

    [Header("3D Voice (AudioSource on Knight)")]
    public AudioSource voiceSource;

    [Tooltip("Plays when panel opens (E pressed).")]
    public AudioClip greetingClip;

    [Tooltip("Plays when question is selected.")]
    public AudioClip finalClip;

    [Header("Settings")]
    public string interactPrompt = "Press [E] to speak";
    public float interactDistance = 3.5f;
    public Transform player;

    [Min(0f)] public float answerDisplaySeconds = 7.2f;
    [Min(0f)] public float endDelaySeconds = 0.0f;
    public string answerLine = "Now I am sending you on your way, traveler. For this is your destiny.";

    private InputAction interactAction;
    private bool panelOpen;
    private bool isEnding;

    private bool wasCursorVisible;
    private CursorLockMode previousLockMode;
    private string currentActionMap;

    private Coroutine endingCo;
    private string promptId;

    private void Awake()
    {
        promptId = $"KnightPrompt:{GetInstanceID()}";

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerInput == null && player != null)
            playerInput = player.GetComponent<PlayerInput>();

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (playerInput != null)
        {
            interactAction = playerInput.actions?.FindAction("Interact", throwIfNotFound: false);
            interactAction?.Enable();
            currentActionMap = playerInput.currentActionMap?.name;
        }
        else
        {
            Debug.LogWarning("[KnightDialogue] PlayerInput not assigned.");
        }

        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();

        if (voiceSource == null)
            Debug.LogWarning("[KnightDialogue] voiceSource missing. Add an AudioSource to Knight or assign one.");
    }

    private void OnEnable()
    {
        if (questionUI != null)
        {
            questionUI.OnQuestionSelected += HandleQuestionSelected;
            questionUI.OnClose += ClosePanel;
        }
        else
        {
            Debug.LogWarning("[KnightDialogue] questionUI reference missing.");
        }
    }

    private void OnDisable()
    {
        if (questionUI != null)
        {
            questionUI.OnQuestionSelected -= HandleQuestionSelected;
            questionUI.OnClose -= ClosePanel;
        }

        PromptManager.Instance?.Clear(promptId);
    }

    private void Update()
    {
        if (panelOpen || isEnding) return;
        if (player == null || interactAction == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance > interactDistance)
        {
            PromptManager.Instance?.Clear(promptId);
            return;
        }

        PromptManager.Instance?.Show(interactPrompt, promptId, PromptPriority.Critical);

        if (interactAction.WasPressedThisFrame())
            OpenPanel();
    }

    private void OpenPanel()
    {
        if (panelOpen) return;
        panelOpen = true;

        PromptManager.Instance?.Clear(promptId);

        CacheCursor();
        UnlockCursor();
        EnableUIInput();

        if (playerController != null)
            playerController.enabled = false;

        PlayVoice(greetingClip);

        questionUI?.Show();
    }

    private void ClosePanel()
    {
        ClosePanel(true);
    }

    private void ClosePanel(bool restorePlayer)
    {
        if (!panelOpen) return;
        panelOpen = false;

        RestoreCursor();

        if (restorePlayer)
            RestorePlayerInput();

        if (playerController != null && restorePlayer)
            playerController.enabled = true;

        questionUI?.Hide();
    }

    private void HandleQuestionSelected(int index)
    {
        Debug.Log($"[KnightDialogue] Question selected (single-button mode). index={index}");

        if (isEnding) return;

        ClosePanel(restorePlayer: false);

        if (endingCo != null)
            StopCoroutine(endingCo);

        endingCo = StartCoroutine(EndSequence());
    }

    private System.Collections.IEnumerator EndSequence()
    {
        isEnding = true;

        if (playerController != null)
            playerController.enabled = false;

        if (playerInput != null)
            playerInput.DeactivateInput();

        UnlockCursor();

        PlayVoice(finalClip, stopFirst: true);

        if (messageUI != null)
            messageUI.ShowPersistent(answerLine, force: true);
        else
            Debug.LogWarning("[KnightDialogue] messageUI missing; cannot show answer.");

        if (answerDisplaySeconds > 0f)
            yield return new WaitForSecondsRealtime(answerDisplaySeconds);

        if (endDelaySeconds > 0f)
            yield return new WaitForSecondsRealtime(endDelaySeconds);

        SceneManager.LoadScene("MenuScene");
    }

    private void PlayVoice(AudioClip clip, bool stopFirst = false)
    {
        if (voiceSource == null || clip == null) return;

        if (stopFirst && voiceSource.isPlaying)
            voiceSource.Stop();

        voiceSource.PlayOneShot(clip);
    }

    private void CacheCursor()
    {
        wasCursorVisible = Cursor.visible;
        previousLockMode = Cursor.lockState;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        Cursor.lockState = previousLockMode;
        Cursor.visible = wasCursorVisible;
    }

    private void EnableUIInput()
    {
        if (playerInput == null) return;

        currentActionMap = playerInput.currentActionMap?.name;

        var uiMap = playerInput.actions?.FindActionMap("UI", throwIfNotFound: false);
        if (uiMap != null)
            playerInput.SwitchCurrentActionMap(uiMap.name);
    }

    private void RestorePlayerInput()
    {
        if (playerInput == null) return;
        if (!string.IsNullOrEmpty(currentActionMap))
            playerInput.SwitchCurrentActionMap(currentActionMap);
    }
}