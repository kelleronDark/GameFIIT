using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LeverControl : MonoBehaviour
{
    public Animator doorAnimator;
    public BoxCollider2D doorCollider;
    public Animator leverAnimator;
    
    [Header("Trap")]
    public BayonetTrap bayonetTrap;
    public bool startsOpened = false; 

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("UI Hint")]
    public GameObject hintPrefab;
    
    [Header("Sparkles")] // <-- НОВОЕ: поле для блёсток
    public GameObject sparklesEffect;
    
    // <-- НОВОЕ: Визуальный отклик двери
    [Header("Door Feedback")]
    public SpriteRenderer doorSpriteRenderer; // Перетащи сюда SpriteRenderer двери
    public GameObject doorOpenParticles;      // Префаб частиц для вспышки
    public Color flashColor = new Color(1f, 1f, 1f, 0.8f); // Цвет вспышки
    public float flashDuration = 0.2f;        // Длительность вспышки

    private GameObject currentHint;
    private bool isPlayerNearby = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // АВТОМАТИЧЕСКОЕ СОЗДАНИЕ БЛЁСТОК ДЛЯ РЫЧАГА
        if (sparklesEffect != null)
        {
            GameObject instance = Instantiate(sparklesEffect, transform.position + Vector3.up * 0.8f, Quaternion.identity);
            instance.transform.SetParent(transform);
            instance.transform.localPosition = Vector3.up * 0.8f;
            sparklesEffect = instance;
        }
        
        if (bayonetTrap != null)
        {
            bool isAlreadyDeactivated = SaveManager.Instance != null && SaveManager.Instance.IsBayonetTrapDeactivated();
            if (isAlreadyDeactivated)
            {
                ApplyState(true);
                return;
            }
        }

        ApplyState(startsOpened);
        UpdateSparkles(); // Инициализация состояния блёсток
    }
    
    private void ApplyState(bool isOpen)
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", isOpen);
            doorAnimator.Play(isOpen ? "Gate_Opened" : "Gate_Closed", 0, 1f);
        }

        if (doorCollider != null) doorCollider.enabled = !isOpen;

        if (bayonetTrap != null)
        {
            bayonetTrap.SetState(isOpen); 
        }

        if (leverAnimator != null)
        {
            leverAnimator.SetBool("isActivated", isOpen);
            leverAnimator.Play(isOpen ? "Lever_On" : "Lever_Off", 0, 1f);
        }
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleGate();
        }
    }

    private void ToggleGate()
    {
        if (audioSource != null)
            audioSource.Play();

        if (doorAnimator != null)
        {
            bool newState = !doorAnimator.GetBool("isOpen");
            doorAnimator.SetBool("isOpen", newState);

            if (doorCollider != null)
                doorCollider.enabled = !newState;

            if (leverAnimator != null)
                leverAnimator.SetBool("isActivated", newState);

            // <-- НОВОЕ: Визуальный отклик при открытии двери
            if (newState) // Только при открытии, не при закрытии
            {
                PlayDoorOpenFeedback();
            }
            
            Debug.Log("Рычаг и дверь переключены. Состояние открыто: " + newState);
        }

        if (bayonetTrap != null)
        {
            bayonetTrap.ToggleTrap();

            if (leverAnimator != null)
                leverAnimator.SetBool("isActivated", !bayonetTrap.IsActive);
            
            if (SaveManager.Instance != null)
                SaveManager.Instance.SetBayonetTrapState(!bayonetTrap.IsActive);

            Debug.Log("Ловушка переключена.");
        }

        HideHint();
        UpdateSparkles(); // Обновляем блёстки после переключения
    }
    
    // <-- НОВЫЙ МЕТОД: Визуальный отклик двери
    private void PlayDoorOpenFeedback()
    {
        Debug.Log("🚪 [FEEDBACK] Door open feedback triggered!");
    
        // 1. Вспышка цвета на спрайте двери
        if (doorSpriteRenderer != null)
        {
            Debug.Log("✨ [FEEDBACK] SpriteRenderer found: " + doorSpriteRenderer.name);
            Debug.Log("🎨 [FEEDBACK] Original color: " + doorSpriteRenderer.color);
            StartCoroutine(FlashDoorSprite());
        }
        else
        {
            Debug.LogWarning("❌ [FEEDBACK] doorSpriteRenderer is NULL!");
        }

        // 2. Частицы в позиции двери
        if (doorOpenParticles != null && doorAnimator != null)
        {
            Debug.Log("💥 [FEEDBACK] Spawning particles at: " + (doorAnimator.transform.position + Vector3.up * 1f));
            Instantiate(doorOpenParticles, doorAnimator.transform.position + Vector3.up * 1f, Quaternion.identity);
        }
        else
        {
            if (doorOpenParticles == null) Debug.LogWarning("❌ [FEEDBACK] doorOpenParticles is NULL!");
            if (doorAnimator == null) Debug.LogWarning("❌ [FEEDBACK] doorAnimator is NULL!");
        }
    }
    
    // <-- Корутина для вспышки спрайта
    private IEnumerator FlashDoorSprite()
    {
        Color originalColor = doorSpriteRenderer.color;
        Debug.Log($"[FLASH] Start: {originalColor} → Target: {flashColor}");
        
        float elapsed = 0f;

        // Быстрое появление вспышки
        while (elapsed < flashDuration / 2f)
        {
            doorSpriteRenderer.color = Color.Lerp(originalColor, flashColor, elapsed / (flashDuration / 2f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Плавное затухание обратно
        elapsed = 0f;
        while (elapsed < flashDuration / 2f)
        {
            doorSpriteRenderer.color = Color.Lerp(flashColor, originalColor, elapsed / (flashDuration / 2f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        doorSpriteRenderer.color = originalColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = true;
            ShowHint();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = false;
            HideHint();
        }
    }

    // --- НОВЫЙ МЕТОД: Управление блёстками (субтильными!) ---
    
    private void UpdateSparkles()
    {
        if (sparklesEffect != null)
        {
            // Показываем блёстки ВСЕГДА, пока рычаг НЕ активирован (независимо от игрока!)
            bool shouldShow = !leverAnimator.GetBool("isActivated");
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
        if (currentHint != null) return;

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
        if (currentHint == null) return;

        Destroy(currentHint);
        currentHint = null;
    }
}