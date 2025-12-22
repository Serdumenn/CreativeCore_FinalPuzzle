using UnityEngine;

public class SacredGateController : MonoBehaviour
{
    [Header("References")]
    public DoorController doorController;
    public MessageUI messageUI;

    [Header("Settings")]
    public float interactDistance = 5f;

    private Transform player;

    [SerializeField] private bool isUnlocked = false;
    public bool IsUnlocked => isUnlocked;

    private string promptId;

    private void Awake()
    {
        promptId = $"GatePrompt:{GetInstanceID()}";

        if (doorController != null)
            doorController.SetLocked(true);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null || doorController == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d > interactDistance)
        {
            PromptManager.Instance?.Clear(promptId);
            return;
        }

        if (!isUnlocked)
        {
            // Kalıcı prompt: “önce sword”
            PromptManager.Instance?.Show("Ignite the Sacred Sword first!", promptId, PromptPriority.Warning);
            return;
        }

        // Gate unlocked -> gate prompt artık kapansın (door kendi promptunu basacak)
        PromptManager.Instance?.Clear(promptId);
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        if (doorController != null)
            doorController.SetLocked(false);

        PromptManager.Instance?.Clear(promptId);

        if (messageUI != null)
            messageUI.ShowTimed("The sacred gate is now unlocked.", 2.0f, force: true);

        Debug.Log("[SacredGate] Unlocked.");
    }
}