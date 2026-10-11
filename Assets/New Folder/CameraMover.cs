using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public static CameraMover Instance { get; private set; }

    [System.Serializable]
    public class CameraPoint
    {
        public string name;      // например "up", "down", "default", "left"
        public Transform point;  // куда лететь
    }

    [Header("Точки камеры")]
    public List<CameraPoint> points = new List<CameraPoint>();

    [Header("Плавность")]
    [Tooltip("Чем больше — тем медленнее и плавнее")]
    public float smoothTime = 0.35f;
    [Tooltip("Порог, после которого камера 'прилипает' к цели")]
    public float snapThreshold = 0.02f;

    [Header("Границы (опционально)")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-50, -50);
    public Vector2 maxBounds = new Vector2(50, 50);

    private Dictionary<string, Transform> pointMap = new Dictionary<string, Transform>();
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        targetPosition = transform.position;

        // Строим словарь для быстрого доступа
        foreach (var p in points)
        {
            if (p != null && !string.IsNullOrEmpty(p.name) && p.point != null)
            {
                if (!pointMap.ContainsKey(p.name))
                    pointMap.Add(p.name, p.point);
                else
                    Debug.LogWarning($"CameraMover: точка с именем '{p.name}' уже есть");
            }
        }
    }

    void Update()
    {
        Vector3 newPos = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // Если почти доехали — ставим точно
        if (Vector3.Distance(newPos, targetPosition) < snapThreshold)
        {
            newPos = targetPosition;
            velocity = Vector3.zero;
        }

        newPos.z = transform.position.z;
        transform.position = newPos;
    }

    /// <summary>Переместиться в точку по имени</summary>
    public void MoveTo(string pointName)
    {
        if (pointMap.TryGetValue(pointName, out Transform t))
        {
            targetPosition = new Vector3(t.position.x, t.position.y, transform.position.z);
        }
        else
        {
            Debug.LogWarning($"CameraMover: точка '{pointName}' не найдена");
        }
    }

    /// <summary>Переместиться в произвольную мировую позицию</summary>
    public void MoveToPosition(Vector3 worldPos)
    {
        targetPosition = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    /// <summary>Мгновенно переместиться (без анимации)</summary>
    public void SnapTo(string pointName)
    {
        if (pointMap.TryGetValue(pointName, out Transform t))
        {
            targetPosition = new Vector3(t.position.x, t.position.y, transform.position.z);
            transform.position = targetPosition;
            velocity = Vector3.zero;
        }
    }
}