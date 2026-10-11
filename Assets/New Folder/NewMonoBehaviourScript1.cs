using System;
using UnityEngine;

public static class PlayerName
{
    private const string Key = "PlayerName";

    public static event Action OnChanged;

    public static bool HasName => PlayerPrefs.HasKey(Key);

    public static string Value
    {
        get => PlayerPrefs.GetString(Key, "");
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            PlayerPrefs.SetString(Key, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }
}