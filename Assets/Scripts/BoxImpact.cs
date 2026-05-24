using UnityEngine;
using System.Collections;
using Pathfinding;

public class BoxImpact : MonoBehaviour
{
    [Header("Destruction Settings")]
    public int boxHealth = 3; // Сколько ударов выдерживает коробка

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
            // Создаем объект обновления графа в границах коробки
            GraphUpdateObject guo = new GraphUpdateObject(col.bounds);

            // ЖЕЛЕЗНО ПРИКАЗЫВАЕМ А*: эта область ДОЛЖНА быть проходимой!
            guo.modifyWalkability = true;
            guo.setWalkability = true;

            // Присваиваем узлам под коробкой штрафной тег 1
            guo.modifyTag = true;
            guo.setTag = 1;

            // Обновляем сетку
            AstarPath.active.UpdateGraphs(guo);
        }
    }   

    // Визуализация радиуса в редакторе (для удобства)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    /// <summary>
    /// Метод получения урона коробкой от монстра
    /// </summary>
    public void TakeDamage(int damage)
    {
        boxHealth -= damage;

        if (boxHealth <= 0)
        {
            DestroyBox();
        }
    }

    private void DestroyBox()
    {
        Debug.Log($"📦 Коробка {gameObject.name} уничтожена!");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && AstarPath.active != null)
        {
            col.enabled = false; // Отключаем физику, чтобы монстр сразу прошел вперед

            // Возвращаем узлы сетки в обычное состояние
            GraphUpdateObject guo = new GraphUpdateObject(col.bounds);
            guo.modifyWalkability = true;
            guo.setWalkability = true;
            guo.modifyTag = true;
            guo.setTag = 0; // Возвращаем тег Basic (0)

            AstarPath.active.UpdateGraphs(guo);
        }

        Destroy(gameObject);
    }
}