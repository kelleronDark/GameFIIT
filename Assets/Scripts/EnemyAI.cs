using UnityEngine;
using Pathfinding;
using System.Collections; // Обязательно для корутин!

public class EnemyAI : MonoBehaviour
{
    // Добавили состояние Stun
    public enum State { Patrol, Chase, Search, Stun }
    public State currentState = State.Patrol;

    public Transform player;
    public Transform[] waypoints;

    [Header("Attack Settings")]
    public int damageAmount = 20;
    public float attackCooldown = 1.5f; // Задержка между ударами
    private float lastAttackTime;

    [Header("Detection Settings")]
    public float chaseDistance = 5f;
    public float stopChaseDistance = 8f;
    public float playerDistError = 1.1f;
    public LayerMask obstacleMask;
    public Animator anim;

    private IAstarAI ai;
    private int currentWaypointIndex = 0;
    private float searchTimer;
    private Vector2 lastPlayerPosition;

    void Start()
    {
        ai = GetComponent<IAstarAI>();
    }

    void Update()
    {
        // Если монстр оглушен, мы пропускаем всю логику преследования и поиска
        if (currentState == State.Stun)
        {
            UpdateAnimation(); // Чтобы анимация переключилась в Idle
            return;
        }

        switch (currentState)
        {
            case State.Patrol:
                ai.destination = waypoints[currentWaypointIndex].position;
                if (ai.reachedDestination)
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

                CheckForPlayer();
                break;

            case State.Chase:
                ai.destination = player.position;
                ChaseLogic();
                break;

            case State.Search:
                ai.destination = lastPlayerPosition;
                SearchLogic();
                CheckForPlayer();
                break;
        }

        UpdateAnimation();
    }

    // Метод, который вызывает коробка
    public IEnumerator BecomeStunned(float duration)
    {
        State previousState = currentState; // Запоминаем, что он делал
        currentState = State.Stun;
        ai.isStopped = true; // Останавливаем движение плагина A*

        Debug.Log("Монстр оглушен!");

        yield return new WaitForSeconds(duration);

        ai.isStopped = false; // Разрешаем ходить
        currentState = State.Patrol; // Возвращаем в патруль (или previousState)
        Debug.Log("Монстр пришел в себя");
    }

    void UpdateAnimation()
    {
        Vector2 velocity = ai.velocity;
        float speed = velocity.magnitude;

        // Если монстр оглушен или просто стоит
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

    // ... (остальные методы ChaseLogic, CheckForPlayer, SearchLogic остаются без изменений)
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
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < chaseDistance)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask);
            if (hit.collider == null) currentState = State.Chase;
        }
    }

    void SearchLogic()
    {
        float distToLastPlayerPos = Vector2.Distance(transform.position, lastPlayerPosition);
        if (distToLastPlayerPos < playerDistError)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0) currentState = State.Patrol;
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
        // Проверяем, что коснулись игрока
        if (collision.gameObject.CompareTag("Player"))
        {
            // Проверяем, прошло ли достаточно времени с прошлой атаки
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damageAmount);
                    lastAttackTime = Time.time;

                    // Здесь можно запустить анимацию атаки, если она есть
                    // anim.SetTrigger("Attack"); 
                }
            }
        }
    }
}