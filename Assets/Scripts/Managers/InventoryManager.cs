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
        playerController.ForceDropItem(); 

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
}