using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    [Header("Settings")]
    public bool isOpened = false;
    public bool requiresKey = true;
    
    [Header("References")]
    public Animator animator;
    public GameObject hintPrefab;
    public AudioSource audioSource;
    public GameObject sparklesEffect; // <-- НОВОЕ: блёстки для двери

    private GameObject currentHint;
    private bool playerInRange = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    
        // АВТОМАТИЧЕСКОЕ СОЗДАНИЕ БЛЁСТОК ДЛЯ ДВЕРИ
        if (sparklesEffect != null)
        {
            // Создаём копию префаба прямо на сцене
            GameObject instance = Instantiate(sparklesEffect, transform.position + Vector3.up * 1.2f, Quaternion.identity);
            instance.transform.SetParent(transform); // Делаем дочерним
            instance.transform.localPosition = Vector3.up * 1.2f; // Позиционируем над дверью
            sparklesEffect = instance; // Заменяем ссылку на префаб ссылкой на объект
        }
    
        UpdateSparkles();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowHint();
            UpdateSparkles(); // Показываем блёстки, когда игрок рядом
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideHint();
            UpdateSparkles(); // Скрываем, когда игрок ушёл
        }
    }

    void Update()
    {
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

        if (audioSource != null)
            audioSource.Play();

        if (animator != null)
            animator.SetBool("IsOpen", true);

        Collider2D doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
            doorCollider.enabled = false;

        HideHint();
        UpdateSparkles(); // Скрываем блёстки после открытия
    }

    // --- НОВЫЙ МЕТОД: Управление блёстками ---
    
    private void UpdateSparkles()
    {
        if (sparklesEffect != null)
        {
            // Показываем блёстки, если дверь ЗАКРЫТА (независимо от игрока!)
            bool shouldShow = !isOpened;
            sparklesEffect.SetActive(shouldShow);
        
            var particle = sparklesEffect.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                if (shouldShow && !particle.isPlaying)
                    particle.Play();
                else if (!shouldShow && particle.isPlaying)
                    particle.Stop();
            }
        }
    }

    void ShowHint()
    {
        if (isOpened || currentHint != null) return;

        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                canvas.worldCamera = mainCamera;
        }

        TextMeshProUGUI hintText = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (hintText != null)
        {
            if (requiresKey && !KeyInventory.Instance.HasKeys())
                hintText.text = "Требуется ключ";
            else
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

    [ContextMenu("Test Open Door")]
    void TestOpen()
    {
        TryOpen();
    }
}