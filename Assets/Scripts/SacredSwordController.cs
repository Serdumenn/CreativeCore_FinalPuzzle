using UnityEngine;
using UnityEngine.InputSystem;

public class SacredSwordController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem fireEffect;
    public Light swordLight;
    public AudioSource igniteSound;
    public AudioSource hornSound;

    public SacredGateController gateController;
    public PlayerInput playerInput;

    [Header("Settings")]
    public float interactDistance = 5f;
    public string interactPrompt = "Press [E] to ignite the sacred sword.";

    private Transform player;
    private InputAction interactAction;
    private bool isActivated;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerInput == null && player != null)
            playerInput = player.GetComponent<PlayerInput>();

        if (fireEffect != null) fireEffect.Stop();
        if (swordLight != null) swordLight.enabled = false;

        if (playerInput != null)
        {
            interactAction = playerInput.actions?.FindAction("Interact", false);
            interactAction?.Enable();
        }
    }

    private void Update()
    {
        if (isActivated) return;
        if (player == null || interactAction == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d <= interactDistance)
        {
            PromptManager.Instance?.Show(interactPrompt, PromptPriority.Interact);

            if (interactAction.WasPressedThisFrame())
                ActivateSword();
        }
        else
        {
            PromptManager.Instance?.Clear(interactPrompt);
        }
    }

    private void ActivateSword()
    {
        isActivated = true;

        PromptManager.Instance?.Clear(interactPrompt);

        fireEffect?.Play();
        if (swordLight != null) swordLight.enabled = true;

        if (igniteSound != null) igniteSound.Play();
        if (hornSound != null) hornSound.Play();

        gateController?.UnlockGate();
    }

    public bool IsActivated() => isActivated;
}