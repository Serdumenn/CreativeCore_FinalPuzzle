using UnityEngine;
using UnityEngine.InputSystem;

public class SacredGateController : MonoBehaviour
{
    [Header("References")]
    public DoorController doorController;
    public MessageUI messageUI;
    public SacredSwordController sacredSword; // optional
    public PlayerInput playerInput;

    [Header("Settings")]
    public float interactDistance = 5f;

    private Transform player;
    private InputAction interactAction;

    [SerializeField] private bool isUnlocked = false;
    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        if (doorController != null)
        {
            doorController.SetLocked(true);
            // güvenlik: kapının override ile kapanan selfInteract'ini aç
            if (!doorController.selfInteract)
            {
                doorController.selfInteract = true;
                Debug.LogWarning("[SacredGate] Door selfInteract was OFF. Enabled for safety.");
            }
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerInput != null)
            interactAction = playerInput.actions?.FindAction("Interact", false);
    }

    private void Update()
    {
        if (player == null || doorController == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d > interactDistance) return;

        if (!isUnlocked)
        {
            messageUI?.ShowMessage("The sacred sword must be ignited first!");
            return;
        }
        // kilit açıldıysa DoorController kendi prompt + E akışını yürütür
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        if (doorController != null)
        {
            doorController.SetLocked(false);
            // güvenlik: yine selfInteract'i ON tut
            if (!doorController.selfInteract)
            {
                doorController.selfInteract = true;
                Debug.LogWarning("[SacredGate] selfInteract forced ON at unlock.");
            }
        }

        messageUI?.ShowMessage("The sacred gate is now open.");
        Debug.Log("🔓 Sacred gate unlocked (SetLocked(false) invoked).");
    }
}
