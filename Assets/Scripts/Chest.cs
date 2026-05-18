using UnityEngine;
using TMPro;

public class Chest : MonoBehaviour
{
    [Header("Settings")]
    public bool isOpened = false;
    public bool containsKey = true;
    
    [Header("References")]
    public Animator animator;
    public GameObject hintPrefab;
    public AudioSource audioSource;
    public GameObject sparklesEffect; // <-- НОВОЕ: ссылка на блёстки

    private GameObject currentHint;
    private bool playerInRange = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    
        // АВТОМАТИЧЕСКОЕ СОЗДАНИЕ БЛЁСТОК
        if (sparklesEffect != null)
        {
            // Создаём копию префаба прямо на сцене
            GameObject instance = Instantiate(sparklesEffect, transform.position + Vector3.up, Quaternion.identity);
            instance.transform.SetParent(transform); // Делаем дочерним
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

        if (audioSource != null)
            audioSource.Play();

        if (animator != null)
            animator.SetBool("IsOpen", true);

        if (containsKey && KeyInventory.Instance != null)
        {
            bool added = KeyInventory.Instance.AddKey();
            if (added) Debug.Log("Ключ добавлен в инвентарь!");
            else Debug.LogWarning("Инвентарь полон! Ключ не подобран.");
        }

        // Скрываем подсказку и блёстки после открытия
        HideHint();
        UpdateSparkles();
    }

    // --- НОВЫЙ МЕТОД: Управление блёстками ---
    
    private void UpdateSparkles()
    {
        if (sparklesEffect != null)
        {
            // Показываем блёстки только если сундук ещё НЕ открыт
            sparklesEffect.SetActive(!isOpened);
            
            // Если это Particle System, можно управлять воспроизведением
            var particle = sparklesEffect.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                if (!isOpened && !particle.isPlaying)
                    particle.Play();
                else if (isOpened && particle.isPlaying)
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
            hintText.text = "Нажмите F";
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