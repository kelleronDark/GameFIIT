using UnityEngine;
using Pathfinding;
using System.Collections;

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
    public int damageAmount = 30;
    public float attackCooldown = 1.5f; // �������� ����� �������
    private float lastAttackTime;

    [Header("Detection Settings")]
    [Tooltip("Дистанция зрения перед собой")]
    public float chaseDistance = 6f; 
    [Tooltip("Дистанция слуха/зрения сзади")]
    public float backChaseDistance = 3.5f;
    [Tooltip("Дистанция, при которой монстр теряет интерес")]
    public float stopChaseDistance = 8f;
    public float playerDistError = 0.4f;
    public LayerMask obstacleMask;
    
    [Header("Layer Settings")]
    public string doorLayerName = "DynamicObs";

    [Header("Search Settings")]
    public float searchWaitDuration = 3f;
    public float predictionDistance = 2.5f;

    [Header("VFX References")]
    public GameObject stunEffectObject; // ���� ������������� ������ StunEffects �� ��������

    private IAstarAI ai;
    private int currentWaypointIndex = 0;
    private float searchTimer;
    private float stuckCheckTimer;
    
    private Vector2 searchTargetPosition;
    private Vector2 lastKnownPlayerPosition;
    
    private Vector2 lastPlayerDirection = Vector2.zero;
    private Vector3 lastPlayerPosTrack;
    
    private Vector2 lastFacingDirection = Vector2.down;

    void Start()
    {
        ai = GetComponent<IAstarAI>();
        if (anim == null) anim = GetComponent<Animator>();
        if (stunEffectObject != null) stunEffectObject.SetActive(false);
    }

    void Update()
    {
        TrackPlayerDirection();
        
        if (currentState == State.Stun)
        {
            UpdateAnimation();
            return;
        }
        
        HandleAIBehavior();
        UpdateAnimation();
    }
    
    void TrackPlayerDirection()
    {
        if (player == null) return;

        Vector2 currentFrameMovement = (player.position - lastPlayerPosTrack);
        if (currentFrameMovement.magnitude > 0.005f)
        {
            lastPlayerDirection = currentFrameMovement.normalized;
        }
        lastPlayerPosTrack = player.position;
    }
    
    void HandleAIBehavior()
    {
        float currentVisionRange = (currentState == State.Chase) ? stopChaseDistance : chaseDistance;
        bool canSeePlayer = EvaluateLineOfSight(currentVisionRange, out RaycastHit2D hit);

        // Если мы в режиме патруля или поиска и увидели игрока — ГАШИМ В ПОГОНЮ
        if (canSeePlayer && player != null)
        {
            lastKnownPlayerPosition = player.position;

            if (currentState == State.Patrol || currentState == State.Search)
            {
                currentState = State.Chase;
            }
        }

        switch (currentState)
        {
            case State.Patrol:
                ai.isStopped = false;
                ExecutePatrolLogic();
                break;

            case State.Chase:
                ai.isStopped = false;
                if (player != null)
                {
                    // В погоне цель — всегда живой игрок
                    ai.destination = player.position;

                    // Если зрение пропало (за углом или из-за двери)
                    if (!canSeePlayer)
                    {
                        Vector2 rawPredictedTarget = lastKnownPlayerPosition + (lastPlayerDirection * predictionDistance);
                        Vector2 finalSearchTarget = lastKnownPlayerPosition;
                        
                        RaycastHit2D hitCheck = Physics2D.Raycast(lastKnownPlayerPosition, lastPlayerDirection, predictionDistance, obstacleMask);
                        
                        if (hitCheck.collider != null)
                        {
                            // Если на пути упреждения стена или закрытая дверь — 
                            // монстр бежит К ЭТОЙ СТЕНЕ/ДВЕРИ, останавливаясь чуть-чуть не доходя (на 0.4 метра)
                            finalSearchTarget = hitCheck.point - (lastPlayerDirection * 0.4f);
                        }
                        
                        else
                        {
                            // Если впереди пусто — проверяем точку через А*
                            if (AstarPath.active != null)
                            {
                                var safeNodeInfo = AstarPath.active.GetNearest(rawPredictedTarget, NNConstraint.Default);
                                if (safeNodeInfo.node != null && safeNodeInfo.node.Walkable)
                                {
                                    finalSearchTarget = (Vector3)safeNodeInfo.position;
                                }
                                else
                                {
                                    finalSearchTarget = lastKnownPlayerPosition;
                                }
                            }
                            else
                            {
                                finalSearchTarget = rawPredictedTarget;
                            }
                        }

                        StartSearchingAt(finalSearchTarget);
                    }
                }
                break;

            case State.Search:
                ExecuteSearchLogic();
                break;
        }
    }
    
    // Честная проверка прямой видимости с возвратом данных о препятствии
    bool EvaluateLineOfSight(float maxDistance, out RaycastHit2D hitResult)
    {
        hitResult = new RaycastHit2D();
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        
        // РАБОТА СО СТЕЛСОМ (Конус зрения + "Слух")
        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        
        // Скалярное произведение векторов взгляда и направления на игрока
        float cosAngle = Vector2.Dot(lastFacingDirection, directionToPlayer);
        
        float allowedDistance = maxDistance;

        // Если cosAngle < 0.35f, значит игрок находится сбоку или за спиной (угол > ~72 градусов от центра взгляда)
        if (cosAngle < 0.35f)
        {
            allowedDistance = backChaseDistance; // Включаем радиус "слуха"
        }
        
        // Если игрок за пределами радиуса (динамического) — не видим его
        if (distance > allowedDistance) return false;

        hitResult = Physics2D.Raycast(transform.position, directionToPlayer, distance, obstacleMask);

        if (hitResult.collider == null)
        {
            Debug.DrawLine(transform.position, player.position, Color.green); // Вижу!
            return true;
        }

        Debug.DrawLine(transform.position, hitResult.point, Color.red); // Вижу только стену/дверь
        return false;
    }
    
    void StartSearchingAt(Vector2 targetPos)
    {
        currentState = State.Search;
        searchTargetPosition = targetPos;
        searchTimer = searchWaitDuration;
        stuckCheckTimer = 0f;
        ai.isStopped = true; 
        ai.destination = searchTargetPosition;
    
        // Даем небольшую задержку перед тем, как он снова пойдет искать
        // Это позволит системе А* обновить граф и понять, что путь перекрыт
        StartCoroutine(ResumeMovementAfterDelay(0.3f));
    }
    
    IEnumerator ResumeMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ai.isStopped = false;
    }
    
    void ExecutePatrolLogic()
    {
        if (waypoints.Length == 0 || waypoints[currentWaypointIndex] == null) return;

        ai.destination = waypoints[currentWaypointIndex].position;
        if (ai.reachedDestination)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
    
    void ExecuteSearchLogic()
    {
        float distanceToSearchPoint = Vector2.Distance(transform.position, searchTargetPosition);
        
        bool reachedTarget = distanceToSearchPoint < playerDistError;

        stuckCheckTimer += Time.deltaTime;
        bool isStuck = (stuckCheckTimer > 0.5f) && (ai.velocity.magnitude < 0.1f);

        if (reachedTarget || isStuck)
        {
            ai.isStopped = true; // Красиво стоим на месте и «ищем»

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                ai.isStopped = false;
                currentState = State.Patrol; // Обыскали, никого нет — пошли патрулировать дальше
            }
        }
        else
        {
            // Пока бежим к углу/двери — тормозить нельзя!
            ai.isStopped = false;
            ai.destination = searchTargetPosition;
        }
    }

    public IEnumerator BecomeStunned(float duration)
    {
        // ������ �������: ���� ������ ��� ��������� � ��������� ���������, 
        // �� ������ ��������� ������� �� ������ � ���������� ����� �������!
        if (currentState == State.Stun)
        {
            yield break; // ��������� ���������� �������� ����� �����
        }

        State previousState = currentState; // ���������� ������� ���������
        currentState = State.Stun;
        ai.isStopped = true;

        if (anim != null)
        {
            anim.SetBool("IsStunned", true);
        }

        if (stunEffectObject != null)
        {
            stunEffectObject.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (anim != null)
        {
            anim.SetBool("IsStunned", false);
        }

        if (stunEffectObject != null)
        {
            stunEffectObject.SetActive(false);
        }

        ai.isStopped = false; // ��������� ������� �* ����� ������
        currentState = previousState; // ���������� ������� � ����, ��� �� ��������� �� ����� (��������, Chase ��� Patrol)
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        Vector2 velocity = ai.velocity;
        float speed = velocity.magnitude;

        if (speed > 0.2f && currentState != State.Stun)
        {
            Vector2 dir = velocity.normalized;
            
            if (Mathf.Abs(dir.x) > 0.3f || Mathf.Abs(dir.y) > 0.3f)
            {
                lastFacingDirection = dir;
                
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                {
                    anim.SetFloat("MoveX", dir.x > 0 ? 1f : -1f);
                    anim.SetFloat("MoveY", 0f);
                }
                else
                {
                    anim.SetFloat("MoveX", 0f);
                    anim.SetFloat("MoveY", dir.y > 0 ? 1f : -1f);
                }
            }
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Зеленый конус/луч — куда сейчас направлен взгляд ИИ
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, (Vector3)lastFacingDirection * chaseDistance);

        // Синяя зона — радиус "слуха" со спины
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, backChaseDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);

        if (currentState == State.Search)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(searchTargetPosition, 0.4f);
            Gizmos.DrawLine(lastKnownPlayerPosition, searchTargetPosition);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // ���� ������ � ��������, �� �� ����� ���������
        if (currentState == State.Stun) return;

        // ��������� ������� ����� (����� ��� ������ � ��� �������)
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 1. ������ ����� ������
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
            // 2. ����� ������ ����� �������
            else if (collision.gameObject.CompareTag("Item"))
            {
                BoxImpact box = collision.gameObject.GetComponent<BoxImpact>();
                if (box != null)
                {
                    // ������ ������� ������� 1 ������� ����� (������� 1 �� �����)
                    box.TakeDamage(1);
                    lastAttackTime = Time.time;

                    // ��������� �� �� ����� �������� ����� �����/�������!
                    if (anim != null) anim.SetTrigger("Attack");
                }
            }
        }
    }
}