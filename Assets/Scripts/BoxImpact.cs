using UnityEngine;
using System.Collections;
using Pathfinding;

public class BoxImpact : MonoBehaviour
{
    private bool canStun = false;
    public float explosionRadius = 1.5f; // Радиус оглушения

    private void Start()
    {
        // Когда сцена запускается, сразу обновляем сетку под коробкой, 
        // чтобы монстр обходил коробки, расставленные на уровне изначально
        UpdateAstarGraph();
    }

    public void ActivateImpact()
    {
        canStun = true;
        // Запуск таймера проверки области на 0.5 секунды
        StartCoroutine(CheckAreaForTime(0.5f));

        // Коробка приземлилась — принудительно обновляем сетку путей вокруг неё
        UpdateAstarGraph();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canStun) return;

        // Фиксация касания врага при прямом попадании
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var obj in hitEnemies)
        {
            if (obj.CompareTag("Enemy"))
            {
                EnemyAI enemy = obj.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    StartCoroutine(enemy.BecomeStunned(3f));
                    canStun = false; // Чтобы не глушить несколько врагов одной коробкой
                    Debug.Log("Монстр получил коробкой по голове!");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Вспомогательный метод для динамического обновления сетки A* вокруг коробки
    /// </summary>
    private void UpdateAstarGraph()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && AstarPath.active != null)
        {
            // Берем границы нашего коллайдера и просим А* пересчитать эту область
            AstarPath.active.UpdateGraphs(col.bounds);
        }
    }

    // Визуализация радиуса в редакторе (для удобства)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}