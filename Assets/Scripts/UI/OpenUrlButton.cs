using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenUrlButton : MonoBehaviour
{
    public string url;
    void Awake() =>
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!string.IsNullOrWhiteSpace(url))
                Application.OpenURL(url);
        });
}
