using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    [Header("Doors")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Motion")]
    public float openAngle = 90f;
    public float openSpeed = 5f;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public float viewAngle = 45f;
    public float safeDistance = 1.5f;
    public bool selfInteract = true;

    public PlayerInput playerInput;
    public string interactActionName = "Interact";

    [Header("Audio")]
    public AudioSource doorSound;

    [SerializeField] private bool isLocked = false;

    private bool isOpen = false;
    private bool isOpeningInward = true;

    private Transform player;
    private InputAction interactAction;

    private Quaternion leftClosedRot, rightClosedRot;
    private Quaternion leftOpenInwardRot, rightOpenInwardRot;
    private Quaternion leftOpenOutwardRot, rightOpenOutwardRot;

    private const string PromptKey = "DOOR_INTERACT";
    private const string PromptMsg = "Press [E] to open the door.";

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (selfInteract)
        {
            if (playerInput == null)
                playerInput = FindFirstObjectByType<PlayerInput>();

            interactAction = playerInput != null
                ? playerInput.actions?.FindAction(interactActionName, false)
                : null;

            interactAction?.Enable();
        }

        leftClosedRot = leftDoor.localRotation;
        rightClosedRot = rightDoor.localRotation;

        leftOpenInwardRot = leftClosedRot * Quaternion.Euler(0f, -openAngle, 0f);
        rightOpenInwardRot = rightClosedRot * Quaternion.Euler(0f, openAngle, 0f);

        leftOpenOutwardRot = leftClosedRot * Quaternion.Euler(0f, openAngle, 0f);
        rightOpenOutwardRot = rightClosedRot * Quaternion.Euler(0f, -openAngle, 0f);
    }

    private void Update()
    {
        if (player == null)
        {
            PromptManager.Instance?.Clear(PromptKey);
            return;
        }

        if (!selfInteract || isLocked || interactAction == null)
        {
            PromptManager.Instance?.Clear(PromptKey);
            AnimateDoors();
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);
        bool isCloseEnough = distance <= interactDistance;

        Vector3 toDoor = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toDoor);
        bool isLookingAtDoor = angle < viewAngle;

        bool canInteract = isCloseEnough && isLookingAtDoor;

        if (canInteract)
        {
            PromptManager.Instance?.Show(PromptKey, PromptMsg, PromptPriority.Interact);

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
                }
            }
        }
        else
        {
            PromptManager.Instance?.Clear(PromptKey);
        }

        AnimateDoors();
    }

    private void AnimateDoors()
    {
        if (leftDoor == null || rightDoor == null) return;

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
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (isLocked) isOpen = false;
    }

    public void ToggleDoor()
    {
        if (isLocked) return;
        isOpen = !isOpen;
        PlayDoorSound();
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