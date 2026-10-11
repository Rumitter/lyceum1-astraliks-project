using UnityEngine;

public class ClickSaver : MonoBehaviour
{
    const string Key = "was_clicked";

    void Start()
    {
        // Восстановить состояние при старте
        if (PlayerPrefs.GetInt(Key, 0) == 1)
            Debug.Log("Игрок уже кликал ранее");
    }

    void OnMouseDown()
    {
        PlayerPrefs.SetInt(Key, 1);
        PlayerPrefs.Save();
        Debug.Log("Клик сохранён");
    }
}