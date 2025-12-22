// Assets/Scripts/KnightDialogueController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class KnightDialogueController : MonoBehaviour
{
    [Header("References")]
    public KnightQuestionUI questionUI;
    public TMP_Text answerText; // optional
    public MessageUI messageUI;
    public KnightAnswerDatabase answerDatabase;
    public PlayerInput playerInput;
    public PlayerController playerController;

    [Header("Settings")]
    public string interactPrompt = "Press [E] to speak";
    public float interactDistance = 3.5f;
    public Transform player;
    [Min(0f)] public float answerDisplaySeconds = 1.5f;
    [Min(0f)] public float endDelaySeconds = 4f;
    [TextArea] public string finalMessage = "Now I will send you...";

    private InputAction interactAction;
    private bool panelOpen;
    private bool wasCursorVisible;
    private CursorLockMode previousLockMode;
    private string currentActionMap;
    private bool warnedMissingInteract;
    private bool isEnding;
    private Coroutine endingCo;

    private string promptId;

    private void Awake()
    {
        promptId = $"KnightPrompt:{GetInstanceID()}";

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (playerInput == null && player != null)
            playerInput = player.GetComponent<PlayerInput>();

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (playerInput != null)
        {
            interactAction = playerInput.actions?.FindAction("Interact", throwIfNotFound: false);
            interactAction?.Enable();
            currentActionMap = playerInput.currentActionMap?.name;

            if (interactAction == null && !warnedMissingInteract)
            {
                Debug.LogWarning("[KnightDialogue] Interact action not found. Check Input Action Asset for an action named 'Interact'.");
                warnedMissingInteract = true;
            }
        }
        else
        {
            Debug.LogWarning("[KnightDialogue] PlayerInput not assigned; UI will not open.");
        }
    }

    private void OnEnable()
    {
        if (questionUI != null)
        {
            questionUI.OnQuestionSelected += HandleQuestionSelected;
            questionUI.OnClose += ClosePanel;
        }
    }

    private void OnDisable()
    {
        if (questionUI != null)
        {
            questionUI.OnQuestionSelected -= HandleQuestionSelected;
            questionUI.OnClose -= ClosePanel;
        }

        if (interactAction != null)
            interactAction.Disable();

        PromptManager.Instance?.Clear(promptId);
    }

    private void Update()
    {
        if (panelOpen || isEnding) return;
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance > interactDistance)
        {
            PromptManager.Instance?.Clear(promptId);
            return;
        }

        PromptManager.Instance?.Show(interactPrompt, promptId, PromptPriority.Critical);

        if (interactAction == null)
        {
            if (!warnedMissingInteract)
            {
                Debug.LogWarning("[KnightDialogue] Interact action is null; cannot open dialogue.");
                warnedMissingInteract = true;
            }
            return;
        }

        if (!interactAction.enabled)
            interactAction.Enable();

        if (interactAction.WasPerformedThisFrame())
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

        if (answerDatabase != null && questionUI != null)
            questionUI.SetQuestions(answerDatabase.answers);

        questionUI?.Show();

        if (answerText != null)
            answerText.text = "";
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
        if (answerDatabase == null) return;
        if (answerDatabase.answers == null) return;
        if (index < 0 || index >= answerDatabase.answers.Length) return;
        if (isEnding) return;

        ClosePanel(restorePlayer: false);

        string answer = answerDatabase.answers[index].answer;

        if (answerText != null)
            answerText.text = answer;

        if (endingCo != null)
            StopCoroutine(endingCo);

        endingCo = StartCoroutine(EndSequence(answer));
    }

    private System.Collections.IEnumerator EndSequence(string answer)
    {
        isEnding = true;

        if (playerController != null)
            playerController.enabled = false;

        if (playerInput != null)
            playerInput.DeactivateInput();

        UnlockCursor();

        // 1) Answer timed message
        if (messageUI != null)
            messageUI.ShowTimed(answer, answerDisplaySeconds, force: true);

        if (answerDisplaySeconds > 0f)
            yield return new WaitForSecondsRealtime(answerDisplaySeconds);

        // 2) Final timed message
        if (messageUI != null)
            messageUI.ShowTimed(finalMessage, endDelaySeconds, force: true);

        if (endDelaySeconds > 0f)
            yield return new WaitForSecondsRealtime(endDelaySeconds);

        SceneManager.LoadScene("MenuScene");
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