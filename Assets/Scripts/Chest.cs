using UnityEngine;
using TMPro;

public class Chest : MonoBehaviour
{
    [Header("Settings")]
    public bool isOpened = false;
    public bool containsKey = true;
    
    [Header("References")]
    public Animator animator;
    public GameObject hintPrefab; // Префаб подсказки
    private GameObject currentHint;

    private bool playerInRange = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowHint();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideHint();
        }
    }

    public void OpenChest()
    {
        if (isOpened) return;

        isOpened = true;
        Debug.Log("Сундук открыт!");

        if (animator != null)
            animator.SetBool("IsOpen", true);

        if (containsKey && KeyInventory.Instance != null)
        {
            bool added = KeyInventory.Instance.AddKey();
            if (added) Debug.Log("Ключ добавлен в инвентарь!");
            else Debug.LogWarning("Инвентарь полон! Ключ не подобран.");
        }

        HideHint(); // Скрываем подсказку после открытия
    }

    void ShowHint()
    {
        if (isOpened || currentHint != null) return;

        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        // === НАЗНАЧАЕМ КАМЕРУ ===
        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
            }
            else
            {
                Debug.LogError("❌ Камера не найдена для подсказки сундука!");
            }
        }

        TextMeshProUGUI hintText = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (hintText != null)
        {
            hintText.text = "Нажмите F";
        }
    }

    void HideHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
    }

    [ContextMenu("Open Chest")]
    void DebugOpenChest()
    {
        OpenChest();
    }
}