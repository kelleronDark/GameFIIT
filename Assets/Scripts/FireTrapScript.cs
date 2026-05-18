using UnityEngine;

public class FireTrap : MonoBehaviour
{
    public int damagePerTick = 10;
    public float damageInterval = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource; // 1. Ссылка на звук

    private bool isPlayerInside = false;
    private float nextDamageTime = 0f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // 2. Запускаем звук сразу при старте (ловушка всегда "гудит")
        if (audioSource != null)
        {
            audioSource.Play(); 
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            nextDamageTime = Time.time + damageInterval;
            ApplyDamage(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    void Update()
    {
        // Логика урона БЕЗ ИЗМЕНЕНИЙ (работает только внутри маленького триггера)
        if (isPlayerInside && Time.time >= nextDamageTime)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                ApplyDamage(player.gameObject);
            }
            nextDamageTime = Time.time + damageInterval;
        }
    }

    void ApplyDamage(GameObject target)
    {
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damagePerTick);
            Debug.Log($"Ловушка нанесла {damagePerTick} урона игроку.");
        }
    }
}