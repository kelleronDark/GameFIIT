using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search }
    public State currentState = State.Patrol;

    public Transform player;
    public Transform[] waypoints;
    public float speed = 2f;
    public float chaseDistance = 5f;
    public float stopChaseDistance = 8f;

    [Header("Obstacle Avoidance")]
    public float detectionDistance = 1.5f; // Дистанция обнаружения стены
    public LayerMask obstacleLayer;        // Слой, на котором находятся стены

    private int currentWaypointIndex = 0;
    private float searchTimer;
    private Vector2 lastPlayerPosition;

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                MoveWithAvoidance(waypoints[currentWaypointIndex].position);
                if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.5f)
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                CheckForPlayer();
                break;

            case State.Chase:
                ChaseLogic();
                break;

            case State.Search:
                MoveWithAvoidance(lastPlayerPosition);
                SearchLogic();
                CheckForPlayer();
                break;
        }
    }

    // ГЛАВНЫЙ МЕТОД: Движение с обходом стен
    void MoveWithAvoidance(Vector2 target)
    {
        Vector2 targetDir = (target - (Vector2)transform.position).normalized;
        Vector2 chosenDir = targetDir;

        // 1. Проверяем, свободен ли путь напрямую к цели
        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDir, detectionDistance, obstacleLayer);

        if (hit.collider != null)
        {
            bool foundPath = false;

            // 2. Если путь прегражден, ищем ближайшее свободное направление
            // Проверяем углы от 20 до 90 градусов в обе стороны
            for (float angle = 20; angle <= 90; angle += 15)
            {
                // Проверка влево
                Vector2 leftDir = Quaternion.Euler(0, 0, angle) * targetDir;
                if (Physics2D.Raycast(transform.position, leftDir, detectionDistance, obstacleLayer).collider == null)
                {
                    chosenDir = leftDir;
                    foundPath = true;
                    break;
                }

                // Проверка вправо
                Vector2 rightDir = Quaternion.Euler(0, 0, -angle) * targetDir;
                if (Physics2D.Raycast(transform.position, rightDir, detectionDistance, obstacleLayer).collider == null)
                {
                    chosenDir = rightDir;
                    foundPath = true;
                    break;
                }
            }

            // 3. Если кругом тупик, медленно пятимся назад
            if (!foundPath) chosenDir = -targetDir * 0.5f;
        }

        // Двигаемся в выбранном направлении
        transform.Translate(chosenDir * speed * Time.deltaTime);
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
        else
        {
            MoveWithAvoidance(player.position);
        }
    }

    void CheckForPlayer()
    {
        if (Vector2.Distance(transform.position, player.position) < chaseDistance)
            currentState = State.Chase;
    }

    void SearchLogic()
    {
        if (Vector2.Distance(transform.position, lastPlayerPosition) < 0.2f)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0) currentState = State.Patrol;
        }
    }

    // Отрисовка лучей в редакторе для тестов
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, (Vector2)transform.right * detectionDistance);
    }
}