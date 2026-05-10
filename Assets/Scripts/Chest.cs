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
    public AudioSource audioSource; // 1. Источник звука

    private GameObject currentHint;
    private bool playerInRange = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Если не назначили вручную, ищем на объекте
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
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

        // 2. Воспроизводим звук
        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (animator != null)
            animator.SetBool("IsOpen", true);

        if (containsKey && KeyInventory.Instance != null)
        {
            bool added = KeyInventory.Instance.AddKey();
            if (added) Debug.Log("Ключ добавлен в инвентарь!");
            else Debug.LogWarning("Инвентарь полон! Ключ не подобран.");
        }

        HideHint();
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
            {
                canvas.worldCamera = mainCamera;
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