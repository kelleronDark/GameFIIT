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
    public float attackCooldown = 2.1f;
    public float attackRange = 1.2f;
    private float lastAttackTime;
    private GameObject currentAttackTarget;

    [Header("Detection Settings")]
    [Tooltip("Дистанция зрения перед собой")]
    public float chaseDistance = 6f; 
    [Tooltip("Дистанция зрения сзади")]
    public float backChaseDistance = 3.5f;
    [Tooltip("Дистанция, при которой монстр теряет интерес")]
    public float stopChaseDistance = 8f;
    public float playerDistError = 0.4f;
    public LayerMask obstacleMask;
    
    private float changeDirectionTimer = 0f;
    [Header("Damping Settings")]
    public float directionChangeDamping = 0.08f;
    private Vector2 movementIntention = Vector2.zero;
    
    [Header("Layer Settings")]
    public string doorLayerName = "DynamicObs";

    [Header("Search Settings")]
    public float searchWaitDuration = 3f;
    public float predictionDistance = 2.5f;

    [Header("VFX References")]
    public GameObject stunEffectObject;

    private IAstarAI ai;
    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private float searchTimer;
    private float stuckCheckTimer;
    
    private Vector2 searchTargetPosition;
    private Vector2 lastKnownPlayerPosition;
    
    private Vector2 lastPlayerDirection = Vector2.zero;
    private Vector3 lastPlayerPosTrack;
    
    private Vector2 lastFacingDirection = Vector2.down;
    
    private float originalMaxSpeed;

    void Start()
    {
        ai = GetComponent<IAstarAI>();
        rb = GetComponent<Rigidbody2D>();
        if (ai != null) originalMaxSpeed = ai.maxSpeed;
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
                    ai.destination = player.position;
                    
                    TryAttackTarget();
                    
                    if (!canSeePlayer)
                    {
                        Vector2 rawPredictedTarget = lastKnownPlayerPosition + (lastPlayerDirection * predictionDistance);
                        Vector2 finalSearchTarget = lastKnownPlayerPosition;
                        
                        RaycastHit2D hitCheck = Physics2D.Raycast(lastKnownPlayerPosition, lastPlayerDirection, predictionDistance, obstacleMask);
                        
                        if (hitCheck.collider != null)
                        {
                            finalSearchTarget = hitCheck.point - (lastPlayerDirection * 0.4f);
                        }
                        
                        else
                        {
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
    
    private void TryAttackTarget()
    {
        if (player == null || currentState == State.Stun) return;
        
        if (currentAttackTarget != null) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                currentAttackTarget = player.gameObject;
                lastAttackTime = Time.time;

                if (anim != null) 
                    anim.SetTrigger("Attack");
                else 
                    ExecuteDirectDamage();
            }
        }
    }
    
    public void OnAttackAnimationHit()
    {
        ExecuteDirectDamage();
    }
    
    private void ExecuteDirectDamage()
    {
        if (currentAttackTarget == null || currentState == State.Stun) return;

        float dist = Vector2.Distance(transform.position, currentAttackTarget.transform.position);
        if (dist <= attackRange + 1.5f)
        {
            if (currentAttackTarget == player.gameObject)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damageAmount);
                    Debug.Log("[Монстр] Нанес урон игроку!");
                }
            }
            else if (currentAttackTarget.CompareTag("Item"))
            {
                BoxImpact box = currentAttackTarget.GetComponent<BoxImpact>();
                if (box != null)
                {
                    box.TakeDamage(1);
                    Debug.Log("[Монстр] Ударил по коробке, игрок в безопасности.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[Монстр] Промах по объекту {currentAttackTarget.name}! Дистанция: {dist}, а надо хотя бы {attackRange + 1.5f}");
        }
        
        currentAttackTarget = null;
    }
    
    bool EvaluateLineOfSight(float maxDistance, out RaycastHit2D hitResult)
    {
        hitResult = new RaycastHit2D();
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        
        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        
        float cosAngle = Vector2.Dot(lastFacingDirection, directionToPlayer);
        
        float allowedDistance = maxDistance;

        if (cosAngle < 0.35f)
        {
            allowedDistance = backChaseDistance;
        }
        
        if (distance > allowedDistance) return false;

        hitResult = Physics2D.Raycast(transform.position, directionToPlayer, distance, obstacleMask);

        if (hitResult.collider == null)
        {
            Debug.DrawLine(transform.position, player.position, Color.green);
            return true;
        }

        Debug.DrawLine(transform.position, hitResult.point, Color.red);
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
            if (rb != null) rb.linearVelocity = Vector2.zero;
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
            ai.isStopped = true;
            
            if (rb != null) rb.linearVelocity = Vector2.zero;

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                ai.isStopped = false;
                currentState = State.Patrol;
            }
        }
        else
        {
            ai.isStopped = false;
            ai.destination = searchTargetPosition;
        }
    }

    private Coroutine stunCoroutine;

    public IEnumerator BecomeStunned(float duration)
    {
        if (currentState == State.Stun) yield break;
        
        currentState = State.Stun;
        
        currentAttackTarget = null;
        
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunExecution(duration));
    }

    private IEnumerator StunExecution(float duration)
    {
        if (ai != null)
        {
            ai.isStopped = true;
            ai.maxSpeed = 0f;
            if (ai is MonoBehaviour aiComponent) aiComponent.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.SetBool("IsStunned", true);
            anim.SetBool("isMoving", false);
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

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (ai != null)
        {
            if (ai is MonoBehaviour aiComponent) aiComponent.enabled = true;
            ai.isStopped = false;
            ai.maxSpeed = originalMaxSpeed;
        }

        currentState = State.Patrol;
        stunCoroutine = null;
    }

    void UpdateAnimation()
    {
        if (anim == null || ai == null) return;
        
        Vector2 desiredDir = ai.desiredVelocity;
        float speed = desiredDir.magnitude;

        if (speed > 0.5f && currentState != State.Stun && !ai.isStopped && !ai.reachedDestination)
        {
            Vector2 normalizedDir = desiredDir.normalized;
            
            Vector2 snapedDir = Vector2.zero;
            if (Mathf.Abs(normalizedDir.x) > Mathf.Abs(normalizedDir.y))
            {
                snapedDir = new Vector2(normalizedDir.x > 0 ? 1f : -1f, 0f);
            }
            else
            {
                snapedDir = new Vector2(0f, normalizedDir.y > 0 ? 1f : -1f);
            }

            if (snapedDir == movementIntention)
            {
                changeDirectionTimer = 0f;
            }
            else
            {
                if (movementIntention == Vector2.zero)
                {
                    movementIntention = snapedDir;
                    changeDirectionTimer = 0f;
                }
                else
                {
                    changeDirectionTimer += Time.deltaTime;

                    if (changeDirectionTimer >= directionChangeDamping)
                    {
                        movementIntention = snapedDir;
                        changeDirectionTimer = 0f;
                    }
                }
            }
            
            if (movementIntention != Vector2.zero)
            {
                lastFacingDirection = movementIntention;
                
                anim.SetFloat("MoveX", movementIntention.x);
                anim.SetFloat("MoveY", movementIntention.y);
                anim.SetBool("isMoving", true);
            }
        }
        else
        {
            anim.SetBool("isMoving", false);
            anim.SetFloat("MoveX", 0f);
            anim.SetFloat("MoveY", 0f);
            
            changeDirectionTimer = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, (Vector3)lastFacingDirection * chaseDistance);

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
        if (currentState == State.Stun) return;
        
        if (currentAttackTarget != null) return;
        
        if (collision.gameObject.CompareTag("Item"))
        {
            BoxImpact box = collision.gameObject.GetComponent<BoxImpact>();
            if (box != null && !box.IsFlyingAndCanStun)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    currentAttackTarget = collision.gameObject;
                    lastAttackTime = Time.time;
                    if (anim != null) anim.SetTrigger("Attack");
                }
            }
        }
    }
}