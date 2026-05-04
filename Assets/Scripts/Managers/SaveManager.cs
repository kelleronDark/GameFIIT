using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private string filePath;
    private Transform player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        filePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    // Тот самый метод SaveGame, которого не хватало!
    public void SaveGame()
    {
        // 1. Ищем игрока (на случай если сцена перезагрузилась)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null) return;

        // 2. Собираем данные
        SaveData data = new SaveData();
        data.posX = player.position.x;
        data.posY = player.position.y;

        // Добавляем данные из инвентаря (если InventoryManager существует)
        if (InventoryManager.Instance != null)
        {
            data.inventoryItemNames = InventoryManager.Instance.GetCollectedItemsNames();
        }

        // 3. Сериализация в JSON
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"<color=green>Прогресс сохранен в {filePath}</color>");

        // 4. Запуск анимации иконки в углу
        if (UIAnimationController.Instance != null)
        {
            UIAnimationController.Instance.TriggerSaveIcon();
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        
        player.position = new Vector2(data.posX, data.posY);
        
        // Тут в будущем добавим логику восстановления предметов в инвентарь
    }
}