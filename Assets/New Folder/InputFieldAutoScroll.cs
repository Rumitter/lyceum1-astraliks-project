using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldAutoScroll : MonoBehaviour
{
    [Tooltip("Прокручивать вниз при каждом изменении текста")]
    [SerializeField] bool scrollOnValueChanged = true;

    [Tooltip("Прокручивать вниз при получении фокуса")]
    [SerializeField] bool scrollOnFocus = true;

    private TMP_InputField inputField;
    private ScrollRect scrollRect;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();

        // Ищем ScrollRect: либо на этом объекте, либо у родителя/дочернего
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null) scrollRect = GetComponentInParent<ScrollRect>();
        if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();

        if (scrollRect == null)
        {
            Debug.LogWarning($"[InputFieldAutoScroll] Не найден ScrollRect у {name}. " +
                             "Добавьте ScrollRect в иерархию InputField.");
            return;
        }

        if (scrollOnValueChanged)
            inputField.onValueChanged.AddListener(_ => ScrollToBottom());

        if (scrollOnFocus)
            inputField.onSelect.AddListener(_ => ScrollToBottom());
    }

    /// <summary>
    /// Прокручивает ScrollRect вниз (к последней строке).
    /// </summary>
    public void ScrollToBottom()
    {
        if (scrollRect == null) return;

        // Ждём кадр, чтобы TMP успел пересчитать размеры текста
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}