using UnityEngine;
using UnityEngine.UI;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory Instance; // Singleton для доступа из любого места

    [Header("Settings")]
    public int maxKeys = 4; // Максимум ключей
    private int currentKeys = 0; // Текущее количество

    [Header("UI Slots")]
    public Image[] keySlots; // Массив из 4 слотов в UI
    public Sprite keySprite; // Спрайт ключа для отображения

    [Header("Debug")]
    public bool showDebugLogs = true;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Инициализация UI
        InitializeUI();
    }

    void InitializeUI()
    {
        // Очищаем все слоты при старте
        foreach (var slot in keySlots)
        {
            if (slot != null)
                slot.enabled = false;
        }
    }

    /// <summary>
    /// Добавить ключ в инвентарь
    /// </summary>
    /// <returns>true если ключ добавлен, false если инвентарь полон</returns>
    public bool AddKey()
    {
        if (currentKeys >= maxKeys)
        {
            if (showDebugLogs)
                Debug.LogWarning("Инвентарь ключей полон! Нельзя взять больше ключей.");
            return false;
        }

        currentKeys++;
        UpdateUI();
        
        if (showDebugLogs)
            Debug.Log($"Ключ подобран! Всего ключей: {currentKeys}/{maxKeys}");
        
        return true;
    }

    /// <summary>
    /// Использовать ключ (удалить один)
    /// </summary>
    /// <returns>true если ключ был использован, false если ключей нет</returns>
    public bool UseKey()
    {
        if (currentKeys <= 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("У игрока нет ключей!");
            return false;
        }

        currentKeys--;
        UpdateUI();
        
        if (showDebugLogs)
            Debug.Log($"Ключ использован. Осталось ключей: {currentKeys}/{maxKeys}");
        
        return true;
    }

    /// <summary>
    /// Проверить наличие ключей
    /// </summary>
    public bool HasKeys()
    {
        return currentKeys > 0;
    }

    /// <summary>
    /// Получить текущее количество ключей
    /// </summary>
    public int GetKeyCount()
    {
        return currentKeys;
    }

    void UpdateUI()
    {
        // Обновляем визуальное отображение слотов
        for (int i = 0; i < keySlots.Length; i++)
        {
            if (keySlots[i] != null)
            {
                // Показываем слот если есть ключ
                keySlots[i].enabled = (i < currentKeys);
                
                // Устанавливаем спрайт ключа
                if (keySprite != null)
                    keySlots[i].sprite = keySprite;
            }
        }
    }

    // Для отладки - можно вызвать из консоли
    [ContextMenu("Add Test Key")]
    void AddTestKey()
    {
        AddKey();
    }

    [ContextMenu("Remove Test Key")]
    void RemoveTestKey()
    {
        UseKey();
    }
}