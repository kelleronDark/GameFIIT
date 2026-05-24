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
    
    [Header("Sparkles (Lever)")] 
    public GameObject sparklesEffect; // Префаб блёсток рычага
    
    [Header("Door Feedback")]
    public GameObject doorOpenParticles;      // Префаб блёсток/частиц двери

    private GameObject leverSparklesInstance; // Храним инстанс отдельно, не ломая префаб
    private GameObject currentHint;
    private bool isPlayerNearby = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    
        // 1. Создаем блёстки для РЫЧАГА
        if (sparklesEffect != null)
        {
            leverSparklesInstance = Instantiate(sparklesEffect, transform.position + Vector3.up * 0.8f, Quaternion.identity);
            leverSparklesInstance.transform.SetParent(transform);
            leverSparklesInstance.transform.localPosition = Vector3.up * 0.8f;
        }
    
        // 2. Определяем начальное состояние
        bool targetState = startsOpened; 

        if (bayonetTrap != null)
        {
            bool isAlreadyDeactivated = SaveManager.Instance != null && SaveManager.Instance.IsBayonetTrapDeactivated();
            if (isAlreadyDeactivated)
            {
                targetState = true; 
            }
        }

        // 3. Применяем состояние
        ApplyState(targetState);
    
        // 4. Обновляем видимость блесток рычага
        UpdateSparkles(); 
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

        bool newState = false;

        if (doorAnimator != null)
        {
            newState = !doorAnimator.GetBool("isOpen");
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
            newState = !bayonetTrap.IsActive; 

            if (leverAnimator != null)
                leverAnimator.SetBool("isActivated", newState);
            
            if (SaveManager.Instance != null)
                SaveManager.Instance.SetBayonetTrapState(newState);

            Debug.Log("Ловушка переключена.");
        }

        // Вызываем визуальный отклик двери (только блёстки), если она ОТКРЫЛАСЬ
        if (newState) 
        {
            PlayDoorOpenFeedback();
        }

        HideHint();
        UpdateSparkles(); 
    }
    
    private void PlayDoorOpenFeedback()
    {
        Debug.Log("🚪 [FEEDBACK] Spawning door sparkles!");

        // Спавн частиц блёсток около двери
        if (doorOpenParticles != null)
        {
            // Позиция спавна: если есть doorAnimator, берем его позицию, иначе позицию самого рычага
            Vector3 spawnPosition = (doorAnimator != null) ? doorAnimator.transform.position : transform.position;
            spawnPosition += Vector3.up * 1f; // Смещение чуть выше центра двери

            GameObject particlesInstance = Instantiate(doorOpenParticles, spawnPosition, Quaternion.identity);
            
            // Запускаем систему частиц, если она не стартует сама
            ParticleSystem ps = particlesInstance.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                // Автоматически удаляем объект из сцены, как только частицы догорят
                Destroy(particlesInstance, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                // Резервный таймер удаления для обычных объектов
                Destroy(particlesInstance, 2f); 
            }
        }
        else
        {
            Debug.LogWarning("❌ [FEEDBACK] doorOpenParticles (префаб блёсток двери) не задан в инспекторе!");
        }
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
    
    private void UpdateSparkles()
    {
        // ВАЖНО: проверяем leverSparklesInstance (созданную копию), а не префаб!
        if (leverSparklesInstance != null && leverAnimator != null)
        {
            bool shouldShow = !leverAnimator.GetBool("isActivated");
            leverSparklesInstance.SetActive(shouldShow);
    
            var particle = leverSparklesInstance.GetComponentInChildren<ParticleSystem>();
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
