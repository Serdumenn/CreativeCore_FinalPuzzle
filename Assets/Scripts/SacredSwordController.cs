using UnityEngine;
using UnityEngine.InputSystem;

public class SacredSwordController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem fireEffect;
    public Light swordLight;
    public AudioSource igniteSound;
    public MessageUI messageUI;
    public SacredGateController gateController;
    public PlayerInput playerInput;

    [Header("Settings")]
    public float interactDistance = 5f;

    private Transform player;
    private bool isActivated = false;
    private InputAction interactAction;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (fireEffect != null)
            fireEffect.Stop();

        if (swordLight != null)
            swordLight.enabled = false;

        if (playerInput != null)
            interactAction = playerInput.actions["Interact"];
    }

    private void Update()
    {
        if (player == null || interactAction == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (!isActivated && distance < interactDistance)
        {
            messageUI?.ShowMessage("Press [E] to ignite the sacred sword.");
            if (interactAction.WasPerformedThisFrame())
            {
                ActivateSword();
            }
        }
    }

    private void ActivateSword()
    {
        isActivated = true;

        if (fireEffect != null) fireEffect.Play();
        if (swordLight != null) swordLight.enabled = true;
        if (igniteSound != null) igniteSound.Play();

        messageUI?.ShowMessage("The sacred sword has been ignited!");
        Debug.Log("🔥 The sacred sword has been ignited!");

        if (gateController != null)
            gateController.UnlockGate();
    }

    public bool IsActivated() => isActivated;
}
