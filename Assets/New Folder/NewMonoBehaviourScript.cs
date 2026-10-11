using System;
using UnityEngine;

public class ResetClickReceiver : MonoBehaviour
{
    public event Action OnClicked;

    void OnMouseDown()
    {
        OnClicked?.Invoke();
    }
}