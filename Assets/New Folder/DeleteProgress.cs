using UnityEngine;

using UnityEngine.UI;

using UnityEngine.SceneManagement;



public class DeleteProgress : MonoBehaviour

{

    [Header("UI")]

    [SerializeField] private Button deleteButton;

    [SerializeField] private GameObject confirmPanel;   // панель подтверждения

    [SerializeField] private Button yesButton;

    [SerializeField] private Button noButton;



    private void Start()

    {

        // Основная кнопка "Удалить прогресс"

        deleteButton.onClick.AddListener(ShowConfirm);



        // Кнопки подтверждения

        yesButton.onClick.AddListener(DeleteAll);

        noButton.onClick.AddListener(HideConfirm);



        if (confirmPanel != null)

            confirmPanel.SetActive(false);

    }



    private void ShowConfirm()

    {

        if (confirmPanel != null)

            confirmPanel.SetActive(true);

    }



    private void HideConfirm()

    {

        if (confirmPanel != null)

            confirmPanel.SetActive(false);

    }



    private void DeleteAll()

    {

        // 1. Удаляем PlayerPrefs

        PlayerPrefs.DeleteAll();

        PlayerPrefs.Save();



        // 2. Удаляем сохранения в файлах (если используешь JSON/бинарные)

        string path = Application.persistentDataPath + "/save.json";

        if (System.IO.File.Exists(path))

            System.IO.File.Delete(path);



        // 3. Сбрасываем статические данные (если есть)

        // GameData.Reset(); 



        Debug.Log("Прогресс удалён!");



        // 4. Перезагружаем сцену (или грузим главное меню)

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

}