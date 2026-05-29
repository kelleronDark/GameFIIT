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
    public GameObject sparklesEffect;

    private GameObject currentHint;
    public bool IsPlayerInRange { get; private set; } = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    
        if (sparklesEffect != null)
        {
            GameObject instance = Instantiate(sparklesEffect, transform.position + Vector3.up, Quaternion.identity);
            instance.transform.SetParent(transform);
            sparklesEffect = instance;
        }
    
        UpdateSparkles();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInRange = true;
            ShowHint();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInRange = false;
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

        HideHint();
        UpdateSparkles();
    }
    
    private void UpdateSparkles()
    {
        if (sparklesEffect != null)
        {
            sparklesEffect.SetActive(!isOpened);
            
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