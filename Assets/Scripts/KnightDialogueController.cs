using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightDialogueController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public PlayerInput playerInput;
    public PlayerController playerController;

    public MessageUI messageUI;
    public KnightQuestionUI questionUI;
    public TMP_Text answerText;
    public KnightAnswerDatabase answerDatabase;

    [Header("Settings")]
    public float interactDistance = 3.5f;
    public string interactPrompt = "Press [E] to speak";

    private InputAction interactAction;
    private bool panelOpen;

    private bool wasCursorVisible;
    private CursorLockMode previousLockMode;
    private string previousActionMap;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            if (playerInput == null) playerInput = player.GetComponent<PlayerInput>();
            if (playerController == null) playerController = player.GetComponent<PlayerController>();
        }

        if (playerInput != null)
            interactAction = playerInput.actions?.FindAction("Interact", throwIfNotFound: false);
    }

    private void OnEnable()
    {
        if (questionUI != null)
        {
            questionUI.QuestionSelected += OnQuestionSelected;
            questionUI.CloseRequested += ClosePanel;
        }

        interactAction?.Enable();
    }

    private void OnDisable()
    {
        if (questionUI != null)
        {
            questionUI.QuestionSelected -= OnQuestionSelected;
            questionUI.CloseRequested -= ClosePanel;
        }

        interactAction?.Disable();
    }

    private void Update()
    {
        if (panelOpen) return;
        if (player == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d > interactDistance) return;

        messageUI?.ShowMessage(interactPrompt);

        if (interactAction != null && interactAction.WasPressedThisFrame())
            OpenPanel();
    }

    private void OpenPanel()
    {
        if (panelOpen) return;
        panelOpen = true;

        CacheCursor();
        UnlockCursor();
        SwitchToUIMap();

        if (playerController != null)
            playerController.enabled = false;

        if (answerText != null)
            answerText.text = string.Empty;

        if (answerDatabase != null && questionUI != null)
            questionUI.SetQuestions(answerDatabase.answers);

        questionUI?.Show();
    }

    private void ClosePanel()
    {
        if (!panelOpen) return;
        panelOpen = false;

        questionUI?.Hide();

        RestoreActionMap();
        RestoreCursor();

        if (playerController != null)
            playerController.enabled = true;
    }

    private void OnQuestionSelected(int index)
    {
        if (answerDatabase == null || answerDatabase.answers == null) return;
        if (answerText == null) return;
        if (index < 0 || index >= answerDatabase.answers.Length) return;

        answerText.text = answerDatabase.answers[index].answer;
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

    private void SwitchToUIMap()
    {
        if (playerInput == null) return;

        previousActionMap = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : null;

        var uiMap = playerInput.actions?.FindActionMap("UI", throwIfNotFound: false);
        if (uiMap != null)
            playerInput.SwitchCurrentActionMap(uiMap.name);
    }

    private void RestoreActionMap()
    {
        if (playerInput == null) return;
        if (!string.IsNullOrEmpty(previousActionMap))
            playerInput.SwitchCurrentActionMap(previousActionMap);
    }
}
