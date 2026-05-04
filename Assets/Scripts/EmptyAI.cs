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
    public float playerDistError = 1.1f;
    public LayerMask obstacleMask; // Слой стен (Obstacles)
    public Animator anim; // Ссылка на аниматор

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

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        // Получаем вектор скорости монстра
        Vector2 velocity = ai.velocity;
        float speed = velocity.magnitude;

        // Если монстр движется (скорость выше порога)
        if (speed > 0.1f)
        {
            // Передаем нормализованное направление в Blend Tree (значения от -1 до 1)
            Vector2 dir = velocity.normalized;
            anim.SetFloat("MoveX", dir.x);
            anim.SetFloat("MoveY", dir.y);
            anim.SetBool("isMoving", true);

            // Если ты используешь только 2 анимации (лево/право), оставь Flip.
            // Если в Blend Tree настроены 4 стороны (вверх/вниз), Flip можно закомментировать.
            //if (dir.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
            //else if (dir.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // Если монстр стоит на месте
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

        Debug.Log(distToLastPlayerPos);

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
}