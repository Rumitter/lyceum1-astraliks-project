using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeChecker : MonoBehaviour
{
    // ---------- Класс задания ----------

    [System.Serializable]
    public class CodeTask
    {
        [Tooltip("Текст задания")]
        public string question = "Напиши код, который сложит два числа";

        [Tooltip("Входные данные для input() по порядку")]
        public string[] inputs = { "5", "3" };

        [Tooltip("Ожидаемый вывод print(...)")]
        public string expectedOutput = "8";
    }
    public void ResetInputOnly()
    {
        if (inputField != null)
        {
            inputField.interactable = true;
            inputField.text = "";
        }
        if (confirmButton != null) confirmButton.interactable = true;

        if (correctObject != null) correctObject.SetActive(false);
        if (wrongObject != null) wrongObject.SetActive(false);
        if (errorText != null) errorText.text = ""; // только для CodeChecker

        Debug.Log("[Checker] Сброшен только ввод.");
    }
    // ---------- Ввод ----------

    [Header("Ввод")]
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button confirmButton;

    [Header("Enter активирует кнопку")]
    [SerializeField] bool enterActivatesButton = true;

    // ---------- Кнопка/объект сброса ----------

    [Header("Сброс прогресса")]
    [Tooltip("GameObject с Collider2D — по клику сбрасывается прогресс")]
    [SerializeField] GameObject resetObject;

    // ---------- Задания ----------

    [Header("Список всех заданий")]
    [SerializeField] CodeTask[] allTasks;

    [Header("Сколько заданий показать (рандомно)")]
    [Tooltip("0 или меньше — использовать все задания")]
    [SerializeField] int tasksToShow = 3;

    [Header("UI для отображения задания")]
    [SerializeField] TMP_Text questionText;

    // ---------- Опции ----------

    [Header("Опции")]
    [SerializeField] bool ignoreCase = true;
    [SerializeField] float floatTolerance = 0.0001f;

    // ---------- Результат ----------

    [Header("Результат — правильный ответ")]
    [SerializeField] GameObject correctObject;
    [SerializeField] TMP_Text correctText;
    [SerializeField] string correctMessage = "Правильно!";

    [Header("Результат — неправильный ответ")]
    [SerializeField] GameObject wrongObject;
    [SerializeField] TMP_Text wrongText;
    [SerializeField] string wrongMessage = "Неправильно, попробуй ещё";

    [Header("Сообщение об ошибке в коде")]
    [SerializeField] TMP_Text errorText;
    [SerializeField] string errorMessage = "Ошибка в коде: ";

    // ---------- Финал ----------

    [Header("Финальное сообщение")]
    [SerializeField] GameObject finishObject;
    [SerializeField] TMP_Text finishText;
    [SerializeField] string finishMessage = "Готово!";

    [Header("Объекты, которые появятся ПОСЛЕ всех заданий")]
    [SerializeField] GameObject[] objectsToShowAfterAllTasks;

    [Header("Задержка перед появлением награды (в секундах)")]
    [SerializeField] float showRewardDelay = 0f;

    // ---------- Внутреннее ----------

    private List<CodeTask> currentTasks = new List<CodeTask>();
    private int currentIndex = 0;
    private bool allTasksCompleted = false;

    private ResetClickReceiver resetReceiver;

    // ---------- Жизненный цикл ----------

    void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(Check);

        if (inputField != null && enterActivatesButton)
            inputField.onSubmit.AddListener(_ => TriggerConfirm());

        // Подписка на объект сброса
        if (resetObject != null)
        {
            resetReceiver = resetObject.GetComponent<ResetClickReceiver>();
            if (resetReceiver == null)
                resetReceiver = resetObject.AddComponent<ResetClickReceiver>();

            resetReceiver.OnClicked += ResetAll;
        }

        ResetAll();
    }

    private void TriggerConfirm()
    {
        if (confirmButton != null)
            confirmButton.onClick.Invoke();
        else
            Check();
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
        if (errorText != null) errorText.text = "";

        if (objectsToShowAfterAllTasks != null)
        {
            foreach (GameObject obj in objectsToShowAfterAllTasks)
                if (obj != null) obj.SetActive(false);
        }

        PickRandomTasks();
        ShowCurrentTask();

        Debug.Log("[CodeChecker] Прогресс сброшен.");
    }

    // ---------- Выбор случайных заданий ----------

    private void PickRandomTasks()
    {
        currentTasks.Clear();

        if (allTasks == null || allTasks.Length == 0)
        {
            Debug.LogWarning("[CodeChecker] Список заданий пуст!");
            return;
        }

        List<CodeTask> pool = new List<CodeTask>(allTasks);

        for (int i = 0; i < pool.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, pool.Count);
            (pool[i], pool[rnd]) = (pool[rnd], pool[i]);
        }

        int count = (tasksToShow <= 0 || tasksToShow > pool.Count) ? pool.Count : tasksToShow;

        for (int i = 0; i < count; i++)
            currentTasks.Add(pool[i]);

        currentIndex = 0;
    }

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
            Debug.LogWarning("[CodeChecker] Нет активных заданий.");
            return;
        }

        if (errorText != null) errorText.text = "";

        CodeTask task = currentTasks[currentIndex];
        string code = inputField != null ? inputField.text : "";

        string output;
        try
        {
            output = RunCode(code, task.inputs);
        }
        catch (Exception e)
        {
            ShowError(e.Message);
            ShowResult(false);
            Debug.LogWarning($"[CodeChecker] Ошибка: {e.Message}");
            return;
        }

        bool isCorrect = CompareOutput(output, task.expectedOutput);
        ShowResult(isCorrect);

        Debug.Log($"[CodeChecker] Задание {currentIndex + 1}/{currentTasks.Count}, " +
                  $"Вывод: '{output}', Ожидалось: '{task.expectedOutput}', Верно: {isCorrect}");

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
        if (errorText != null) errorText.text = "";

        ShowCurrentTask();
    }

    // ---------- Все задания выполнены ----------

    private void CompleteAllTasks()
    {
        allTasksCompleted = true;

        Debug.Log("[CodeChecker] Все задания выполнены!");

        // Блокировка ввода
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

    // ---------- Выполнение кода ----------

    private string RunCode(string code, string[] inputs)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new Exception("Код пуст.");

        var variables = new Dictionary<string, object>();
        string printResult = null;
        int inputIndex = 0;

        string[] lines = code.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith("#")) continue;

            // print(...)
            var printMatch = Regex.Match(line, @"^print\s*\(\s*(.+?)\s*\)\s*;?$", RegexOptions.IgnoreCase);
            if (printMatch.Success)
            {
                string expr = printMatch.Groups[1].Value;
                object value = Evaluate(expr, variables, inputs, ref inputIndex);
                printResult = FormatValue(value);
                continue;
            }

            // присваивание
            var assignMatch = Regex.Match(line, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.+?)\s*;?$");
            if (assignMatch.Success)
            {
                string varName = assignMatch.Groups[1].Value;
                string expr = assignMatch.Groups[2].Value;
                object value = Evaluate(expr, variables, inputs, ref inputIndex);
                variables[varName] = value;
                continue;
            }

            throw new Exception($"Не понимаю строку: '{rawLine}'");
        }

        if (printResult == null)
            throw new Exception("В коде нет print(...).");

        return printResult;
    }

    // ---------- Вычисление выражения ----------

    private object Evaluate(string expr, Dictionary<string, object> variables, string[] inputs, ref int inputIndex)
    {
        expr = expr.Trim();

        // int(input())
        if (Regex.IsMatch(expr, @"^int\s*\(\s*input\s*\(\s*\)\s*\)$", RegexOptions.IgnoreCase))
        {
            string raw = GetNextInput(inputs, ref inputIndex);
            if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                throw new Exception($"input() вернул не число: '{raw}'");
            return num;
        }

        // float(input())
        if (Regex.IsMatch(expr, @"^float\s*\(\s*input\s*\(\s*\)\s*\)$", RegexOptions.IgnoreCase))
        {
            string raw = GetNextInput(inputs, ref inputIndex);
            if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                throw new Exception($"input() вернул не число: '{raw}'");
            return num;
        }

        // str(input())
        if (Regex.IsMatch(expr, @"^str\s*\(\s*input\s*\(\s*\)\s*\)$", RegexOptions.IgnoreCase))
            return GetNextInput(inputs, ref inputIndex);

        // input()
        if (Regex.IsMatch(expr, @"^input\s*\(\s*\)$", RegexOptions.IgnoreCase))
            return GetNextInput(inputs, ref inputIndex);

        // просто переменная
        if (Regex.IsMatch(expr, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
        {
            if (variables.TryGetValue(expr, out object val))
                return val;
            throw new Exception($"Переменная '{expr}' не определена.");
        }

        // выражение с переменными → считаем как число
        string numeric = expr.Replace(" ", "").Replace("\t", "");

        numeric = Regex.Replace(numeric, @"[a-zA-Z_][a-zA-Z0-9_]*", m =>
        {
            if (variables.TryGetValue(m.Value, out object val))
            {
                if (val is double d)
                    return d.ToString(CultureInfo.InvariantCulture);
                throw new Exception($"Переменная '{m.Value}' не число.");
            }
            throw new Exception($"Переменная '{m.Value}' не определена.");
        });

        var parser = new ExpressionParser(numeric);
        return parser.Parse();
    }

    private string GetNextInput(string[] inputs, ref int inputIndex)
    {
        if (inputs == null || inputIndex >= inputs.Length)
            throw new Exception("input() вызван больше раз, чем задано входных данных.");

        string value = inputs[inputIndex];
        inputIndex++;
        return value;
    }

    // ---------- Форматирование и сравнение ----------

    private string FormatValue(object value)
    {
        if (value is string s) return s;

        if (value is double d)
        {
            if (Math.Abs(d - Math.Round(d)) < floatTolerance)
                return ((long)Math.Round(d)).ToString(CultureInfo.InvariantCulture);
            return d.ToString(CultureInfo.InvariantCulture);
        }

        return value?.ToString() ?? "";
    }

    private bool CompareOutput(string actual, string expected)
    {
        actual = actual?.Trim() ?? "";
        expected = expected?.Trim() ?? "";

        if (double.TryParse(actual, NumberStyles.Any, CultureInfo.InvariantCulture, out double a) &&
            double.TryParse(expected, NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
        {
            return Math.Abs(a - b) < floatTolerance;
        }

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(actual, expected, comparison);
    }

    // ---------- UI ----------

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

    private void ShowError(string message)
    {
        if (errorText != null) errorText.text = errorMessage + message;
    }
}

// ---------- Парсер выражений ----------

public class ExpressionParser
{
    private readonly string s;
    private int pos;

    public ExpressionParser(string input) { s = input; pos = 0; }

    public double Parse()
    {
        double value = ParseExpression();
        if (pos < s.Length)
            throw new Exception($"Лишний символ на позиции {pos}: '{s[pos]}'");
        return value;
    }

    private double ParseExpression()
    {
        double left = ParseTerm();
        while (pos < s.Length && (s[pos] == '+' || s[pos] == '-'))
        {
            char op = s[pos++];
            double right = ParseTerm();
            left = op == '+' ? left + right : left - right;
        }
        return left;
    }

    private double ParseTerm()
    {
        double left = ParseFactor();
        while (pos < s.Length && (s[pos] == '*' || s[pos] == '/'))
        {
            char op = s[pos++];
            double right = ParseFactor();
            if (op == '/')
            {
                if (Math.Abs(right) < 1e-12) throw new Exception("Деление на ноль.");
                left /= right;
            }
            else left *= right;
        }
        return left;
    }

    private double ParseFactor()
    {
        if (pos >= s.Length) throw new Exception("Неожиданный конец выражения.");

        if (s[pos] == '-') { pos++; return -ParseFactor(); }
        if (s[pos] == '+') { pos++; return ParseFactor(); }

        if (s[pos] == '(')
        {
            pos++;
            double value = ParseExpression();
            if (pos >= s.Length || s[pos] != ')')
                throw new Exception("Не закрыта скобка.");
            pos++;
            return value;
        }

        int start = pos;
        while (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.'))
            pos++;

        if (start == pos)
            throw new Exception($"Ожидалось число на позиции {pos}.");

        string numStr = s.Substring(start, pos - start);
        if (!double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
            throw new Exception($"Не число: '{numStr}'");

        return num;
    }
}