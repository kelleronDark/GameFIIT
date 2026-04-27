using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damagePerTick = 10;
    public float damageInterval = 0.5f;

    private bool isPlayerInside = false;
    private float nextDamageTime = 0f;

    void Update()
    {
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

    void ApplyDamage(GameObject target)
    {
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damagePerTick);
            Debug.Log($"Ловушка с колючками нанесла {damagePerTick} урона игроку.");
        }
    }
}