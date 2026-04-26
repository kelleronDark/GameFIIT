using UnityEngine;
using TMPro; // Добавь этот namespace

public class Door : MonoBehaviour
{
    [Header("Settings")]
    public bool isOpened = false;
    public bool requiresKey = true;
    
    [Header("References")]
    public Animator animator;
    public GameObject hintPrefab; // Префаб подсказки
    private GameObject currentHint; // Текущая подсказка над дверью

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
            ShowHint(); // Показываем подсказку
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideHint(); // Скрываем подсказку
        }
    }

    void Update()
    {
        // Обновляем текст подсказки, если она активна
        if (currentHint != null && playerInRange)
        {
            TextMeshProUGUI hintText = currentHint.GetComponentInChildren<TextMeshProUGUI>();
            if (hintText != null)
            {
                if (requiresKey && !KeyInventory.Instance.HasKeys())
                {
                    hintText.text = "Требуется ключ";
                }
                else
                {
                    hintText.text = "Нажмите F";
                }
            }
        }
    }

    public void TryOpen()
    {
        if (isOpened) return;

        if (requiresKey)
        {
            if (KeyInventory.Instance != null && KeyInventory.Instance.HasKeys())
            {
                KeyInventory.Instance.UseKey();
                OpenDoor();
            }
            else
            {
                Debug.Log("Нужен ключ, чтобы открыть эту дверь!");
                // Можно добавить звук ошибки или визуальный эффект
            }
        }
        else
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        isOpened = true;
        Debug.Log("Дверь открыта!");

        if (animator != null)
            animator.SetBool("IsOpen", true);

        Collider2D doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
            doorCollider.enabled = false;

        HideHint(); // Скрываем подсказку после открытия
    }

    void ShowHint()
    {
        if (isOpened || currentHint != null) return;

        // Создаём подсказку
        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        // === НОВОЕ: Находим камеру и назначаем её в Canvas ===
        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            // Ищем главную камеру в сцене
            Camera mainCamera = Camera.main; // или FindObjectOfType<Camera>()
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
                Debug.Log("✅ Камера назначена в Canvas подсказки!");
            }
            else
            {
                Debug.LogError("❌ Не найдена камера в сцене! Проверь тег 'MainCamera'");
            }
        }

        // Обновляем текст
        TextMeshProUGUI hintText = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (hintText != null)
        {
            if (requiresKey && !KeyInventory.Instance.HasKeys())
            {
                hintText.text = "Требуется ключ";
            }
            else
            {
                hintText.text = "Нажмите F";
            }
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

    [ContextMenu("Test Open Door")]
    void TestOpen()
    {
        TryOpen();
    }
}