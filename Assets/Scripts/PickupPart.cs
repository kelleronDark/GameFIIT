using UnityEngine;
using UnityEngine.InputSystem;

public class PickupPart : MonoBehaviour
{
    public int partIndex;        // ID детали (можно использовать для логики)
    public Sprite partSprite;  

    private InventoryManager inventory;
    private bool playerIsNear = false;

    void Start()
    {
        inventory = FindObjectOfType<InventoryManager>();
        if (inventory == null)
            Debug.LogError("InventoryManager не найден на сцене!");
    }

    void Update()
    {
        if (playerIsNear && Keyboard.current.fKey.wasPressedThisFrame)
        {
            bool picked = inventory.PickupItem(partSprite);
            if (picked)
                Destroy(gameObject); // удаляем предмет со сцены
            else
                Debug.Log("Не удалось подобрать — инвентарь полон.");
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