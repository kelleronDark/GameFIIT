using UnityEngine;
using Pathfinding;
using System.Collections; // Обязательно для корутин!

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search, Stun }
    [Header("AI State")]
    public State currentState = State.Patrol;   

    [Header("References")]
    public Transform player;
    public Transform[] waypoints;
    public Animator anim;

    [Header("Attack Settings")]
    public int damageAmount = 20;
    public float attackCooldown = 1.5f; // Задержка между ударами
    private float lastAttackTime;

    [Header("Detection Settings")]
    public float chaseDistance = 5f;
    public float stopChaseDistance = 8f;
    public float playerDistError = 1.1f;
    public LayerMask obstacleMask;

    [Header("VFX References")]
    public GameObject stunEffectObject; // Сюда перетаскиваем объект StunEffects из иерархии

    private IAstarAI ai;
    private int currentWaypointIndex = 0;
    private float searchTimer;
    private Vector2 lastPlayerPosition;

    void Start()
    {
        ai = GetComponent<IAstarAI>();

        // На всякий случай подстрахуемся: если забыли перетащить аниматор руками, попробуем найти его сами
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        // При старте игры визуальный эффект оглушения должен быть гарантированно выключен
        if (stunEffectObject != null)
        {
            stunEffectObject.SetActive(false);
        }
    }

    void Update()
    {
        // Если монстр оглушен, мы полностью пропускаем всю логику преследования и поиска
        if (currentState == State.Stun)
        {
            UpdateAnimation();
            return;
        }

        switch (currentState)
        {
            case State.Patrol:
                if (waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
                {
                    ai.destination = waypoints[currentWaypointIndex].position;
                    if (ai.reachedDestination)
                        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                }

                CheckForPlayer();
                break;

            case State.Chase:
                if (player != null)
                {
                    ai.destination = player.position;
                    ChaseLogic();
                }
                break;

            case State.Search:
                ai.destination = lastPlayerPosition;
                SearchLogic();
                CheckForPlayer();
                break;
        }

        UpdateAnimation();
    }

    // Метод оглушения, который вызывается при попадании коробки
    public IEnumerator BecomeStunned(float duration)
    {
        // МУДРОЕ РЕШЕНИЕ: Если монстр УЖЕ находится в состоянии оглушения, 
        // мы просто мгновенно выходим из метода и игнорируем новую коробку!
        if (currentState == State.Stun)
        {
            yield break; // Прерывает выполнение корутины прямо здесь
        }

        State previousState = currentState; // Запоминаем текущее состояние
        currentState = State.Stun;
        ai.isStopped = true; // Принудительно останавливаем движение плагина A*

        // Включаем визуальные эффекты оглушения
        if (anim != null) anim.SetBool("IsStunned", true);
        if (stunEffectObject != null) stunEffectObject.SetActive(true);

        Debug.Log("Монстр оглушен!");

        yield return new WaitForSeconds(duration);

        // Отключаем визуальные эффекты оглушения
        if (anim != null) anim.SetBool("IsStunned", false);
        if (stunEffectObject != null) stunEffectObject.SetActive(false);

        ai.isStopped = false; // Разрешаем плагину А* снова ходить
        currentState = State.Patrol; // Безопасно возвращаем в патруль
        Debug.Log("Монстр пришел в себя");
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        Vector2 velocity = ai.velocity;
        float speed = velocity.magnitude;

        // Если монстр движется и НЕ оглушен
        if (speed > 0.1f && currentState != State.Stun)
        {
            Vector2 dir = velocity.normalized;
            anim.SetFloat("MoveX", dir.x);
            anim.SetFloat("MoveY", dir.y);
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    void ChaseLogic()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > stopChaseDistance)
        {
            lastPlayerPosition = player.position;
            searchTimer = 3f;
            currentState = State.Search;
        }
    }

    void CheckForPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < chaseDistance)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // Настройка фильтра: игнорируем любые триггеры (зоны видимости, свет и т.д.)
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            filter.SetLayerMask(obstacleMask); // Луч будет реагировать ТОЛЬКО на стены из obstacleMask

            // Создаем массив для результата (нам нужен только 1 хит — самое первое препятствие)
            RaycastHit2D[] results = new RaycastHit2D[1];

            // Пускаем луч от монстра к игроку, который проверяет ТОЛЬКО стены
            int hitCount = Physics2D.Raycast(transform.position, directionToPlayer, filter, results, distanceToPlayer);

            // ЕСЛИ на пути луча до игрока НЕ встретилось ни одной стены (hitCount == 0)
            if (hitCount == 0)
            {
                // Значит, между монстром и игроком чистый воздух! Монстр видит игрока.
                currentState = State.Chase;
            }
            else
            {
                // Если луч во что-то попал, значит между ними стена. Монстр не видит игрока.
                // Для теста можно вывести в консоль, что именно перекрыло обзор:
                // Debug.Log($"Игрок скрыт за объектом: {results[0].collider.name}");
            }
        }
    }

    void SearchLogic()
    {
        float distToLastPlayerPos = Vector2.Distance(transform.position, lastPlayerPosition);
        if (distToLastPlayerPos < playerDistError)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                currentState = State.Patrol;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Если монстр в отключке, он не может атаковать
        if (currentState == State.Stun) return;

        // Проверяем Кулдаун атаки (общий для игрока и для коробок)
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 1. ЛОГИКА АТАКИ ИГРОКА
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damageAmount);
                    lastAttackTime = Time.time;

                    if (anim != null) anim.SetTrigger("Attack");
                }
            }
            // 2. НОВАЯ ЛОГИКА АТАКИ КОРОБКИ
            else if (collision.gameObject.CompareTag("Item"))
            {
                BoxImpact box = collision.gameObject.GetComponent<BoxImpact>();
                if (box != null)
                {
                    // Монстр наносит коробке 1 единицу урона (снимает 1 ХП ящика)
                    box.TakeDamage(1);
                    lastAttackTime = Time.time;

                    // Запускаем ту же самую анимацию удара мечом/кулаком!
                    if (anim != null) anim.SetTrigger("Attack");
                }
            }
        }
    }
}