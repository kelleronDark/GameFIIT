using System.Collections;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    public Transform holdPoint;
    private GameObject carriedItem;

    public HealthBarController healthBar;

    private Vector3 lastCheckpointPos;

    [Header("Throw Settings")]
    public GameObject throwCursor;
    public float maxThrowDistance = 5f;
    public float throwHeight = 2f;
    public float throwSpeed = 2f;
    private bool isAiming = false;
    
    [Header("Interaction Settings")]
    public float interactionRadius = 1f; 
    public float pickupRadius = 1f;

    [Header("Arc Visualization")]
    private LineRenderer lineRenderer;
    public int arcResolution = 15;

    [Header("Death Screen")]
    public DeathScreen deathScreen;
    
    private int originalItemLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        Debug.Log($"Здоровье игрока: {currentHealth}/{maxHealth}");

        lastCheckpointPos = transform.position;

        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            Vector3 cpPos = SaveManager.Instance.GetSavedCheckpointPosition();

            transform.position = cpPos;
            lastCheckpointPos = cpPos;

            Debug.Log($"Игрок возродился на чекпоинте: {cpPos}");
        }
        else
        {
            lastCheckpointPos = transform.position;
        }

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null) cam.Warp();

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogError("HealthBar не назначен в PlayerController!");
        }

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = arcResolution;
            lineRenderer.enabled = false;
        }
    }

    public void SetCheckpoint(Vector3 newPosition)
    {
        lastCheckpointPos = newPosition;
        Debug.Log("Точка возрождения обновлена!");
    }

    void Update()
    {
        if (isDead) 
        {
            moveInput = Vector2.zero;
            if (throwCursor != null) throwCursor.SetActive(false);
            if (lineRenderer != null) lineRenderer.enabled = false;
            return; 
        }
        
        if (Keyboard.current != null)
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y = 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y = -1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x = 1;

            moveInput = input;
        }

        anim.SetFloat("MoveX", Mathf.Abs(moveInput.x));
        anim.SetFloat("MoveY", moveInput.y);

        Vector3 currentScale = transform.localScale;
        if (moveInput.x < 0)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);
        }
        else if (moveInput.x > 0)
        {
            currentScale.x = Mathf.Abs(currentScale.x);
        }
        transform.localScale = currentScale;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            bool interactedWithObject = false;

            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, interactionRadius);

            foreach (var hit in nearbyObjects)
            {
                HealthPotion potion = hit.GetComponent<HealthPotion>();
                if (potion != null)
                {
                    potion.UsePotion();
                    interactedWithObject = true;
                    break;
                }

                Chest chest = hit.GetComponent<Chest>();
                if (chest != null && chest.IsPlayerInRange && !chest.isOpened)
                {
                    chest.OpenChest();
                    interactedWithObject = true;
                    break;
                }

                Door door = hit.GetComponent<Door>();
                if (door != null &&  door.IsPlayerInRange && !door.isOpened)
                {
                    door.TryOpen();
                    interactedWithObject = true;
                    break;
                }
            }

            if (!interactedWithObject)
            {
                if (carriedItem == null)
                {
                    TryPickUp();
                }
                else
                {
                    DropItem();
                }
            }
        }

        if (carriedItem != null && Mouse.current.rightButton.isPressed)
        {
            isAiming = true;
            throwCursor.SetActive(true);

            if (lineRenderer != null) lineRenderer.enabled = true;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0;

            float dist = Vector2.Distance(transform.position, mousePos);
            if (dist > maxThrowDistance)
            {
                mousePos = transform.position + (mousePos - transform.position).normalized * maxThrowDistance;
            }

            throwCursor.transform.position = mousePos;

            if (lineRenderer != null)
            {
                DrawTrajectoryArc(holdPoint.position, mousePos);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (lineRenderer != null) lineRenderer.enabled = false;
                StartCoroutine(ThrowItem(carriedItem, mousePos));
                carriedItem = null;
                isAiming = false;
                throwCursor.SetActive(false);
            }
        }
        else
        {
            isAiming = false;
            if (throwCursor != null) throwCursor.SetActive(false);
            if (lineRenderer != null) lineRenderer.enabled = false;
        }
    }

    void TryPickUp()
    {
        Debug.Log("TryPickUp: Начало проверки...");
    
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        Debug.Log($"TryPickUp: Найдено объектов в радиусе: {hits.Length}");

        foreach (var hit in hits)
        {
            Debug.Log($"TryPickUp: Проверка объекта: {hit.gameObject.name}, Tag: {hit.gameObject.tag}");
        
            if (hit.gameObject != gameObject && hit.CompareTag("Item"))
            {
                Debug.Log("TryPickUp: Найден предмет с тегом 'Item'!");
            
                carriedItem = hit.gameObject;
                originalItemLayer = carriedItem.layer;

                carriedItem.layer = LayerMask.NameToLayer("Ignore Raycast");

                Bounds boxBounds = carriedItem.GetComponent<Collider2D>().bounds;

                carriedItem.transform.SetParent(holdPoint);
                carriedItem.transform.localPosition = Vector3.zero;

                if (carriedItem.GetComponent<Rigidbody2D>())
                    carriedItem.GetComponent<Rigidbody2D>().simulated = false;

                Collider2D col = carriedItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                
                SetLayerRecursively(carriedItem, 2);

                if (AstarPath.active != null)
                {
                    AstarPath.active.UpdateGraphs(boxBounds);
                }

                Debug.Log("Tutorial: Ищем BoxTutorialManager...");
                BoxTutorialManager tutorial = FindObjectOfType<BoxTutorialManager>();
            
                if (tutorial == null)
                {
                    Debug.LogError("Tutorial: BoxTutorialManager НЕ найден на сцене!");
                }
                else
                {
                    Debug.Log("Tutorial: BoxTutorialManager найден!");
                    tutorial.ShowBoxTutorial();
                }

                break;
            }
        }
    }

    void DropItem()
    {
        if (carriedItem == null) return;
        
        carriedItem.layer = LayerMask.NameToLayer("DynamicObstacles");

        Collider2D col = carriedItem.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        
        SetLayerRecursively(carriedItem, originalItemLayer);

        carriedItem.transform.SetParent(null);

        if (carriedItem.GetComponent<Rigidbody2D>())
            carriedItem.GetComponent<Rigidbody2D>().simulated = true;

        if (col != null && AstarPath.active != null)
        {
            AstarPath.active.UpdateGraphs(col.bounds);
        }

        carriedItem = null;
    }

    void DrawTrajectoryArc(Vector3 startPos, Vector3 targetPos)
    {
        for (int i = 0; i < arcResolution; i++)
        {
            float t = (float)i / (arcResolution - 1);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            float height = Mathf.Sin(t * Mathf.PI) * throwHeight;
            currentPos.y += height;

            lineRenderer.SetPosition(i, currentPos);
        }
    }

    IEnumerator ThrowItem(GameObject item, Vector3 targetPos)
    {
        item.transform.SetParent(null);
        Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
        Collider2D itemCol = item.GetComponent<Collider2D>();

        if (itemCol)
        {
            itemCol.enabled = true;
            itemCol.isTrigger = true;
        }

        if (itemRb) itemRb.simulated = false;

        Vector3 startPos = item.transform.position;
        float timer = 0;

        while (timer < 1f)
        {
            timer += Time.deltaTime * throwSpeed;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, timer);
            float height = Mathf.Sin(timer * Mathf.PI) * throwHeight;
            currentPos.y += height;

            item.transform.position = currentPos;
            yield return null;
        }

        if (itemRb) itemRb.simulated = true;
        if (itemCol) itemCol.isTrigger = false;
        
        SetLayerRecursively(item, originalItemLayer);

        item.layer = LayerMask.NameToLayer("DynamicObstacles");

        BoxImpact impact = item.GetComponent<BoxImpact>();
        if (impact == null) impact = item.AddComponent<BoxImpact>();
        impact.ActivateImpact();
    }

    void FixedUpdate()
    {
        if (isDead) return;
        
        if (moveInput.magnitude > 0)
        {
            Vector2 movement = moveInput.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }
    
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        healthBar.SetHealth(currentHealth, maxHealth);

        Debug.Log($"Игрок получил {damage} урона. Здоровье: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(FlashSpriteRed());

        IEnumerator FlashSpriteRed()
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.15f);
                sr.color = Color.white;
            }
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        Debug.Log($"Игрок восстановил {amount} HP. Здоровье: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        Debug.Log("Игрок погиб! Показываем экран смерти...");
        
        isDead = true; 
        rb.linearVelocity = Vector2.zero; 
        anim.SetFloat("MoveX", 0f);
        anim.SetFloat("MoveY", 0f);

        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
        else
        {
            Debug.LogWarning("️DeathScreen не назначен в PlayerController!");
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }

    public void ForceDropItem()
    {
        if (carriedItem != null)
        {
            DropItem();
        }
    }

    public Vector3 GetLastCheckpointPos()
    {
        return lastCheckpointPos;
    }
}