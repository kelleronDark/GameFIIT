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