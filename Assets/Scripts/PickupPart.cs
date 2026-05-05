using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupPart : MonoBehaviour
{
    public int partIndex;        // ID детали (можно использовать для логики)
    public Sprite partSprite;  

    private InventoryManager inventory;
    private bool playerIsNear = false;
    private bool hasChecked = false;

    void Start()
    {
        inventory = InventoryManager.Instance;
        // Дублируем проверку через небольшую паузу для уверенности после загрузки JSON
        Invoke(nameof(CheckIfAlreadyPicked), 0.15f);
    }
    
    void OnEnable()
    {
        // Повторная проверка при активации объекта
        CheckIfAlreadyPicked();
    }
    
    void CheckIfAlreadyPicked()
    {
        if (inventory == null) inventory = InventoryManager.Instance;

        if (inventory != null && partSprite != null)
        {
            if (inventory.HasItem(partSprite.name))
            {
                Debug.Log($"[CLEANUP] Деталь {partSprite.name} уже в инвентаре. Удаляю объект со сцены.");
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (inventory == null)
        {
            inventory = InventoryManager.Instance;
            return; // Пока инвентарь не найден, Update дальше не идет
        }
        
        if (!hasChecked && inventory != null && inventory.isLoaded)
        {
            if (inventory.HasItem(partSprite.name))
            {
                Debug.Log($"[SaveSystem] Предмет {partSprite.name} уже в инвентаре. Самоуничтожение.");
                Destroy(gameObject);
                return;
            }
            hasChecked = true;
        }
        
        if (playerIsNear && Keyboard.current.fKey.wasPressedThisFrame)
        {
            bool picked = inventory.PickupItem(partSprite);
            if (picked)
            {
                // 2. Если подобрали — сохраняем прогресс (позицию, инвентарь, ключи)
                if (SaveManager.Instance != null)
                {
                    // Вызываем наш надежный метод быстрой записи
                    SaveManager.Instance.QuickSave();
                    Debug.Log($"Запчасть {partIndex} подобрана. Игра сохранена.");
                }

                // 3. Удаляем объект со сцены
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Не удалось подобрать — инвентарь полон.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = false;
    }
}