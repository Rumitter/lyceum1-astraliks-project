using TMPro;
using UnityEngine;

public class NameInput : MonoBehaviour
{
    [SerializeField] TMP_InputField input;
    [SerializeField] GameObject panel;

    void Start()
    {
        input.characterLimit = 16;
        input.text = PlayerName.HasName ? PlayerName.Value : "";
        // панель НЕ трогаем — управляется кнопками
    }

    // OnClick кнопки "Подтвердить"
    public void Confirm()
    {
        if (string.IsNullOrWhiteSpace(input.text)) return;
        PlayerName.Value = input.text;
    }

    // OnClick кнопки "Ввести имя"
    public void Open() => panel.SetActive(true);
}