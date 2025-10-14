using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Motion Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Header("Proximity Settings")]
    public float interactDistance = 3.5f;
    public float viewAngle = 45f;
    public float safeDistance = 1.5f;

    [Header("Interaction")]
    [Tooltip("If true, this door listens to input itself (Door1 gibi). Gate’te false yapın.")]
    public bool selfInteract = true;
    public PlayerInput playerInput;          // selfInteract true ise doldurun
    public string interactActionName = "Interact";
    public MessageUI messageUI;              // selfInteract true ise opsiyonel olarak doldurun

    [SerializeField] private bool isLocked = false;

    private bool isOpen = false;
    private bool isOpeningInward = true;
    private Transform player;
    private Quaternion leftClosedRot, rightClosedRot;
    private Quaternion leftOpenInwardRot, rightOpenInwardRot;
    private Quaternion leftOpenOutwardRot, rightOpenOutwardRot;
    private InputAction interactAction;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (selfInteract && playerInput != null)
            interactAction = playerInput.actions[interactActionName];

        leftClosedRot = leftDoor.localRotation;
        rightClosedRot = rightDoor.localRotation;

        leftOpenInwardRot = leftClosedRot * Quaternion.Euler(0, -openAngle, 0);
        rightOpenInwardRot = rightClosedRot * Quaternion.Euler(0,  openAngle, 0);

        leftOpenOutwardRot = leftClosedRot * Quaternion.Euler(0,  openAngle, 0);
        rightOpenOutwardRot = rightClosedRot * Quaternion.Euler(0, -openAngle, 0);
    }

    void Update()
    {
        if (player == null) return;

        // Yalnızca selfInteract kapılarda giriş dinle
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
                if (messageUI != null)
                    messageUI.ShowMessage("Press [E] to open/close the door");

                if (interactAction.WasPressedThisFrame())
                {
                    if (!isOpen)
                    {
                        Vector3 doorForward = transform.forward;
                        Vector3 playerToDoor = (transform.position - player.position).normalized;
                        float dot = Vector3.Dot(doorForward, playerToDoor);
                        isOpeningInward = dot < 0; // oyuncu kapının önündeyse dışarı aç
                    }

                    if (distance > safeDistance)
                        isOpen = !isOpen;
                }
            }
        }

        // Rotasyonler
        if (isOpen)
        {
            if (isOpeningInward)
            {
                leftDoor.localRotation  = Quaternion.Slerp(leftDoor.localRotation,  leftOpenInwardRot,  Time.deltaTime * openSpeed);
                rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightOpenInwardRot, Time.deltaTime * openSpeed);
            }
            else
            {
                leftDoor.localRotation  = Quaternion.Slerp(leftDoor.localRotation,  leftOpenOutwardRot,  Time.deltaTime * openSpeed);
                rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightOpenOutwardRot, Time.deltaTime * openSpeed);
            }
        }
        else
        {
            leftDoor.localRotation  = Quaternion.Slerp(leftDoor.localRotation,  leftClosedRot,  Time.deltaTime * openSpeed);
            rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightClosedRot, Time.deltaTime * openSpeed);
        }
    }

    // SacredGate tarafından çağrılır
    public void ToggleDoor()
    {
        if (isLocked) return;
        isOpen = !isOpen;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (isLocked) isOpen = false;
    }

    // Gate kontrolü veya başka sistemler için yardımcı
    public bool IsPlayerLookingAtDoor()
    {
        if (player == null) return false;

        Vector3 directionToDoor = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, directionToDoor);
        float distance = Vector3.Distance(player.position, transform.position);

        return angle < viewAngle && distance < interactDistance;
    }
}
