using System.Collections;
using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    [Header("Имя точки в CameraMover, куда лететь при клике")]
    public string targetPointName = "default";

    [Header("Объекты, которые нужно СКРЫТЬ при клике")]
    [Tooltip("Перетащите сюда объекты, которые должны исчезнуть после нажатия")]
    public GameObject[] objectsToHide;

    [Header("Объекты, которые нужно ВКЛЮЧИТЬ при клике")]
    [Tooltip("Перетащите сюда объекты, которые должны появиться после нажатия")]
    public GameObject[] objectsToShow;

    [Header("Задержка перед скрытием (в секундах)")]
    public float hideDelay = 2f;

    [Header("Задержка перед включением (в секундах)")]
    public float showDelay = 0f;

    private bool wasClicked = false;

    void OnMouseDown()
    {
        // Перемещение камеры
        if (CameraMover.Instance != null)
        {
            CameraMover.Instance.MoveTo(targetPointName);
        }

        // Включение объектов
        if (showDelay > 0f)
            StartCoroutine(ShowAfterDelay());
        else
            ShowObjects();

        // Скрытие объектов
        StartCoroutine(HideAfterDelay());
    }

    // ---------- Включение ----------

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);
        ShowObjects();
    }

    private void ShowObjects()
    {
        if (objectsToShow == null) return;

        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    // ---------- Скрытие ----------

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        if (objectsToHide == null) yield break;

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}