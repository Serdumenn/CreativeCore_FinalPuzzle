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

    private Transform player;
    private bool isActivated = false;
    private InputAction interactAction;

    private const string PromptKey = "SWORD_INTERACT";
    private const string PromptMsg = "Press [E] to ignite the sacred sword.";

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
        if (player == null || interactAction == null)
        {
            PromptManager.Instance?.Clear(PromptKey);
            return;
        }

        if (isActivated)
        {
            PromptManager.Instance?.Clear(PromptKey);
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactDistance)
        {
            PromptManager.Instance?.Show(PromptKey, PromptMsg, PromptPriority.Interact);

            if (interactAction.WasPressedThisFrame())
                ActivateSword();
        }
        else
        {
            PromptManager.Instance?.Clear(PromptKey);
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

        PromptManager.Instance?.Clear(PromptKey);
        PromptManager.Instance?.OverrideTimed("The sacred sword has been ignited!", true);

        gateController?.UnlockGate();
    }

    public bool IsActivated() => isActivated;
}