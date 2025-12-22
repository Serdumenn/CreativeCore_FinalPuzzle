// Assets/Scripts/SacredGateController.cs
using UnityEngine;

public class SacredGateController : MonoBehaviour
{
    [Header("References")]
    public DoorController gateDoor;     // Gate üzerindeki DoorController
    public MessageUI messageUI;

    [Header("State")]
    [SerializeField] private bool isUnlocked = false;
    public bool IsUnlocked => isUnlocked;

    [Header("Optional Messages")]
    [TextArea] public string unlockedMessage = "The gate is now unlocked.";
    public float unlockedMessageSeconds = 2.0f;

    private void Start()
    {
        if (gateDoor == null)
            gateDoor = GetComponentInChildren<DoorController>();

        if (!isUnlocked && gateDoor != null)
            gateDoor.SetLocked(true);
    }

    public void UnlockGate()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        if (gateDoor != null)
            gateDoor.SetLocked(false);

        if (messageUI != null && !string.IsNullOrEmpty(unlockedMessage))
            messageUI.ShowTimed(unlockedMessage, unlockedMessageSeconds, force: true);

        Debug.Log("[SacredGate] Gate unlocked.");
    }
}