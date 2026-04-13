using UnityEngine;
using UnityEngine.InputSystem; // обязательно добавить

public class PickupPart : MonoBehaviour
{
    public int partIndex;
    private InventoryManager inventory;
    private bool playerIsNear = false;

    void Start()
    {
        inventory = FindObjectOfType<InventoryManager>();
        if (inventory == null)
            Debug.LogError("InventoryManager не найден!");
    }

    void Update()
    {
        // Новый способ проверки нажатия F
        if (playerIsNear && Keyboard.current.fKey.wasPressedThisFrame)
        {
            inventory.PickupItem(partIndex);
            Destroy(gameObject);
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