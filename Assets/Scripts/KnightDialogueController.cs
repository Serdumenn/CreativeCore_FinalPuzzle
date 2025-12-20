using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class KnightDialogueController : MonoBehaviour
{
    [Header("References")]
    public KnightQuestionUI questionUI;
    public TMP_Text answerText;                 // optional: can be null
    public MessageUI messageUI;                 // prompt + final messages
    public KnightAnswerDatabase answerDatabase;
    public PlayerInput playerInput;
    public PlayerController playerController;

    [Header("Settings")]
    public string interactPrompt = "Press [E] to speak";
    public float interactDistance = 3.5f;

    [Min(0f)] public float answerDisplaySeconds = 1.5f;
    [Min(0f)] public float endDelaySeconds = 3.0f;
    [TextArea] public string finalMessage = "Now I will send you...";

    [Header("Scene")]
    public string menuSceneName = "MenuScene";

    private Transform player;
    private InputAction interactAction;
    private bool panelOpen;
    private bool isEnding;
    private string previousActionMap;
    private bool wasCursorVisible;
    private CursorLockMode previousLockMode;
    private Coroutine endCo;

    private const string PromptKey = "KNIGHT_INTERACT";

    private void Awake()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;

        if (player != null)
        {
            if (playerInput == null) playerInput = player.GetComponent<PlayerInput>();
            if (playerController == null) playerController = player.GetComponent<PlayerController>();
        }

        if (playerInput != null)
        {
            interactAction = playerInput.actions?.FindAction("Interact", false);
            interactAction?.Enable();
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

        PromptManager.Instance?.Clear(PromptKey);
    }

    private void Update()
    {
        if (isEnding || panelOpen) return;
        if (player == null) return;

        float d = Vector3.Distance(player.position, transform.position);

        if (d > interactDistance)
        {
            PromptManager.Instance?.Clear(PromptKey);
            return;
        }

        // Show stable prompt (no flicker)
        PromptManager.Instance?.Show(PromptKey, interactPrompt, PromptPriority.Interact);

        if (interactAction == null) return;

        if (interactAction.WasPressedThisFrame())
            OpenPanel();
    }

    private void OpenPanel()
    {
        if (panelOpen || isEnding) return;
        panelOpen = true;

        PromptManager.Instance?.Clear(PromptKey);

        CacheCursor();
        UnlockCursor();

        // Switch to UI map so buttons work
        if (playerInput != null)
        {
            previousActionMap = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : "";
            var uiMap = playerInput.actions?.FindActionMap("UI", false);
            if (uiMap != null) playerInput.SwitchCurrentActionMap("UI");
        }

        if (playerController != null) playerController.enabled = false;

        if (answerDatabase != null && questionUI != null)
            questionUI.SetQuestions(answerDatabase.answers);

        questionUI?.Show();

        if (answerText != null) answerText.text = "";
    }

    private void ClosePanel()
    {
        if (!panelOpen) return;
        panelOpen = false;

        RestoreCursor();

        if (playerInput != null && !string.IsNullOrEmpty(previousActionMap))
            playerInput.SwitchCurrentActionMap(previousActionMap);

        if (playerController != null) playerController.enabled = true;

        questionUI?.Hide();
    }

    private void HandleQuestionSelected(int index)
    {
        if (isEnding) return;
        if (answerDatabase == null) return;
        if (index < 0 || index >= answerDatabase.answers.Length) return;

        // Close panel immediately
        panelOpen = false;
        questionUI?.Hide();

        // Keep player frozen until end
        if (playerController != null) playerController.enabled = false;
        if (playerInput != null) playerInput.DeactivateInput();

        RestoreCursor(); // show cursor OK; optional

        string answer = answerDatabase.answers[index].answer;

        if (answerText != null) answerText.text = answer;

        if (endCo != null) StopCoroutine(endCo);
        endCo = StartCoroutine(EndFlow(answer));
    }

    private IEnumerator EndFlow(string answer)
    {
        isEnding = true;

        // Show answer as timed message
        if (messageUI != null)
            messageUI.ShowTimed(answer, true);

        if (answerDisplaySeconds > 0f)
            yield return new WaitForSecondsRealtime(answerDisplaySeconds);

        // Show final message
        if (messageUI != null)
            messageUI.ShowTimed(finalMessage, true);

        if (endDelaySeconds > 0f)
            yield return new WaitForSecondsRealtime(endDelaySeconds);

        SceneManager.LoadScene(menuSceneName);
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
}