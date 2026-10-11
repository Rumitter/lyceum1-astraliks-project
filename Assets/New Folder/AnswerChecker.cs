using System.Collections.Generic;

using TMPro;

using UnityEngine;

using UnityEngine.UI;



public class AnswerChecker : MonoBehaviour

{

    [System.Serializable]

    public class Task

    {

        [Tooltip("Текст задания")]

        public string question;



        [Tooltip("Правильные ответы для этого задания")]

        public string[] correctAnswers = { "" };

    }



    [Header("Ввод")]

    [SerializeField] TMP_InputField inputField;

    [SerializeField] Button confirmButton;



    [Header("Кнопка сброса прогресса")]

    [Tooltip("Перетащите сюда GameObject с Collider2D, по клику на который сбрасывается прогресс")]

    [SerializeField] GameObject resetObject;



    [Header("Список всех заданий")]

    [SerializeField] Task[] allTasks;



    [Header("Сколько заданий показать (рандомно)")]

    [Tooltip("0 или меньше — использовать все задания")]

    [SerializeField] int tasksToShow = 3;



    [Header("UI для отображения задания")]

    [SerializeField] TMP_Text questionText;



    [Header("Опции")]

    [SerializeField] bool ignoreCase = true;

    [SerializeField] bool trimSpaces = true;

    [SerializeField] bool ignoreSpaces = true;



    [Header("Результат — правильный ответ")]

    [SerializeField] GameObject correctObject;

    [SerializeField] TMP_Text correctText;

    [SerializeField] string correctMessage = "Правильно!";



    [Header("Результат — неправильный ответ")]

    [SerializeField] GameObject wrongObject;

    [SerializeField] TMP_Text wrongText;

    [SerializeField] string wrongMessage = "Неправильно, попробуй ещё";



    [Header("Финальное сообщение")]

    [Tooltip("Отдельный объект для надписи 'Готово!'")]

    [SerializeField] GameObject finishObject;

    [SerializeField] TMP_Text finishText;

    [SerializeField] string finishMessage = "Готово!";



    [Header("Объекты, которые появятся ПОСЛЕ всех заданий")]

    [SerializeField] GameObject[] objectsToShowAfterAllTasks;



    [Header("Задержка перед появлением награды (в секундах)")]

    [SerializeField] float showRewardDelay = 0f;



    private List<Task> currentTasks = new List<Task>();

    private int currentIndex = 0;

    private bool allTasksCompleted = false;



    // Ссылка на компонент, который ловит клик по resetObject

    private ResetClickReceiver resetReceiver;



    void Start()

    {

        if (confirmButton != null)

            confirmButton.onClick.AddListener(Check);



        // Подписываемся на клик по объекту с коллайдером

        if (resetObject != null)

        {

            resetReceiver = resetObject.GetComponent<ResetClickReceiver>();

            if (resetReceiver == null)

                resetReceiver = resetObject.AddComponent<ResetClickReceiver>();



            resetReceiver.OnClicked += ResetAll;

        }



        if (inputField != null)

            inputField.onSubmit.AddListener(_ => Check());



        ResetAll();

    }



    // ---------- Полный сброс ----------



    public void ResetAll()

    {

        CancelInvoke();



        allTasksCompleted = false;

        currentIndex = 0;



        if (inputField != null)

        {

            inputField.interactable = true;

            inputField.text = "";

        }

        if (confirmButton != null) confirmButton.interactable = true;



        if (correctObject != null) correctObject.SetActive(false);

        if (wrongObject != null) wrongObject.SetActive(false);

        if (finishObject != null) finishObject.SetActive(false);



        if (objectsToShowAfterAllTasks != null)

        {

            foreach (GameObject obj in objectsToShowAfterAllTasks)

                if (obj != null) obj.SetActive(false);

        }



        PickRandomTasks();

        ShowCurrentTask();



        Debug.Log("[AnswerChecker] Прогресс сброшен.");

    }



    // ---------- Выбор случайных заданий ----------



    private void PickRandomTasks()

    {

        currentTasks.Clear();



        if (allTasks == null || allTasks.Length == 0)

        {

            Debug.LogWarning("[AnswerChecker] Список заданий пуст!");

            return;

        }



        List<Task> pool = new List<Task>(allTasks);



        for (int i = 0; i < pool.Count; i++)

        {

            int rnd = Random.Range(i, pool.Count);

            (pool[i], pool[rnd]) = (pool[rnd], pool[i]);

        }



        int count = (tasksToShow <= 0 || tasksToShow > pool.Count) ? pool.Count : tasksToShow;



        for (int i = 0; i < count; i++)

            currentTasks.Add(pool[i]);



        currentIndex = 0;

    }



    // ---------- Отображение текущего задания ----------



    private void ShowCurrentTask()

    {

        if (currentTasks.Count == 0)

        {

            if (questionText != null) questionText.text = "";

            return;

        }



        if (questionText != null)

            questionText.text = currentTasks[currentIndex].question;

    }



    // ---------- Проверка ----------



    public void Check()

    {

        if (allTasksCompleted) return;



        if (currentTasks.Count == 0)

        {

            Debug.LogWarning("[AnswerChecker] Нет активных заданий.");

            return;

        }



        string input = inputField != null ? inputField.text : "";

        input = Normalize(input);



        Task task = currentTasks[currentIndex];

        bool isCorrect = false;



        foreach (string answer in task.correctAnswers)

        {

            string a = Normalize(answer);



            if (ignoreCase)

            {

                if (string.Equals(input, a, System.StringComparison.OrdinalIgnoreCase))

                {

                    isCorrect = true;

                    break;

                }

            }

            else

            {

                if (input == a)

                {

                    isCorrect = true;

                    break;

                }

            }

        }



        ShowResult(isCorrect);

        Debug.Log($"[AnswerChecker] Задание {currentIndex + 1}/{currentTasks.Count}, Ввод: '{input}', Верно: {isCorrect}");



        if (isCorrect)

            NextTask();

    }



    // ---------- Переход к следующему заданию ----------



    private void NextTask()

    {

        currentIndex++;



        if (currentIndex >= currentTasks.Count)

        {

            CompleteAllTasks();

            return;

        }



        if (inputField != null) inputField.text = "";

        if (correctObject != null) correctObject.SetActive(false);

        if (wrongObject != null) wrongObject.SetActive(false);



        ShowCurrentTask();

    }



    // ---------- Все задания выполнены ----------



    private void CompleteAllTasks()

    {

        allTasksCompleted = true;



        Debug.Log("[AnswerChecker] Все задания выполнены!");



        if (inputField != null) inputField.interactable = false;

        if (confirmButton != null) confirmButton.interactable = false;



        if (correctObject != null) correctObject.SetActive(false);

        if (wrongObject != null) wrongObject.SetActive(false);



        if (finishObject != null) finishObject.SetActive(true);

        if (finishText != null) finishText.text = finishMessage;



        if (showRewardDelay > 0f)

            Invoke(nameof(ShowRewardObjects), showRewardDelay);

        else

            ShowRewardObjects();

    }



    private void ShowRewardObjects()

    {

        if (objectsToShowAfterAllTasks == null) return;



        foreach (GameObject obj in objectsToShowAfterAllTasks)

            if (obj != null) obj.SetActive(true);

    }



    // ---------- Нормализация ----------



    private string Normalize(string value)

    {

        if (string.IsNullOrEmpty(value)) return "";



        if (trimSpaces)

            value = value.Trim();



        if (ignoreSpaces)

            value = value.Replace(" ", "").Replace("\t", "").Replace("\u00A0", "");



        return value;

    }



    // ---------- Показ результата ----------



    private void ShowResult(bool isCorrect)

    {

        if (isCorrect)

        {

            if (wrongObject != null) wrongObject.SetActive(false);

            if (correctObject != null) correctObject.SetActive(true);

            if (correctText != null) correctText.text = correctMessage;

        }

        else

        {

            if (correctObject != null) correctObject.SetActive(false);

            if (wrongObject != null) wrongObject.SetActive(true);

            if (wrongText != null) wrongText.text = wrongMessage;

        }

    }

}