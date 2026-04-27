using UnityEngine;

public class AlwaysActiveSpikeTrap : MonoBehaviour
{
    [Header("Settings")]
    public int damagePerTick = 10;      // Урон за один "тик"
    public float damageInterval = 0.5f; // Интервал между нанесениями урона (секунды)

    private bool isPlayerInside = false;
    private float nextDamageTime = 0f;

    void Start()
    {
        // Опционально: можно добавить проверку, что коллайдер есть и является триггером
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        if (trigger == null || !trigger.isTrigger)
        {
            Debug.LogWarning($"У объекта {gameObject.name} нет BoxCollider2D или он не является триггером!");
        }
    }

    void Update()
    {
        // Наносим урон периодически, пока игрок внутри зоны
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            nextDamageTime = Time.time + damageInterval; // Первый урон сразу
            ApplyDamage(other.gameObject);               // И сразу наносим первый тик
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    void ApplyDamage(GameObject target)
    {
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damagePerTick);
            // Можно убрать лог в релизе, но для отладки полезно
            // Debug.Log($"Активная ловушка нанесла {damagePerTick} урона игроку.");
        }
    }
}