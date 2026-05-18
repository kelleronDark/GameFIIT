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
        State previousState = currentState; // Запоминаем текущее состояние
        currentState = State.Stun;
        ai.isStopped = true; // Принудительно останавливаем движение плагина A*

        // Включаем визуальные эффекты оглушения
        if (anim != null)
        {
            anim.SetBool("IsStunned", true);
        }

        if (stunEffectObject != null)
        {
            stunEffectObject.SetActive(true); // Зажигаем звездочки/искры над головой
        }

        Debug.Log("Монстр оглушен!");

        yield return new WaitForSeconds(duration);

        // Отключаем визуальные эффекты оглушения
        if (anim != null)
        {
            anim.SetBool("IsStunned", false);
        }

        if (stunEffectObject != null)
        {
            stunEffectObject.SetActive(false); // Тушим звездочки/искры
        }

        ai.isStopped = false; // Разрешаем плагину А* снова ходить
        currentState = previousState; // Возвращаем монстра к тому, чем он занимался до удара (например, Chase или Patrol)
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
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask);

            if (hit.collider == null)
            {
                currentState = State.Chase;
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
        // Если монстр в отключке, он не может атаковать игрока
        if (currentState == State.Stun) return;

        // Проверяем, что коснулись игрока
        if (collision.gameObject.CompareTag("Player"))
        {
            // Проверяем Кулдаун атаки
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damageAmount);
                    lastAttackTime = Time.time;

                    // === ВОТ ЭТУ СТРОКУ МЫ АКТИВИРОВАЛИ: ===
                    if (anim != null)
                    {
                        anim.SetTrigger("Attack");
                    }
                }
            }
        }
    }
}