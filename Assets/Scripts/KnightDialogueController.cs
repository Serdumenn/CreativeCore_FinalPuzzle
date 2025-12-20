using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class KnightDialogueController : MonoBehaviour
{
    [Header("UI References")]
    public KnightQuestionUI questionUI;
    public TMP_Text answerText;
    public MessageUI messageUI;

    [Header("Data")]
    public KnightAnswerDatabase answerDatabase;

    [Header("Player References")]
    public Transform player;
    public PlayerInput playerInput;
    public PlayerController playerController;

    [Header("Settings")]
    public string interactPrompt = "Press [E] to speak";
    public float interactDistance = 3.5f;
    public float endDelay = 2.0f;
    public string menuSceneName = "MenuScene";

    private InputAction interactAction;
    private bool panelOpen;

    private bool wasCursorVisible;
    private CursorLockMode previousLockMode;
    private string previousActionMap;

    private Coroutine endRoutine;

    private void Awake()
    {
        // Player auto-find
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
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
        }
    }

    private void OnEnable()
    {
        if (questionUI != null)
        {
            questionUI.OnQuestionSelected.AddListener(HandleQuestionSelected);
            questionUI.OnClosePanel.AddListener(ClosePanel);
        }
    }

    private void OnDisable()
    {
        if (questionUI != null)
        {
            questionUI.OnQuestionSelected.RemoveListener(HandleQuestionSelected);
            questionUI.OnClosePanel.RemoveListener(ClosePanel);
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

        if (questionUI != null)
        {
            questionUI.SetQuestions(answerDatabase != null ? answerDatabase.answers : null);
            questionUI.Show();
        }

        if (answerText != null)
            answerText.text = string.Empty;
    }

    private void ClosePanel()
    {
        if (!panelOpen) return;
        panelOpen = false;

        if (endRoutine != null)
        {
            StopCoroutine(endRoutine);
            endRoutine = null;
        }

        RestoreCursor();
        RestoreActionMap();

        if (playerController != null)
            playerController.enabled = true;

        questionUI?.Hide();
    }

    private void HandleQuestionSelected(int index)
    {
        if (answerDatabase == null || answerDatabase.answers == null) return;
        if (index < 0 || index >= answerDatabase.answers.Length) return;

        if (answerText != null)
            answerText.text = answerDatabase.answers[index].answer;

        // İstersen paneli kapatma, sadece cevap göster ve bitir.
        if (endRoutine != null) StopCoroutine(endRoutine);
        endRoutine = StartCoroutine(EndGameToMenu());
    }

    private IEnumerator EndGameToMenu()
    {
        yield return new WaitForSeconds(endDelay);
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
