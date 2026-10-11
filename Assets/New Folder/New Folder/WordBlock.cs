using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WordBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public string word;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector2 originalPosition;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform currentParent;

    public Transform CurrentParent => currentParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(string wordText, Transform parent)
    {
        word = wordText;
        GetComponentInChildren<TextMeshProUGUI>().text = wordText;
        originalParent = parent;
        currentParent = parent;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform); // поверх всего
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Ищем ближайшую зону под курсором
        DropZone zone = FindDropZone(eventData.position);
        if (zone != null && zone.AcceptBlock(this))
        {
            transform.SetParent(zone.transform);
            rectTransform.anchoredPosition = Vector2.zero;
            currentParent = zone.transform;
        }
        else
        {
            // вернуть назад
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
            currentParent = originalParent;
        }
    }

    private DropZone FindDropZone(Vector2 screenPos)
    {
        var zones = FindObjectsOfType<DropZone>();
        foreach (var z in zones)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                z.GetComponent<RectTransform>(), screenPos, canvas.worldCamera))
                return z;
        }
        return null;
    }
}