using UnityEngine;
using UnityEngine.InputSystem;

public class SacredGateController : MonoBehaviour
{
    [Header("References")]
    public DoorController doorController;
    public MessageUI messageUI;
    public SacredSwordController sacredSword; // opsiyonel
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
            doorController.SetLocked(true);
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

        // kilit açıldıysa Door kendi mantığıyla E'yi dinler; burada ekstra iş yok
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;
        isUnlocked = true;
        doorController?.SetLocked(false);
        messageUI?.ShowMessage("The sacred gate is now open.");
    }
}
