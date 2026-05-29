using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupPart : MonoBehaviour
{
    public int partIndex;
    public Sprite partSprite;
    private InventoryManager inventory;
    private bool playerIsNear = false;
    private bool hasChecked = false;

    [Header("Visual")]
    public GameObject auraGlowPrefab;
    
    void Start()
    {
        inventory = InventoryManager.Instance;
        if (auraGlowPrefab != null)
        {
            GameObject aura = Instantiate(auraGlowPrefab, transform);
            aura.transform.localPosition = Vector3.zero;
            aura.name = "AuraGlow_Instance";
        }
        
        Invoke(nameof(CheckIfAlreadyPicked), 0.15f);
    }
    
    void OnEnable()
    {
        CheckIfAlreadyPicked();
    }
    
    void CheckIfAlreadyPicked()
    {
        if (inventory == null) inventory = InventoryManager.Instance;

        if (inventory != null && partSprite != null)
        {
            if (inventory.HasItem(partSprite.name))
            {
                Debug.Log($"Деталь {partSprite.name} уже в инвентаре. Удаляю объект со сцены.");
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (inventory == null)
        {
            inventory = InventoryManager.Instance;
            return;
        }
        
        if (!hasChecked && inventory != null && inventory.isLoaded)
        {
            if (inventory.HasItem(partSprite.name))
            {
                Debug.Log($"Предмет {partSprite.name} уже в инвентаре. Самоуничтожение.");
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
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.QuickSave();
                    Debug.Log($"Запчасть {partIndex} подобрана. Игра сохранена.");
                }

                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Не удалось подобрать - инвентарь полон.");
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