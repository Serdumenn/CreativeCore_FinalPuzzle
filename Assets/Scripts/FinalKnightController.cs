using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FinalKnightController : MonoBehaviour
{
    [Header("References")]
    public SacredGateController gateController;   // Doors/SacredGate
    public MessageUI messageUI;                   // UI_Canvas/UI_MessageText
    public PlayerInput playerInput;               // Player (PlayerInput)

    [Header("Settings")]
    [Min(0.5f)] public float interactDistance = 2.5f;
    public string lockedMessage = "The gate remains sealed...";
    public string readyMessage  = "Press [E] to speak with the knight.";

    private Transform player;
    private InputAction interactAction;
    private PlayerController playerController;
    private bool isEnding;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player ? player.GetComponent<PlayerController>() : null;

        if (playerInput == null && player)
            playerInput = player.GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            interactAction = playerInput.actions?.FindAction("Interact", throwIfNotFound: false);
            interactAction?.Enable();
        }
    }

    private void Update()
    {
        if (isEnding) return;
        if (player == null || gateController == null || interactAction == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d > interactDistance) return;

        if (!gateController.IsUnlocked)
        {
            messageUI?.ShowMessage(lockedMessage);
            return;
        }

        messageUI?.ShowMessage(readyMessage);
        if (interactAction.WasPressedThisFrame())
            StartCoroutine(FinishAndLoadMenu());
    }

    private IEnumerator FinishAndLoadMenu()
    {
        if (isEnding) yield break;
        isEnding = true;

        messageUI?.ShowMessage("The knight will guide you home...", true);

        // kontrolü kapat
        if (playerInput != null) playerInput.DeactivateInput();
        if (playerController != null) playerController.enabled = false;

        // fade YOK — doğrudan menü
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        SceneManager.LoadScene("MenuScene");
        yield break;
    }
}
