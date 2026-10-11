using UnityEngine;

public static class Progress
{
    // Сохранить прогресс при нажатии кнопки
    public static void SaveLevel(int level)
    {
        int current = PlayerPrefs.GetInt("MaxLevel", 1);

        // Сохраняем только если новый уровень больше текущего
        if (level > current)
        {
            PlayerPrefs.SetInt("MaxLevel", level);
            PlayerPrefs.Save();
        }
    }

    // Проверить, открыт ли уровень
    public static bool IsLevelUnlocked(int level)
    {
        int maxLevel = PlayerPrefs.GetInt("MaxLevel", 1);
        return level <= maxLevel;
    }

    // Получить максимальный открытый уровень
    public static int GetMaxLevel()
    {
        return PlayerPrefs.GetInt("MaxLevel", 1);
    }
}