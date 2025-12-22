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
    public SacredGateController gateController; // optional (recommend)

    [Header("Settings")]
    public string interactPrompt = "Press [E] to speak";
    public float interactDistance = 3.5f;
    public Transform player;
    [Min(0f)] public float answerDisplaySeconds = 2.5f;
    [Min(0f)] public float endDelaySeconds = 4.5f;
    [TextArea] public string finalMessage = "Now I will send you...";
    [TextArea] public string lockedMessage = "Prove yourself first. Ignite the Sacred Sword and unlock the gate.";

    private InputAction interactAction;
    private bool panelOpen;
    private bool wasCursorVisible;
    private CursorLockMode previousLockMode;
    private string currentActionMap;
    private bool isEnding;
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

        // Gate şartı (önerilen)
        if (gateController != null && !gateController.IsUnlocked)
        {
            PromptManager.Instance?.Show(lockedMessage, promptId, PromptPriority.Warning);
            // istersek E basınca sadece timed bir uyarı da gösterebiliriz
            if (interactAction.WasPressedThisFrame() && messageUI != null)
                messageUI.ShowTimed(lockedMessage, 2.0f, force: true);
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

        if (answerDatabase != null && questionUI != null)
            questionUI.SetQuestions(answerDatabase.answers);

        questionUI?.Show();

        if (answerText != null)
            answerText.text = string.Empty;
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
        if (answerDatabase == null || answerDatabase.answers == null) return;
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

        if (messageUI != null)
            messageUI.ShowTimed(answer, answerDisplaySeconds, force: true);

        if (answerDisplaySeconds > 0f)
            yield return new WaitForSecondsRealtime(answerDisplaySeconds);

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