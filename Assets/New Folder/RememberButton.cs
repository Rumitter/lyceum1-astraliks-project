using UnityEngine;
using UnityEngine.UI;

public class RememberButton : MonoBehaviour
{
    [Header("Кнопка")]
    [SerializeField] Button button;

    [Header("Панель, которая включается")]
    [SerializeField] GameObject panel;

    [Header("Ключ сохранения (уникальный!)")]
    [SerializeField] string saveKey = "panel_unlocked";

    [Header("Опции")]
    [Tooltip("Если выключено — панель просто запоминается. Если включено — можно скрыть обратно")]
    [SerializeField] bool toggleable = false;

    void Start()
    {
        // Проверяем сохранение: 1 = уже нажимали
        bool wasPressed = PlayerPrefs.GetInt(saveKey, 0) == 1;

        // Ставим панель в нужное состояние СРАЗУ при старте
        if (panel != null)
            panel.SetActive(wasPressed);

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (panel == null) return;

        bool newState;

        if (toggleable)
            newState = !panel.activeSelf;
        else
            newState = true;

        panel.SetActive(newState);

        // Сохраняем
        PlayerPrefs.SetInt(saveKey, newState ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[RememberButton] Панель: {newState}, сохранено в '{saveKey}'");
    }

    // Для теста: сбросить сохранение
    [ContextMenu("Сбросить сохранение")]
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        Debug.Log($"[RememberButton] Ключ '{saveKey}' удалён");
    }
}