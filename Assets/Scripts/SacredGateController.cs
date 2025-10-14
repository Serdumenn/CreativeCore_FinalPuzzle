using UnityEngine;
using UnityEngine.InputSystem;

public class SacredGateController : MonoBehaviour
{
    [Header("References")]
    public DoorController doorController;         // Aynı objede
    public MessageUI messageUI;
    public SacredSwordController sacredSword;
    public PlayerInput playerInput;

    [Header("Settings")]
    public float interactDistance = 5f;

    private Transform player;
    private InputAction interactAction;
    private bool isUnlocked = false;

    private void Awake()
    {
        // Oyun başında gate kilitli olsun
        if (doorController != null)
            doorController.SetLocked(true);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerInput != null)
            interactAction = playerInput.actions["Interact"];
    }

    private void Update()
    {
        if (player == null || interactAction == null || doorController == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < interactDistance)
        {
            if (!isUnlocked)
            {
                messageUI?.ShowMessage("The sacred sword must be ignited first!");
                return; // kilitliyken Toggle yok
            }

            // Unlocked durumda: kapı aç/kapat
            messageUI?.ShowMessage("Press [E] to open the gate.");
            if (interactAction.WasPressedThisFrame())
                doorController.ToggleDoor();
        }
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        // Kapıyı artık haricen açılabilir hale getir
        doorController?.SetLocked(false);

        messageUI?.ShowMessage("The sacred gate has been unlocked!");
        Debug.Log("<color=green>✅ Sacred Gate unlocked!</color>");
    }
}
