using UnityEngine;
using UnityEngine.InputSystem;

public class SacredSwordController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem fireEffect;
    public Light swordLight;
    public AudioSource igniteSound;   // tek seferlik ateş SFX
    public AudioSource hornSound;     // arka plan/ambiyans (opsiyonel)
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
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (fireEffect != null) fireEffect.Stop();
        if (swordLight != null) swordLight.enabled = false;

        if (playerInput != null)
            interactAction = playerInput.actions?.FindAction("Interact", false);
    }

    private void Update()
    {
        if (player == null || interactAction == null) return;
        if (isActivated) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance < interactDistance)
        {
            messageUI?.ShowMessage("Press [E] to ignite the sacred sword.");
            if (interactAction.WasPressedThisFrame())
                ActivateSword();
        }
    }

    private void ActivateSword()
    {
        if (isActivated) return;
        isActivated = true;

        fireEffect?.Play();
        if (swordLight != null) swordLight.enabled = true;

        if (igniteSound != null && !igniteSound.isPlaying) igniteSound.Play();
        if (hornSound   != null && !hornSound.isPlaying)   hornSound.Play();

        messageUI?.ShowMessage("The sacred sword has been ignited!", true);
        gateController?.UnlockGate();

        Debug.Log("🔥 Sacred sword ignited.");
    }

    public bool IsActivated() => isActivated;
}
