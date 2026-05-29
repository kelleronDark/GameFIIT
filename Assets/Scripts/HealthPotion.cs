using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class HealthPotion : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 25;
    private bool isUsed = false;

    [Header("UI Hint")]
    public GameObject hintPrefab;
    private GameObject currentHint;
    private bool playerInRange = false;
    
    void Update()
    {
        if (isUsed) return;
        if (playerInRange && !isUsed && Keyboard.current.fKey.wasPressedThisFrame)
        {
            UsePotion();
        }
    }
    
    public void UsePotion()
    {
        if (isUsed) return;
        
        isUsed = true;
        playerInRange = false;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.Heal(healAmount);
        }
        
        Collider2D potionCollider = GetComponent<Collider2D>();
        if (potionCollider != null) 
        {
            potionCollider.enabled = false;
        }
        
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        
        HideHint();

        AudioSource audio = GetComponent<AudioSource>();
        float delay = 0.1f;
        if (audio != null && audio.clip != null)
        {
            audio.Play();
            delay = audio.clip.length;
        }

        Destroy(gameObject, delay + 0.1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isUsed) return;
        
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
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
    }
}