using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class HealthPotion : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 25;
    private bool isUsed = false; // Чтобы нельзя было юзнуть дважды

    [Header("UI Hint")]
    public GameObject hintPrefab; // Сюда в инспекторе кинешь префаб подсказки
    private GameObject currentHint;
    private bool playerInRange = false;
    
    void Update()
    {
        // Проверяем нажатие кнопки F, если игрок рядом и хилка еще не использована
        if (playerInRange && !isUsed && Keyboard.current.fKey.wasPressedThisFrame)
        {
            UsePotion();
        }
    }
    
    void UsePotion()
    {
        isUsed = true; // Сразу блокируем повторное нажатие

        // Ищем скрипт здоровья на игроке (замени PlayerHealth на имя своего скрипта)
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.Heal(healAmount); // Лечим через метод игрока
        }

        AudioSource audio = GetComponent<AudioSource>();
        float delay = 0.1f;
        if (audio != null && audio.clip != null)
        {
            audio.Play();
            delay = audio.clip.length;
        }
        
        HideHint();
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;

        // Удаляем объект после того как доиграет звук
        Destroy(gameObject, delay + 0.1f);
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