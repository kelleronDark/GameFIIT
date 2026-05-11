using UnityEngine;
using System.Collections;

public class BoxImpact : MonoBehaviour
{
    private bool canStun = false;

    public void ActivateImpact()
    {
        canStun = true;
        // Вместо простого ожидания, запустим проверку зоны на 0.5 секунды
        StartCoroutine(CheckAreaForTime(0.5f));
    }

    IEnumerator CheckAreaForTime(float time)
    {
        float timer = 0;
        while (timer < time && canStun)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius);
            if (hit != null && hit.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    StartCoroutine(enemy.BecomeStunned(3f));
                    canStun = false;
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        canStun = false;
    }

    private void Deactivate() => canStun = false;

    public float explosionRadius = 1.5f; // Настрой в инспекторе (чуть больше коробки)

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canStun) return;

        // Создаем круглую зону поражения в точке удара
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var obj in hitEnemies)
        {
            if (obj.CompareTag("Enemy"))
            {
                EnemyAI enemy = obj.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    StartCoroutine(enemy.BecomeStunned(3f));
                    canStun = false; // Чтобы не оглушить дважды одним броском
                    Debug.Log("Монстр задет радиусом коробки!");
                    break;
                }
            }
        }
    }

    // Визуализация радиуса в редакторе (для настройки)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}