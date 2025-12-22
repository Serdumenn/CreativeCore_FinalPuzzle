// Assets/Scripts/DoorController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class DoorController : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float interactDistance = 3f;
    public float viewAngle = 45f;
    public float safeDistance = 1.5f;

    [Header("Interaction")]
    public bool selfInteract = true;
    public PlayerInput playerInput;
    public string interactActionName = "Interact";

    [Header("Audio")]
    public AudioSource doorSound;

    [SerializeField] private bool isLocked = false;

    private bool isOpen = false;
    private bool isOpeningInward = true;
    private Transform player;

    private Quaternion leftClosedRot, rightClosedRot;
    private Quaternion leftOpenInwardRot, rightOpenInwardRot;
    private Quaternion leftOpenOutwardRot, rightOpenOutwardRot;

    private InputAction interactAction;
    private bool loggedCanInteract;
    private bool hasLoggedMissingAction;

    private string promptId;
    private const string InteractPrompt = "Press [E] to open the door.";

    void Start()
    {
        promptId = $"DoorPrompt:{GetInstanceID()}";
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (selfInteract)
        {
            if (playerInput == null)
                playerInput = FindFirstObjectByType<PlayerInput>();

            interactAction = playerInput != null
                ? playerInput.actions?.FindAction(interactActionName, false)
                : null;
        }

        leftClosedRot = leftDoor.localRotation;
        rightClosedRot = rightDoor.localRotation;

        leftOpenInwardRot = leftClosedRot * Quaternion.Euler(0f, -openAngle, 0f);
        rightOpenInwardRot = rightClosedRot * Quaternion.Euler(0f, openAngle, 0f);

        leftOpenOutwardRot = leftClosedRot * Quaternion.Euler(0f, openAngle, 0f);
        rightOpenOutwardRot = rightClosedRot * Quaternion.Euler(0f, -openAngle, 0f);
    }

    void Update()
    {
        if (player == null) return;

        if (selfInteract && interactAction == null && !hasLoggedMissingAction)
        {
            Debug.LogWarning($"{name}: Interact action '{interactActionName}' not found on PlayerInput.");
            hasLoggedMissingAction = true;
        }

        bool shouldShowPrompt = false;

        if (selfInteract && !isLocked && interactAction != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            bool isCloseEnough = distance <= interactDistance;

            Vector3 toDoor = (transform.position - player.position).normalized;
            float angle = Vector3.Angle(player.forward, toDoor);
            bool isLookingAtDoor = angle < viewAngle;

            bool canInteractWithDoor = isCloseEnough && isLookingAtDoor;

            if (canInteractWithDoor)
            {
                shouldShowPrompt = true;
                PromptManager.Instance?.Show(InteractPrompt, promptId, PromptPriority.Interact);

                if (!loggedCanInteract)
                {
                    Debug.Log($"[DoorController] Player can interact with {name} (distance={distance:F2}, angle={angle:F1}).");
                    loggedCanInteract = true;
                }

                if (interactAction.WasPressedThisFrame())
                {
                    if (!isOpen)
                    {
                        Vector3 doorForward = transform.forward;
                        Vector3 playerToDoor = (transform.position - player.position).normalized;
                        float dot = Vector3.Dot(doorForward, playerToDoor);
                        isOpeningInward = (dot < 0f);
                    }

                    if (distance > safeDistance)
                    {
                        isOpen = !isOpen;
                        PlayDoorSound();
                        Debug.Log($"[DoorController] {name} toggled state. Now open={isOpen}.");
                    }
                }
            }
            else
            {
                loggedCanInteract = false;
            }
        }

        if (!shouldShowPrompt)
            PromptManager.Instance?.Clear(promptId);

        // Door animation
        if (isOpen)
        {
            if (isOpeningInward)
            {
                leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, leftOpenInwardRot, Time.deltaTime * openSpeed);
                rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightOpenInwardRot, Time.deltaTime * openSpeed);
            }
            else
            {
                leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, leftOpenOutwardRot, Time.deltaTime * openSpeed);
                rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightOpenOutwardRot, Time.deltaTime * openSpeed);
            }
        }
        else
        {
            leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, leftClosedRot, Time.deltaTime * openSpeed);
            rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightClosedRot, Time.deltaTime * openSpeed);
        }
    }

    private void PlayDoorSound()
    {
        if (doorSound != null) doorSound.Play();
        else Debug.LogWarning($"{name}: Missing doorSound AudioSource reference!");
    }

    public void ToggleDoor()
    {
        if (isLocked) return;
        isOpen = !isOpen;
        PlayDoorSound();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (isLocked) isOpen = false;
        Debug.Log($"[DoorController] {name} lock state updated: locked={isLocked}.");
    }

    public bool IsPlayerLookingAtDoor()
    {
        if (player == null) return false;
        Vector3 directionToDoor = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, directionToDoor);
        float distance = Vector3.Distance(player.position, transform.position);
        return angle < viewAngle && distance < interactDistance;
    }
}