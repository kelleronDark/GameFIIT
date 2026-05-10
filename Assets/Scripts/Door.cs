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
    public AudioSource audioSource; // 1. Ссылка на источник звука

    private GameObject currentHint;
    private bool playerInRange = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Если AudioSource не назначен вручную, пробуем взять компонент с этого же объекта
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

        // 2. Воспроизводим звук, если источник есть
        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (animator != null)
            animator.SetBool("IsOpen", true);

        Collider2D doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
            doorCollider.enabled = false;

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