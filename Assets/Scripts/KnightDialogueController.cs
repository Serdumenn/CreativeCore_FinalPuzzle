using UnityEngine;
using UnityEngine.InputSystem;

public class KnightDialogueController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public float interactDistance = 5f;

    public MessageUI messageUI;
    public KnightQuestionUI questionUI;

    public GameObject answerPanel;
    public TMPro.TMP_Text answerText;

    public KnightAnswerDatabase answerDatabase;
    public AudioSource knightAudio;
    public PlayerController playerController;

    private bool panelOpen = false;

    private void Start()
    {
        // Güvenlik kontrolleri
        if (questionUI == null)
        {
            Debug.LogError("KnightDialogueController: questionUI reference is missing.");
            return;
        }

        if (answerPanel == null)
        {
            Debug.LogError("KnightDialogueController: answerPanel reference is missing.");
        }

        if (answerText == null)
        {
            Debug.LogError("KnightDialogueController: answerText reference is missing.");
        }

        if (answerDatabase == null)
        {
            Debug.LogError("KnightDialogueController: answerDatabase reference is missing.");
        }

        // KnightQuestionUI eventlerine abone ol
        questionUI.OnQuestionSelected.AddListener(HandleQuestion);
        questionUI.OnClosePanel.AddListener(ClosePanel);

        // Başlangıçta kapalı olsun
        questionUI.HidePanel();
        if (answerPanel != null)
            answerPanel.SetActive(false);
    }

    private void Update()
    {
        if (panelOpen) return;
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > interactDistance) return;

        // Etrafındaysan prompt göster
        messageUI.ShowMessage("Press [E] to speak with the knight.");

        // E tuşu ile panel aç
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !panelOpen)
        {
            OpenPanel();
        }
    }

    private void OpenPanel()
    {
        panelOpen = true;

        // Mesajı kaldır
        messageUI.HideMessage();

        // Oyuncu hareketini kapat
        if (playerController != null)
            playerController.enabled = false;

        // Mouse'u serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Soru panelini aç
        questionUI.ShowPanel();
    }

    private void ClosePanel()
    {
        panelOpen = false;

        // Oyuncu kontrolü geri ver
        if (playerController != null)
            playerController.enabled = true;

        // Mouse'u tekrar kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Panelleri kapat
        questionUI.HidePanel();
        if (answerPanel != null)
            answerPanel.SetActive(false);

        // UI mesajını temizle
        messageUI.HideMessage();
    }

    private void HandleQuestion(int index)
    {
        if (answerDatabase == null || answerDatabase.answers == null)
        {
            Debug.LogWarning("KnightDialogueController: answerDatabase is not set.");
            return;
        }

        if (index < 0 || index >= answerDatabase.answers.Length)
        {
            Debug.LogWarning("KnightDialogueController: answer index out of range: " + index);
            return;
        }

        var ans = answerDatabase.answers[index];

        // Yazılı cevabı göster
        if (answerPanel != null)
            answerPanel.SetActive(true);

        if (answerText != null)
            answerText.text = ans.answerText;

        // Ses çal (AudioSource atanmadıysa patlamasın)
        if (knightAudio != null && ans.answerClip != null)
        {
            knightAudio.PlayOneShot(ans.answerClip);
        }

        // 3 saniye sonra panel kapansın
        Invoke(nameof(ClosePanel), 3f);
    }
}