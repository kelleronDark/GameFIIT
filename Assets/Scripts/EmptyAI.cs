using UnityEngine;
using Pathfinding; // ОБЯЗАТЕЛЬНО добавь это

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search }
    public State currentState = State.Patrol;

    public Transform player;
    public Transform[] waypoints;

    // Ссылка на компоненты плагина
    private IAstarAI ai;

    private int currentWaypointIndex = 0;
    private float searchTimer;
    private Vector2 lastPlayerPosition;

    public float chaseDistance = 5f;
    public float stopChaseDistance = 8f;

    void Start()
    {
        // Получаем доступ к контроллеру плагина
        ai = GetComponent<IAstarAI>();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                // Просто говорим плагину: "Иди к этой точке"
                ai.destination = waypoints[currentWaypointIndex].position;

                // Проверяем, дошли ли (у плагина есть встроенная проверка)
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
        if (Vector2.Distance(transform.position, player.position) < chaseDistance)
            currentState = State.Chase;
    }

    void SearchLogic()
    {
        if (ai.reachedDestination)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0) currentState = State.Patrol;
        }
    }
}