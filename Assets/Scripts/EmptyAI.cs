using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search }
    public State currentState = State.Patrol;

    public Transform player;
    public Transform[] waypoints;

    [Header("Detection Settings")]
    public float chaseDistance = 5f;
    public float stopChaseDistance = 8f;
    public LayerMask obstacleMask; // Слой стен (Obstacles)

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

        // Логика поворота спрайта (Flip)
        if (ai.velocity.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (ai.velocity.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
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

    // ОБНОВЛЕННАЯ ЛОГИКА ПРОВЕРКИ ИГРОКА
    void CheckForPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < chaseDistance)
        {
            // Направление от монстра к игроку
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // Пускаем луч. 
            // Он игнорирует всё, кроме слоев в obstacleMask и слоя Игрока (если нужно)
            // Но проще всего проверять попадание в препятствие на пути
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask);

            // Если луч НИКОГО не встретил на дистанции до игрока (hit.collider == null),
            // значит путь чист и монстр видит игрока
            if (hit.collider == null)
            {
                currentState = State.Chase;
            }
        }
    }

    void SearchLogic()
    {
        if (ai.reachedDestination)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0) currentState = State.Patrol;
        }
    }

    // Отрисовка радиуса в редакторе (для удобства настройки)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }
}