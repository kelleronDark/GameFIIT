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
        currentKeys = 0; // Сброс на всякий случай

        foreach (var slot in keySlots)
        {
            if (slot == null) continue;

            slot.enabled = true; // Фон всегда виден

            Transform keyIconTransform = slot.transform.Find("KeyIcon");
            if (keyIconTransform != null)
            {
                Image keyIcon = keyIconTransform.GetComponent<Image>();
                if (keyIcon != null)
                    keyIcon.enabled = false; // Иконка скрыта по умолчанию
            }
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
        // Проходим по всем 4 слотам
        for (int i = 0; i < keySlots.Length; i++)
        {
            if (keySlots[i] != null)
            {
                // 1. ОБЪЯВЛЯЕМ переменную hasKey здесь!
                // Она true, если индекс слота меньше текущего количества ключей
                bool hasKey = (i < currentKeys);

                // 2. Находим иконку внутри слота
                Transform keyIconTransform = keySlots[i].transform.Find("KeyIcon");
                
                if (keyIconTransform != null)
                {
                    Image keyIcon = keyIconTransform.GetComponent<Image>();
                    if (keyIcon != null)
                    {
                        // Включаем/выключаем иконку в зависимости от hasKey
                        keyIcon.enabled = hasKey;
                        
                        // Если иконка включена, ставим ей правильный спрайт
                        if (hasKey && keySprite != null)
                        {
                            keyIcon.sprite = keySprite;
                        }
                    }
                }

                // 3. ЗАПУСКАЕМ ПУЛЬСАЦИЮ только для нового ключа
                // Используем ту самую переменную hasKey, которую мы создали выше
                if (hasKey && i == currentKeys - 1) 
                {
                    KeySlotPulse pulser = keySlots[i].GetComponent<KeySlotPulse>();
                    
                    if (pulser != null)
                    {
                        pulser.Pulse();
                    }
                }
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