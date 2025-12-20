using UnityEngine;

public class SacredGateController : MonoBehaviour
{
    [Header("References")]
    public DoorController gateDoor;     // SacredGate door'un DoorController'ı
    public MessageUI messageUI;         // (opsiyonel) eski sistemle mesaj basmak istersen
    public PromptManager promptManager; // (opsiyonel) boşsa PromptManager.Instance kullanır

    [Header("Prompt")]
    public bool showPrompt = true;
    public string lockedPrompt = "The gate is locked.";
    public string unlockedPrompt = "The gate is unlocked.";

    [Header("State")]
    [SerializeField] private bool isUnlocked = false;

    private const string LockedPromptKey = "The gate is locked.";
    private const string UnlockedPromptKey = "The gate is unlocked.";

    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        if (promptManager == null)
            promptManager = PromptManager.Instance;
    }

    private void Start()
    {
        // Başlangıçta kapıyı kilitle
        if (gateDoor != null)
            gateDoor.SetLocked(!isUnlocked);
        else
            Debug.LogWarning("[SacredGate] gateDoor reference is missing!");
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        if (gateDoor != null)
            gateDoor.SetLocked(false);

        // Prompt temizliği (kilitli mesajı kalmasın)
        promptManager?.Clear(lockedPrompt);
        promptManager?.Show(unlockedPrompt, PromptPriority.Info, force: true);

        // İstersen timed mesaj
        messageUI?.ShowMessage("You hear the gate unlock.", true);

        Debug.Log("[SacredGate] Gate unlocked!");
    }

    // İstersen gate yakınındayken bilgi prompt’u gösterelim (opsiyonel)
    private void OnTriggerStay(Collider other)
    {
        if (!showPrompt) return;
        if (!other.CompareTag("Player")) return;

        var pm = promptManager != null ? promptManager : PromptManager.Instance;
        if (pm == null) return;

        if (!isUnlocked)
            pm.Show(lockedPrompt, PromptPriority.Info);
        else
            pm.Show(unlockedPrompt, PromptPriority.Info);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!showPrompt) return;
        if (!other.CompareTag("Player")) return;

        var pm = promptManager != null ? promptManager : PromptManager.Instance;
        if (pm == null) return;

        pm.Clear(lockedPrompt);
        pm.Clear(unlockedPrompt);
    }
}