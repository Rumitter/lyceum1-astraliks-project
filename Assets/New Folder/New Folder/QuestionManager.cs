using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Question
{
    [TextArea] public string questionText;   // текст вопроса
    public string correctAnswer;             // правильный ответ (эталон)
    public string[] words;                   // слова для блоков (перемешаются)
}

public class QuestionManager : MonoBehaviour
{
    [Header("UI ссылки")]
    public TextMeshProUGUI questionLabel;    // текст вопроса
    public Transform wordsZone;              // зона снизу (откуда таскаем)
    public Transform answerZone;             // зона сверху (куда кладём)
    public GameObject wordBlockPrefab;       // префаб блока
    public Button checkButton;               // кнопка "Проверить"
    public TextMeshProUGUI resultText;       // текст результата

    [Header("Вопросы")]
    public Question[] questions;

    [Header("Настройки")]
    public float nextQuestionDelay = 1.5f;   // задержка перед следующим вопросом

    private int currentIndex = 0;
    private string correctAnswer;
    private bool answered = false;

    void Start()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("Массив questions пустой! Заполни его в инспекторе.");
            return;
        }

        if (checkButton != null)
            checkButton.onClick.AddListener(CheckAnswer);

        LoadQuestion(0);
    }

    // =========================
    //   ЗАГРУЗКА ВОПРОСА
    // =========================
    void LoadQuestion(int index)
    {
        if (index >= questions.Length)
        {
            questionLabel.text = "Все вопросы пройдены!";
            resultText.text = "🎉 Молодец!";
            resultText.color = Color.green;
            if (checkButton != null) checkButton.interactable = false;
            return;
        }

        answered = false;
        currentIndex = index;
        correctAnswer = questions[index].correctAnswer;

        // очистить старые блоки
        ClearZone(wordsZone);
        ClearZone(answerZone);

        // показать вопрос
        questionLabel.text = questions[index].questionText;
        resultText.text = "";
        if (checkButton != null) checkButton.interactable = true;

        // перемешать слова
        var shuffled = questions[index].words
            .OrderBy(x => Random.value)
            .ToList();

        // создать блоки
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

    // =========================
    //   ПРОВЕРКА ОТВЕТА
    // =========================
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

    // =========================
    //   УТИЛИТЫ
    // =========================
    void ClearZone(Transform zone)
    {
        if (zone == null) return;
        for (int i = zone.childCount - 1; i >= 0; i--)
        {
            Destroy(zone.GetChild(i).gameObject);
        }
    }
}