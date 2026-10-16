using System.Collections;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public static ClickManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // НЕ DontDestroyOnLoad, если не надо跨 сцены, но можно
    }

    public void ScheduleHide(GameObject[] objs, float delay)
    {
        if (objs == null || objs.Length == 0) return;
        StartCoroutine(HideRoutine(objs, delay));
    }

    public void ScheduleShow(GameObject[] objs, float delay)
    {
        if (objs == null || objs.Length == 0) return;
        StartCoroutine(ShowRoutine(objs, delay));
    }

    private IEnumerator HideRoutine(GameObject[] objs, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        foreach (var o in objs) if (o != null) o.SetActive(false);
    }

    private IEnumerator ShowRoutine(GameObject[] objs, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        foreach (var o in objs) if (o != null) o.SetActive(true);
    }
}