using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // Синглтон — удобно для доступа из PickupPart
    
    private Sprite[] savedSpritesSnapshot = new Sprite[4];
    
    private PlayerController playerController;

    [Header("UI Slots")]
    public Image[] slots; // Перетащи сюда Image от Slot 1–4

    [Header("Settings")]
    public int maxItems = 4; // Максимум предметов
    private Sprite[] collectedSprites = new Sprite[4]; // Храним спрайты подобранных предметов
    
    private int currentKeys = 0;      // Текущее кол-во ключей
    private int savedKeysSnapshot = 0; // Снимок для отката

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
            playerController = playerObj.GetComponent<PlayerController>();

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void SaveInventoryState()
    {
        // Копируем текущие спрайты в массив сохранения
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            savedSpritesSnapshot[i] = collectedSprites[i];
        }
        Debug.Log("Снимок инвентаря для сохранения сделан.");
    }
    
    public void ResetInventory()
    {
        // 1. Выбрасываем то, что в руках (твой метод уже есть)
        if (playerController != null)
            playerController.ForceDropItem();
        
        currentKeys = savedKeysSnapshot;

        // 2. Откатываем массив спрайтов к моменту сохранения
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            collectedSprites[i] = savedSpritesSnapshot[i];
        
            // 3. Обновляем визуальные слоты UI
            if (collectedSprites[i] != null)
            {
                slots[i].sprite = collectedSprites[i];
                slots[i].enabled = true;
            }
            else
            {
                slots[i].sprite = null;
                slots[i].enabled = false;
            }
        }
        Debug.Log("Инвентарь откачен к состоянию последнего сохранения.");
    }

    void Start()
    {
        // Инициализация: очищаем все слоты
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].sprite = null;       // Убираем любой дефолтный спрайт
                slots[i].enabled = false;     // Скрываем изображение (фон можно оставить через другой Image или Canvas)
                collectedSprites[i] = null;
            }
        }
    }

    /// <summary>
    /// Подобрать предмет — добавить его спрайт в первый свободный слот
    /// </summary>
    /// <param name="itemSprite">Спрайт детали</param>
    /// <returns>true если успешно подобрано, false если инвентарь полон</returns>
    public bool PickupItem(Sprite itemSprite)
    {
        if (itemSprite == null)
        {
            Debug.LogWarning("Попытка подобрать предмет с null спрайтом!");
            return false;
        }

        // Ищем первый свободный слот
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            if (collectedSprites[i] == null)
            {
                collectedSprites[i] = itemSprite;
                slots[i].sprite = itemSprite;
                slots[i].enabled = true; // Показываем иконку

                Debug.Log($"Подобрана деталь в слот {i}: {itemSprite.name}");
                return true;
            }
        }

        Debug.LogWarning("Инвентарь полон! Нельзя подобрать ещё одну деталь.");
        return false;
    }

    /// <summary>
    /// Очистить весь инвентарь (для тестов или сброса)
    /// </summary>
    public void ClearInventory()
    {
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            collectedSprites[i] = null;
            if (slots[i] != null)
            {
                slots[i].sprite = null;
                slots[i].enabled = false;
            }
        }
    }

    /// <summary>
    /// Проверить, занят ли слот
    /// </summary>
    public bool IsSlotOccupied(int index)
    {
        if (index < 0 || index >= collectedSprites.Length) return false;
        return collectedSprites[index] != null;
    }
    
    public List<string> GetCollectedItemsNames()
    {
        List<string> names = new List<string>();
        foreach (var sprite in collectedSprites)
        {
            if (sprite != null) names.Add(sprite.name);
        }
        return names;
    }
    
    public bool HasItem(string itemName)
    {
        // Проверяем массив текущих собранных спрайтов
        foreach (var sprite in collectedSprites)
        {
            if (sprite != null && sprite.name == itemName)
            {
                return true;
            }
        }
        return false;
    }
    
    public bool isLoaded = false;
    
    public void LoadInventoryFromNames(List<string> itemNames)
    {
        isLoaded = false; // Начинаем загрузку
        ClearInventory();
        
        if (itemNames == null || itemNames.Count == 0) 
        {
            isLoaded = true; 
            return; 
        }
        
        Debug.Log($"[LOAD] Пытаюсь загрузить {itemNames.Count} предметов из файла.");
        
        // Загружаем ВСЕ спрайты из файла атласа в папке Resources
        // Замени "YourAtlasName" на реальное имя твоего PNG файла (без расширения)
        Sprite[] allSprites = Resources.LoadAll<Sprite>("generated-removebg-preview"); 
        
        if (allSprites == null || allSprites.Length == 0) {
            Debug.LogError("[LOAD] КРИТИЧЕСКАЯ ОШИБКА: Спрайты по пути Resources/generated-removebg-preview не найдены! Проверь имя файла.");
        } else {
            Debug.Log($"[LOAD] В атласе найдено {allSprites.Length} спрайтов.");
        }

        for (int i = 0; i < itemNames.Count && i < collectedSprites.Length; i++)
        {
            string targetName = itemNames[i];
            // Ищем в массиве спрайт с нужным именем
            Sprite found = System.Array.Find(allSprites, s => s.name == itemNames[i]);

            if (found != null)
            {
                collectedSprites[i] = found;
                
                slots[i].sprite = found;
                slots[i].enabled = true;
                
                Color c = slots[i].color;
                c.a = 1f;
                slots[i].color = c;
                
                slots[i].gameObject.SetActive(true); // Активируем объект
                
                Debug.Log($"[LOAD] Предмет {targetName} успешно восстановлен в слот {i}");
            }
            else
            {
                Debug.LogWarning($"[LOAD] Не удалось найти спрайт с именем '{targetName}' в атласе!");
            }
        }
        
        SaveInventoryState();
        isLoaded = true; // Загрузка завершена!
        
        foreach (var slotImage in slots)
        {
            if (slotImage.sprite != null)
            {
                slotImage.enabled = false; // Выключаем
                slotImage.enabled = true;  // Включаем (это заставляет Unity перерисовать Image)
            
                // Убеждаемся, что цвет не прозрачный
                Color c = slotImage.color;
                c.a = 1f; 
                slotImage.color = c;
            
                // Если у тебя есть дочерние объекты или текст, можно обновить и их
                Canvas.ForceUpdateCanvases(); 
            }
        }
        
        // DebugLogInventory();
    }
    
    
    
    [ContextMenu("Debug Log Inventory Content")] // Позволит нажать правой кнопкой на компонент в инспекторе
    public void DebugLogInventory()
    {
        Debug.Log("<color=cyan>--- ИНСПЕКЦИЯ ИНВЕНТАРЯ ---</color>");
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            if (collectedSprites[i] != null)
            {
                Debug.Log($"Слот {i}: [Имя: {collectedSprites[i].name}] [Спрайт назначен: {slots[i].sprite != null}] [Видимость: {slots[i].enabled}]");
            }
            else
            {
                Debug.Log($"Слот {i}: ПУСТО");
            }
        }
        Debug.Log("<color=cyan>---------------------------</color>");
    }
}