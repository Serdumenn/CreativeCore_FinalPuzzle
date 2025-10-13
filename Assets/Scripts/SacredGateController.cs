using UnityEngine;
using UnityEngine.InputSystem;

public class SacredGateController : MonoBehaviour
{
    [Header("References")]
    public DoorController doorController;
    public MessageUI messageUI;
    public SacredSwordController sacredSword;
    public PlayerInput playerInput;

    [Header("Settings")]
    public float interactDistance = 5f;

    private Transform player;
    private InputAction interactAction;
    private bool isUnlocked = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerInput != null)
            interactAction = playerInput.actions["Interact"];
    }

    private void Update()
    {
        if (player == null || interactAction == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < interactDistance)
        {
            if (!isUnlocked && sacredSword != null && !sacredSword.IsActivated())
            {
                messageUI?.ShowMessage("The sacred sword must be ignited first!");
                return;
            }

            messageUI?.ShowMessage("Press [E] to open the sacred gate.");
            if (interactAction.WasPerformedThisFrame() && isUnlocked)
            {
                doorController?.ToggleDoor();
            }
        }
    }

    public void UnlockGate()
    {
        isUnlocked = true;
        Debug.Log("🔓 Sacred Gate unlocked!");
        messageUI?.ShowMessage("The sacred gate has been unlocked!");
    }
}
