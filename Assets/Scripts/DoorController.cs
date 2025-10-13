using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float interactDistance = 3.5f;
    public float viewAngle = 45f;
    public float safeDistance = 1.5f;

    private bool isOpen = false;
    private bool isOpeningInward = true;
    private Transform player;
    private Quaternion leftClosedRot, rightClosedRot;
    private Quaternion leftOpenInwardRot, rightOpenInwardRot;
    private Quaternion leftOpenOutwardRot, rightOpenOutwardRot;
    private MessageUI messageUI;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        messageUI = FindAnyObjectByType<MessageUI>();

        leftClosedRot = leftDoor.localRotation;
        rightClosedRot = rightDoor.localRotation;

        leftOpenInwardRot = leftClosedRot * Quaternion.Euler(0, -openAngle, 0);
        rightOpenInwardRot = rightClosedRot * Quaternion.Euler(0, openAngle, 0);

        leftOpenOutwardRot = leftClosedRot * Quaternion.Euler(0, openAngle, 0);
        rightOpenOutwardRot = rightClosedRot * Quaternion.Euler(0, -openAngle, 0);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool isCloseEnough = distance <= interactDistance;

        Vector3 toDoor = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toDoor);
        bool isLookingAtDoor = angle < viewAngle;

        if (isCloseEnough && isLookingAtDoor)
        {
            messageUI.ShowMessage("Press 'E' to open/close the door");

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (!isOpen)
                {
                    Vector3 doorForward = transform.forward;
                    Vector3 playerToDoor = (transform.position - player.position).normalized;
                    float dot = Vector3.Dot(doorForward, playerToDoor);

                    // ✅ fixed: open outward when player is in front
                    isOpeningInward = dot < 0;
                }

                if (!isOpen && distance > safeDistance)
                    isOpen = true;
                else if (isOpen && distance > safeDistance)
                    isOpen = false;
            }
        }
        else
        {
            //messageUI.HideMessage();
        }

        // Rotation smoothing
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

    // Optional public toggle used by SacredGateController
    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }
    public bool IsPlayerLookingAtDoor()
    {
    Transform player = GameObject.FindGameObjectWithTag("Player").transform;
    Vector3 directionToDoor = (transform.position - player.position).normalized;
    float angle = Vector3.Angle(player.forward, directionToDoor);
    float distance = Vector3.Distance(player.position, transform.position);

        // Player kapıya yakın ve ona bakıyor mu?
        return angle < viewAngle && distance < interactDistance;
    }

}
