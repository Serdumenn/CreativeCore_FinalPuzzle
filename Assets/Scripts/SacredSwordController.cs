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

    private InputAction interactAction;
    private bool isActivated = false;
    private Transform player;

    void Start()
    {
        player = playerInput.transform;
        interactAction = playerInput.actions["Interact"]; // ✅ doğru isim!
        fireEffect.Stop();
        swordLight.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < interactDistance && !isActivated)
        {
            messageUI.ShowMessage("Press [E] to ignite the sacred sword.");

            if (interactAction.WasPressedThisFrame())
            {
                ActivateSword();
            }
        }
        else if (distance >= interactDistance && !isActivated)
        {
            messageUI.HideMessage();
        }
    }

    private void ActivateSword()
    {
        isActivated = true;
        messageUI.HideMessage();

        if (igniteSound != null)
            igniteSound.Play();

        fireEffect.Play();
        swordLight.enabled = true;

        if (gateController != null)
            gateController.UnlockGate();

        Debug.Log("<color=orange>⚔ The sacred sword has been ignited!</color>");
    }

    public bool IsActivated() => isActivated;
}
