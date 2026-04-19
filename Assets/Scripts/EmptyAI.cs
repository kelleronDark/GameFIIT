using UnityEngine;
using UnityEngine.AI; // Обязательно добавляем для работы с навигацией

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search }
    public State currentState = State.Patrol;

    public Transform player;
    public Transform[] waypoints;
    public float speed = 2f;
    public float chaseDistance = 5f;

    private int currentWaypointIndex = 0;

    public float stopChaseDistance = 8f;
    public float searchTime = 3f;
    private float searchTimer;
    private Vector2 lastPlayerPosition;

    private NavMeshAgent agent; // Ссылка на компонент навигации

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Настройки для 2D, чтобы монстр не вращался в 3D и не падал
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Устанавливаем базовую скорость
        agent.speed = speed;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                CheckForPlayer();
                break;
            case State.Chase:
                ChaseLogic();
                break;
            case State.Search:
                SearchLogic();
                CheckForPlayer();
                break;
        }
    }

    void PatrolLogic()
    {
        if (waypoints.Length == 0) return;

        agent.speed = speed;
        Transform target = waypoints[currentWaypointIndex];

        // Говорим агенту идти к точке. Он сам найдет путь в обход стен!
        agent.SetDestination(target.position);

        // Проверяем, дошел ли агент до точки (используем встроенную проверку расстояния)
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void ChaseLogic()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopChaseDistance)
        {
            lastPlayerPosition = player.position;
            searchTimer = searchTime;
            currentState = State.Search;
        }
        else
        {
            // Преследуем игрока с увеличенной скоростью
            agent.speed = speed * 1.5f;
            agent.SetDestination(player.position);
        }
    }

    void SearchLogic()
    {
        agent.speed = speed;
        agent.SetDestination(lastPlayerPosition);

        // Когда дошли до места, где видели игрока последний раз
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0)
            {
                currentState = State.Patrol;
            }
        }
    }

    void CheckForPlayer()
    {
        if (Vector2.Distance(transform.position, player.position) < chaseDistance)
        {
            currentState = State.Chase;
        }
    }
}