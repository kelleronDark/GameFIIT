using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; 
using TMPro;
using Pathfinding;

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
    public GameObject sparklesEffect;
    
    [Header("Door Feedback")]
    public GameObject doorOpenParticles;

    private GameObject leverSparklesInstance;
    private GameObject currentHint;
    private bool isPlayerNearby = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    
        if (sparklesEffect != null)
        {
            leverSparklesInstance = Instantiate(sparklesEffect, transform.position + Vector3.up * 0.8f, Quaternion.identity);
            leverSparklesInstance.transform.SetParent(transform);
            leverSparklesInstance.transform.localPosition = Vector3.up * 0.8f;
        }
    
        bool targetState = startsOpened; 

        if (bayonetTrap != null)
        {
            bool isAlreadyDeactivated = SaveManager.Instance != null && SaveManager.Instance.IsBayonetTrapDeactivated();
            if (isAlreadyDeactivated)
            {
                targetState = true; 
            }
        }

        ApplyState(targetState);
    
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
        
        UpdateAstarGraph();
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
            
            UpdateAstarGraph();

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

        if (newState) 
        {
            PlayDoorOpenFeedback();
        }

        HideHint();
        UpdateSparkles(); 
    }
    
    private void PlayDoorOpenFeedback()
    {
        Debug.Log("Spawning door sparkles!");

        if (doorOpenParticles != null)
        {
            Vector3 spawnPosition = (doorAnimator != null) ? doorAnimator.transform.position : transform.position;
            spawnPosition += Vector3.up * 1f;

            GameObject particlesInstance = Instantiate(doorOpenParticles, spawnPosition, Quaternion.identity);
            
            ParticleSystem ps = particlesInstance.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(particlesInstance, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(particlesInstance, 2f); 
            }
        }
        else
        {
            Debug.LogWarning("doorOpenParticles (префаб блёсток двери) не задан в инспекторе!");
        }
    }
    
    private void UpdateAstarGraph()
    {
        if (doorCollider != null && AstarPath.active != null)
        {
            Bounds customBounds = new Bounds(doorCollider.bounds.center, new Vector3(2.5f, 2.5f, 2.5f));
            AstarPath.active.UpdateGraphs(customBounds);
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
