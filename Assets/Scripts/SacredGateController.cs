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

    private InputAction interactAction;
    private Transform player;
    private bool isUnlocked = false;

    void Start()
    {
        if (playerInput != null)
        {
            player = playerInput.transform;
            interactAction = playerInput.actions["Interact"];
        }
    }

    void Update()
    {
        if (player == null || interactAction == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < interactDistance)
        {
            // Eğer kılıç aktif değilse kapı kilitli kalsın
            if (sacredSword != null && !sacredSword.IsActivated())
            {
                messageUI.ShowMessage("The sacred sword must be ignited first!");
                return;
            }

            // Kılıç aktifse kapı açılabilir
            messageUI.ShowMessage("Press [E] to open the gate.");

            if (interactAction.WasPressedThisFrame())
            {
                doorController.ToggleDoor();
            }
        }
        else
        {
            messageUI.HideMessage();
        }
    }

    public void UnlockGate()
    {
        isUnlocked = true;
        Debug.Log("<color=green>✅ Sacred Gate unlocked!</color>");
    }
}
