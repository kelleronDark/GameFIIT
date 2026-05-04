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
        StartCoroutine(CheckInventoryDelayed());
    }
    
    IEnumerator CheckInventoryDelayed()
    {
        // Ждем чуть-чуть, пока отработает загрузка из SaveManager
        yield return new WaitForSeconds(0.2f);

        if (inventory != null && inventory.HasItem(partSprite.name))
        {
            Debug.Log($"[CLEANUP] Предмет {partSprite.name} уже в кармане. Удаляю с земли.");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!hasChecked && inventory != null && inventory.isLoaded)
        {
            if (inventory.HasItem(partSprite.name))
            {
                Debug.Log($"[SaveSystem] Предмет {partSprite.name} уже в инвентаре. Самоуничтожение.");
                Destroy(gameObject);
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