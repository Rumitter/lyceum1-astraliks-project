using TMPro;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] string prefix = "";   // например "Привет, "
    [SerializeField] string suffix = "";   // например "!"

    void OnEnable()
    {
        PlayerName.OnChanged += UpdateLabel; UpdateLabel();
        UpdateLabel();

    }

    void OnDisable() { PlayerName.OnChanged -= UpdateLabel; }

    void Start()
    {
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        if (label == null) return;

        label.text = PlayerName.HasName
            ? prefix + PlayerName.Value + suffix
            : "";
    }
}