using System.Collections.Generic;
using UnityEngine;

public class PromptManager : MonoBehaviour
{
    [System.Serializable]
    public class Prompt
    {
        public string message;
        public MessageUI ui;
    }

    public List<Prompt> prompts = new List<Prompt>();

    public void ShowPrompt(int index)
    {
        if (index < 0 || index >= prompts.Count)
        {
            Debug.LogWarning("Prompt index out of range.");
            return;
        }

        var p = prompts[index];
        if (p.ui != null)
        {
            // ESKİ: p.ui.ShowMessage(p.message, 3f);
            p.ui.ShowMessage(p.message);  // <- DOĞRU ÇAĞRI
        }
        else
        {
            Debug.LogWarning("Prompt UI reference missing.");
        }
    }
}