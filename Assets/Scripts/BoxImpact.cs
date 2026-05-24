using UnityEngine;
using System.Collections;
using Pathfinding;

public class BoxImpact : MonoBehaviour
{
    [Header("Destruction Settings")]
    public int boxHealth = 3; // Сколько ударов выдерживает коробка
    [Header("Glow Settings")]
    public bool enableGlow = true;              // Вкл/выкл свечение
    public Color glowColor = new Color(1f, 0.9f, 0.6f, 1f); // Тёплый золотой
    public float pulseSpeed = 3f;               // Скорость пульсации
    public float pulseIntensity = 0.15f;        // Сила эффекта (0.1–0.2 — незаметно)
    public float throwCooldown = 3f;            // Секунд без свечения после броска

    private bool canStun = false;
    public float explosionRadius = 1.5f;
    
    // --- Флаги для свечения ---
    private bool isRecentlyThrown = false;      // true = только что бросили, ждём
    private SpriteRenderer sr;
    private Collider2D col;
    private Color originalColor;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            Debug.LogError($"❌ BoxImpact: SpriteRenderer НЕ найден на {gameObject.name}!");
        else
            Debug.Log($"✅ BoxImpact: SpriteRenderer найден. Original color: {sr.color}");
    
        if (col == null)
            Debug.LogError($"❌ BoxImpact: Collider2D НЕ найден на {gameObject.name}!");
        if (sr != null)
            originalColor = sr.color;
        
        UpdateAstarGraph();
    }

    public void ActivateImpact()
    {
        canStun = true;
        StartCoroutine(CheckAreaForTime(0.5f));
        UpdateAstarGraph();
        
        // <-- НОВОЕ: коробка только что приземлилась (или брошена)
        // Ставим флаг "не светить" на время cooldown
        StartThrowCooldown();
    }

    // Запускаем таймер "без свечения" после броска
    private void StartThrowCooldown()
    {
        if (enableGlow)
        {
            isRecentlyThrown = true;
            StartCoroutine(ResetThrowCooldown());
        }
    }

    private IEnumerator ResetThrowCooldown()
    {
        yield return new WaitForSeconds(throwCooldown);
        isRecentlyThrown = false;
    }

    void Update()
    {
        // Обновляем свечение каждый кадр (если включено)
        if (enableGlow && sr != null)
        {
            UpdateGlow();
        }
    }

    // Логика свечения на основе флагов
    private void UpdateGlow()
    {
        // Условия для свечения:
        // 1. Коллайдер включен (коробка НЕ в руках)
        // 2. Не недавно брошена (прошло 3 сек после ActivateImpact)
        bool shouldGlow = col != null && col.enabled && !isRecentlyThrown;

        if (shouldGlow)
        {
            // Пульсация цвета
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            sr.color = Color.Lerp(originalColor, glowColor, pulse * pulseIntensity);
        }
        else
        {
            // Возвращаем оригинальный цвет
            sr.color = originalColor;
        }
    }

    // --- Остальной код без изменений ---

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canStun) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var obj in hitEnemies)
        {
            if (obj.CompareTag("Enemy"))
            {
                EnemyAI enemy = obj.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    StartCoroutine(enemy.BecomeStunned(3f));
                    canStun = false;
                    Debug.Log("Монстр получил коробкой по голове!");
                    break;
                }
            }
        }
    }

    private void UpdateAstarGraph()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null && AstarPath.active != null)
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