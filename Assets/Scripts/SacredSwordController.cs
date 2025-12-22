// Assets/Scripts/SacredSwordController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class SacredSwordController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem fireEffect;
    public Light swordLight;
    public AudioSource igniteSound;
    public AudioSource hornSound;
    public MessageUI messageUI;
    public SacredGateController gateController;
    public PlayerInput playerInput;

    [Header("Settings")]
    public float interactDistance = 5f;

    private Transform player;
    private bool isActivated = false;
    private InputAction interactAction;

    private string promptId;
    private const string InteractPrompt = "Press [E] to ignite the sacred sword.";

    private void Start()
    {
        promptId = $"SwordPrompt:{GetInstanceID()}";
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerInput == null && player != null)
            playerInput = player.GetComponent<PlayerInput>();

        if (fireEffect != null) fireEffect.Stop();
        if (swordLight != null) swordLight.enabled = false;

        if (playerInput != null)
        {
            interactAction = playerInput.actions?.FindAction("Interact", false);
            if (interactAction == null)
                Debug.LogWarning("[SacredSword] Interact action not found on PlayerInput.");
            else
                interactAction.Enable();
        }
        else
        {
            Debug.LogWarning("[SacredSword] PlayerInput reference missing; sword cannot be ignited.");
        }
    }

    private void Update()
    {
        if (player == null || interactAction == null)
        {
            PromptManager.Instance?.Clear(promptId);
            return;
        }

        if (isActivated)
        {
            PromptManager.Instance?.Clear(promptId);
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance < interactDistance)
        {
            PromptManager.Instance?.Show(InteractPrompt, promptId, PromptPriority.Interact);

            if (interactAction.WasPressedThisFrame())
                ActivateSword();
        }
        else
        {
            PromptManager.Instance?.Clear(promptId);
        }
    }

    private void ActivateSword()
    {
        if (isActivated) return;
        isActivated = true;

        fireEffect?.Play();
        if (swordLight != null) swordLight.enabled = true;

        if (igniteSound != null && !igniteSound.isPlaying) igniteSound.Play();
        if (hornSound != null && !hornSound.isPlaying) hornSound.Play();

        PromptManager.Instance?.Clear(promptId);

        if (messageUI != null)
            messageUI.ShowTimed("The sacred sword has been ignited!", 2.0f, force: true);

        gateController?.UnlockGate();

        Debug.Log("[SacredSword] Sacred sword ignited.");
    }

    public bool IsActivated() => isActivated;
}