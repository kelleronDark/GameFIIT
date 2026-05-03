using UnityEngine;

public class FireTrap : MonoBehaviour
{
    public int damagePerTick = 10; // Урон за каждый "тик" (можно сделать периодическим)
    public float damageInterval = 0.5f; // Интервал между нанесениями урона (если хотим повторять)

    private bool isPlayerInside = false;
    private float nextDamageTime = 0f;

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
        if (isPlayerInside && Time.time >= nextDamageTime)
        {
            // Проверяем, есть ли у объекта PlayerController
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