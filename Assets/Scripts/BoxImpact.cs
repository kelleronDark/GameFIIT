using UnityEngine;
using System.Collections;
using Pathfinding;

public class BoxImpact : MonoBehaviour
{
    [Header("Destruction Settings")]
    public int boxHealth = 3;
    [Header("Glow Settings")]
    public bool enableGlow = true;
    public Color glowColor = new Color(1f, 0.9f, 0.6f, 1f);
    public float pulseSpeed = 3f;
    public float pulseIntensity = 0.15f;
    public float throwCooldown = 3f;

    private bool canStun = false;
    public float explosionRadius = 1.5f;
    
    private bool isRecentlyThrown = false;
    private SpriteRenderer sr;
    private Collider2D col;
    private Color originalColor;
    
    public bool IsFlyingAndCanStun => canStun;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            Debug.LogError($"BoxImpact: SpriteRenderer НЕ найден на {gameObject.name}!");
        else
            Debug.Log($"BoxImpact: SpriteRenderer найден. Original color: {sr.color}");
    
        if (col == null)
            Debug.LogError($"BoxImpact: Collider2D НЕ найден на {gameObject.name}!");
        if (sr != null)
            originalColor = sr.color;
        
        UpdateAstarGraph();
    }

    public void ActivateImpact()
    {
        canStun = true;
        StartCoroutine(CheckAreaForTime(0.5f));
        UpdateAstarGraph();
        
        StartThrowCooldown();
    }

    private void StartThrowCooldown()
    {
        if (enableGlow)
        {
            isRecentlyThrown = true;
            StartCoroutine(ResetThrowCooldown());
        }
    }

    private IEnumerator ResetThrowCooldown()
    {
        yield return new WaitForSeconds(throwCooldown);
        isRecentlyThrown = false;
    }

    void Update()
    {
        if (enableGlow && sr != null)
        {
            UpdateGlow();
        }
    }

    private void UpdateGlow()
    {
        bool shouldGlow = col != null && col.enabled && !isRecentlyThrown;

        if (shouldGlow)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            sr.color = Color.Lerp(originalColor, glowColor, pulse * pulseIntensity);
        }
        else
        {
            sr.color = originalColor;
        }
    }
    
    IEnumerator CheckAreaForTime(float time)
    {
        float timer = 0;
        while (timer < time && canStun)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius);
            if (hit != null && hit.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    StartCoroutine(enemy.BecomeStunned(3f));
                    canStun = false;
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        canStun = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canStun) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                canStun = false; 
            
                StartCoroutine(enemy.BecomeStunned(3f));
                Debug.Log("Монстр получил летящей коробкой прямо по голове!");
            }
        }
    }

    private void UpdateAstarGraph()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null && AstarPath.active != null)
        {
            GraphUpdateObject guo = new GraphUpdateObject(col.bounds);

            guo.modifyWalkability = true;
            guo.setWalkability = true;

            guo.modifyTag = true;
            guo.setTag = 1;

            AstarPath.active.UpdateGraphs(guo);
        }
    }   

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void TakeDamage(int damage)
    {
        boxHealth -= damage;

        if (boxHealth <= 0)
        {
            DestroyBox();
        }
    }

    private void DestroyBox()
    {
        Debug.Log($"Коробка {gameObject.name} уничтожена!");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && AstarPath.active != null)
        {
            col.enabled = false;

            GraphUpdateObject guo = new GraphUpdateObject(col.bounds);
            guo.modifyWalkability = true;
            guo.setWalkability = true;
            guo.modifyTag = true;
            guo.setTag = 0;

            AstarPath.active.UpdateGraphs(guo);
        }

        Destroy(gameObject);
    }
}