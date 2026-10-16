using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ResetClickReceiver : MonoBehaviour
{
    public event Action OnClicked;

    private Camera cam;
    private Collider2D col;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Мышь (ПК)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryClick(Mouse.current.position.ReadValue());
        }

        // Тач (мобилка)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    void TryClick(Vector2 screenPos)
    {
        if (cam == null) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        Vector2 point2D = new Vector2(worldPos.x, worldPos.y);

        if (col.OverlapPoint(point2D))
        {
            OnClicked?.Invoke();
            Debug.Log($"[ResetClickReceiver] Клик по {gameObject.name}");
        }
    }
}