using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Question
{
    [TextArea] public string questionText;
    public string correctAnswer;
    public string[] words;
}

public class QuestionManager : MonoBehaviour
{
    [Header("UI ссылки")]
    public TextMeshProUGUI questionLabel;
    public Transform wordsZone;
    public Transform answerZone;
    public GameObject wordBlockPrefab;
    public Button checkButton;
    public TextMeshProUGUI resultText;

    [Header("Вопросы")]
    public Question[] questions;

    [Header("Настройки")]
    public float nextQuestionDelay = 1.5f;

    [Header("Объект-кнопка рестарта (Collider2D)")]
    [Tooltip("Работает только после прохождения всех вопросов")]
    [SerializeField] private GameObject resetObject;

    private int currentIndex = 0;
    private string correctAnswer;
    private bool answered = false;
    private bool allCompleted = false;

    void Start()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("Массив questions пустой! Заполни его в инспекторе.");
            return;
        }

        if (checkButton != null)
            checkButton.onClick.AddListener(CheckAnswer);

        if (resetObject != null)
        {
            var receiver = resetObject.GetComponent<ResetClickReceiver>();
            if (receiver == null)
                receiver = resetObject.AddComponent<ResetClickReceiver>();

            receiver.OnClicked += OnResetClicked;
        }

        LoadQuestion(0);
    }

    void OnResetClicked()
    {
        if (!allCompleted) return;
        RestartAllQuestions();
    }

    public void RestartAllQuestions()
    {
        CancelInvoke();
        allCompleted = false;
        LoadQuestion(0);
    }

    void LoadQuestion(int index)
    {
        if (index >= questions.Length)
        {
            allCompleted = true;

            questionLabel.text = "Все вопросы пройдены!";
            resultText.text = "🎉 Молодец!";
            resultText.color = Color.green;
            if (checkButton != null) checkButton.interactable = false;

            ClearZone(wordsZone);
            ClearZone(answerZone);
            return;
        }

        answered = false;
        currentIndex = index;
        correctAnswer = questions[index].correctAnswer;

        ClearZone(wordsZone);
        ClearZone(answerZone);

        questionLabel.text = questions[index].questionText;
        resultText.text = "";
        if (checkButton != null) checkButton.interactable = true;

        var shuffled = questions[index].words
            .OrderBy(x => Random.value)
            .ToList();

        foreach (var w in shuffled)
        {
            GameObject go = Instantiate(wordBlockPrefab, wordsZone);

            var wb = go.GetComponent<WordBlock>();
            if (wb == null)
            {
                Debug.LogError($"В префабе нет компонента WordBlock! Имя: {go.name}");
                continue;
            }

            wb.Setup(w, wordsZone);
        }
    }

    void CheckAnswer()
    {
        if (answered) return;

        var blocks = answerZone.GetComponentsInChildren<WordBlock>();

        if (blocks.Length == 0)
        {
            resultText.text = "Перетащи слова в поле ответа!";
            resultText.color = Color.yellow;
            return;
        }

        string userAnswer = Normalize(string.Join(" ", blocks.Select(b => b.word)));
        string expected = Normalize(correctAnswer);

        Debug.Log($"Сравнение: '{userAnswer}' vs '{expected}' → {userAnswer == expected}");

        if (userAnswer == expected)
        {
            answered = true;
            resultText.text = "✓ Правильно!";
            resultText.color = Color.green;
            if (checkButton != null) checkButton.interactable = false;

            Invoke(nameof(LoadNext), nextQuestionDelay);
        }
        else
        {
            resultText.text = "✗ Попробуй ещё раз";
            resultText.color = Color.red;
        }
    }

    string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Replace('\u00A0', ' ');
        s = s.Replace('\uFEFF', ' ');
        s = s.Replace('\t', ' ');
        s = s.Replace('\r', ' ');
        s = s.Replace('\n', ' ');
        while (s.Contains("  ")) s = s.Replace("  ", " ");
        return s.Trim().ToLowerInvariant();
    }

    void LoadNext()
    {
        LoadQuestion(currentIndex + 1);
    }

    void ClearZone(Transform zone)
    {
        if (zone == null) return;
        for (int i = zone.childCount - 1; i >= 0; i--)
        {
            Destroy(zone.GetChild(i).gameObject);
        }
    }
}