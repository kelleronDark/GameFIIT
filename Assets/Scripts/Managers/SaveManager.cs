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

        PlayerController pc = player.GetComponent<PlayerController>();
        SaveData data = new SaveData();
        
        if (pc != null)
        {
            Vector3 cpPos = pc.GetLastCheckpointPos(); // Нам нужно добавить этот геттер
            data.posX = cpPos.x;
            data.posY = cpPos.y;
        }

        // Добавляем данные из инвентаря (если InventoryManager существует)
        if (InventoryManager.Instance != null)
        {
            data.inventoryItemNames = InventoryManager.Instance.GetCollectedItemsNames();
        }
        
        data.keyCount = KeyInventory.Instance.GetKeyCount();
        
        if (data.inventoryItemNames != null)
        {
            Debug.Log($"[SAVE] Сохраняю предметов: {data.inventoryItemNames.Count}. Первый: {(data.inventoryItemNames.Count > 0 ? data.inventoryItemNames[0] : "пусто")}");
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
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.LoadInventoryFromNames(data.inventoryItemNames);
        }
            
        if (KeyInventory.Instance != null)
        {
            KeyInventory.Instance.RestoreKeys(data.keyCount);
        }

        // Ищем игрока, если ссылка потерялась при смене сцены
        if (player == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            player.position = new Vector2(data.posX, data.posY);
            
            var cam = FindFirstObjectByType<CameraFollow>();
            if (cam != null) 
            {
                cam.target = player; // Гарантируем, что таргет назначен
                cam.Warp();
            }
            
            Debug.Log("Позиция игрока восстановлена.");
        }
    }
    
    public bool HasSaveFile()
    {
        return File.Exists(filePath);
    }

// 2. Удаление файла (нужна для кнопки "Новая игра")
    public void DeleteSaveFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("<color=red>Файл сохранения удален для новой игры.</color>");
        }
    }
    
    private void OnEnable()
    {
        // Подписываемся на событие загрузки сцены
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Отписываемся, чтобы не было утечек памяти
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Проверяем: если это сцена игры (индекс 1) и у нас есть что загружать
        if (scene.buildIndex == 1 && HasSaveFile())
        {
            Debug.Log("Игровая сцена загружена, восстанавливаем данные...");
            LoadGame(); // Твой метод загрузки
        }
    }
    
    // Добавь этот метод для сохранения при подборе запчастей
    public void QuickSave()
    {
        // Сначала обновляем "снимки" в памяти
        if (InventoryManager.Instance != null) InventoryManager.Instance.SaveInventoryState();
        if (KeyInventory.Instance != null) KeyInventory.Instance.SaveKeyState();

        // Затем пишем в файл
        SaveGame();
    }
}