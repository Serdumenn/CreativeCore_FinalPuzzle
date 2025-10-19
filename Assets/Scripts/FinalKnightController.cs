using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FinalKnightController : MonoBehaviour
{
    [Header("References")]
    public SacredGateController gateController;
    public MessageUI messageUI;
    public PlayerInput playerInput;

    [Header("Settings")]
    [Min(0.5f)] public float interactDistance = 4.5f;
    public string lockedMessage = "The gate remains sealed...";
    public string readyMessage  = "Press [E] to speak with the knight.";

    [Header("Ending")]
    [Min(0f)] public float endDelaySeconds = 4f;
    [TextArea] public string farewellMessage = "The knight acknowledges your deeds...";

    private Transform player;
    private InputAction interactAction;
    private PlayerController playerController;
    private bool isEnding;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player ? player.GetComponent<PlayerController>() : null;

        if (gateController == null)
            gateController = FindFirstObjectByType<SacredGateController>();

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
        if (player == null || gateController == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d > interactDistance) return;

        if (!gateController.IsUnlocked)
        {
            messageUI?.ShowMessage(lockedMessage);
            return;
        }

        messageUI?.ShowMessage(readyMessage);

        if (interactAction != null && interactAction.WasPressedThisFrame())
            StartCoroutine(FinishAndLoadMenu());
    }

    private System.Collections.IEnumerator FinishAndLoadMenu()
    {
        if (isEnding) yield break;
        isEnding = true;

        if (playerController) playerController.enabled = false;
        if (playerInput) playerInput.DeactivateInput();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        messageUI?.ShowMessage(farewellMessage, force: true);
        yield return new WaitForSecondsRealtime(endDelaySeconds);

        SceneManager.LoadScene("MenuScene");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
#endif
}