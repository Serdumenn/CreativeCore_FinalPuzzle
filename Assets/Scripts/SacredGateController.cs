using UnityEngine;

public class SacredGateController : MonoBehaviour
{
    [Header("Gate Door")]
    public DoorController gateDoor;

    [Header("UI")]
    public string lockedPrompt = "The gate is locked.";
    public string unlockedPrompt = "Press [E] to open the gate.";

    public bool IsUnlocked { get; private set; }

    private void Awake()
    {
        // Kapı referansı boşsa otomatik bulmayı dene (aynı objede / child)
        if (gateDoor == null)
            gateDoor = GetComponentInChildren<DoorController>();

        // Başta kilitli başlatmak istiyorsan:
        if (gateDoor != null)
            gateDoor.SetLocked(true);
    }

    public void UnlockGate()
    {
        IsUnlocked = true;

        if (gateDoor != null)
            gateDoor.SetLocked(false);

        // Prompt “kapıya yaklaşınca” DoorController’dan zaten geleceği için burada zorlamıyoruz.
        // İstersen kısa bir timed mesaj basabilirsin:
        // FindFirstObjectByType<MessageUI>()?.ShowMessage("You hear the gate unlock.", true);
    }

    // İstersen kapıya yaklaşınca locked bilgisini göstermek için:
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!IsUnlocked)
            PromptManager.Instance?.Show(lockedPrompt, PromptPriority.Info);
        else
            PromptManager.Instance?.Clear(lockedPrompt);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PromptManager.Instance?.Clear(lockedPrompt);
    }
}