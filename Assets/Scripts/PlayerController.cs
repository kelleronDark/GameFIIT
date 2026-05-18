using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public int maxHealth = 100; // Максимальное здоровье
    private int currentHealth;  // Текущее здоровье

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    public Transform holdPoint;
    private GameObject carriedItem;

    public HealthBarController healthBar; // Назначь в инспекторе

    // [Header("UI")]
    // public UnityEngine.UI.Slider healthSlider; // <-- сюда перетащишь Slider из Unity
    private Vector3 lastCheckpointPos;

    [Header("Throw Settings")]
    public GameObject throwCursor; // Тот самый объект прицела
    public float maxThrowDistance = 5f;
    public float throwHeight = 2f; // Высота дуги
    public float throwSpeed = 2f;  // Время полета (сек)
    private bool isAiming = false;

    [Header("Arc Visualization")]
    private LineRenderer lineRenderer;
    public int arcResolution = 15; // Количество точек дуги (чем больше, тем плавнее)

    [Header("Death Screen")]
    public DeathScreen deathScreen; // Назначь в инспекторе

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

            // Телепортируем игрока
            transform.position = cpPos;
            lastCheckpointPos = cpPos; // Обновляем локальную переменную

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
            Debug.LogError("❌ HealthBar не назначен в PlayerController!");
        }

        // Находим или добавляем Line Renderer на ходу
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = arcResolution;
            lineRenderer.enabled = false; // Выключен по умолчанию
        }

        // UpdateHealthUI(); // <-- ДОБАВЬ ЭТУ СТРОКУ
    }

    public void SetCheckpoint(Vector3 newPosition)
    {
        lastCheckpointPos = newPosition;
        Debug.Log("Точка возрождения обновлена!");
    }

    void Update()
    {
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

            // 1. Проверяем все интерактивные объекты рядом
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, 2f);

            foreach (var hit in nearbyObjects)
            {
                // --- ОБНОВЛЕННАЯ ПРОВЕРКА НА ЗЕЛЬЕ ---
                HealthPotion potion = hit.GetComponent<HealthPotion>();
                if (potion != null)
                {
                    // Просто отдаем команду самой хилке. 
                    // Метод UsePotion сам проверит флаг isUsed, выключит коллайдер и полечит игрока!
                    potion.UsePotion();
                    interactedWithObject = true;
                    break;
                }

                // Проверяем на сундук
                Chest chest = hit.GetComponent<Chest>();
                if (chest != null && !chest.isOpened)
                {
                    chest.OpenChest();
                    interactedWithObject = true;
                    break;
                }

                // Проверяем на дверь
                Door door = hit.GetComponent<Door>();
                if (door != null && !door.isOpened)
                {
                    door.TryOpen();
                    interactedWithObject = true;
                    break;
                }
            }

            // 2. Если ничего не взаимодействовали, работаем с предметами в руках
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

        // ПРИЦЕЛИВАНИЕ (ПКМ)
        if (carriedItem != null && Mouse.current.rightButton.isPressed)
        {
            isAiming = true;
            throwCursor.SetActive(true);

            // Включаем отображение линии траектории
            if (lineRenderer != null) lineRenderer.enabled = true;

            // Получаем позицию мыши в мире
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0;

            // Ограничиваем дистанцию броска
            float dist = Vector2.Distance(transform.position, mousePos);
            if (dist > maxThrowDistance)
            {
                mousePos = transform.position + (mousePos - transform.position).normalized * maxThrowDistance;
            }

            throwCursor.transform.position = mousePos;

            // Строим дугу от точки HoldPoint до курсора прицела
            if (lineRenderer != null)
            {
                DrawTrajectoryArc(holdPoint.position, mousePos);
            }

            // БРОСОК (ЛКМ во время прицеливания)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (lineRenderer != null) lineRenderer.enabled = false; // Отключаем линию
                StartCoroutine(ThrowItem(carriedItem, mousePos));
                carriedItem = null; // Ссылка в руках обнуляется сразу
                isAiming = false;
                throwCursor.SetActive(false);
            }
        }
        else
        {
            isAiming = false;
            if (throwCursor != null) throwCursor.SetActive(false);
            // Прячем линию, если не целимся
            if (lineRenderer != null) lineRenderer.enabled = false;
        }
    }

    void TryPickUp()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Item"))
            {
                carriedItem = hit.gameObject;

                // ДИНАМИЧЕСКИЙ А*: Запоминаем границы коробки ДО того, как отключим её коллайдер
                Bounds boxBounds = carriedItem.GetComponent<Collider2D>().bounds;

                carriedItem.transform.SetParent(holdPoint);
                carriedItem.transform.localPosition = Vector3.zero;

                // 1. Отключаем физику движения
                if (carriedItem.GetComponent<Rigidbody2D>())
                    carriedItem.GetComponent<Rigidbody2D>().simulated = false;

                // 2. ОТКЛЮЧАЕМ КОЛЛАЙДЕР, чтобы игрок не становился широким
                Collider2D col = carriedItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                // ДИНАМИЧЕСКИЙ А*: Освобождаем сетку на месте, где коробка только что лежала
                if (AstarPath.active != null)
                {
                    AstarPath.active.UpdateGraphs(boxBounds);
                }

                break;
            }
        }
    }

    void DropItem()
    {
        if (carriedItem == null) return;

        // Включаем коллайдер обратно перед тем как бросить под ноги
        Collider2D col = carriedItem.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        carriedItem.transform.SetParent(null);

        if (carriedItem.GetComponent<Rigidbody2D>())
            carriedItem.GetComponent<Rigidbody2D>().simulated = true;

        // ДИНАМИЧЕСКИЙ А*: Пересчитываем сетку в месте падения коробки (чтобы враг её обходил)
        // Делаем это ДО того, как сотрем ссылку в carriedItem = null;
        if (col != null && AstarPath.active != null)
        {
            AstarPath.active.UpdateGraphs(col.bounds);
        }

        // Теперь, когда граф обновился, можно смело очищать ссылку
        carriedItem = null;
    }

    void DrawTrajectoryArc(Vector3 startPos, Vector3 targetPos)
    {
        for (int i = 0; i < arcResolution; i++)
        {
            float t = (float)i / (arcResolution - 1);

            // Прямая линия между игроком и целью
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // Искривление вверх по синусоиде (точно такая же парабола, как при броске)
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

        // Включаем коллайдер (он был выключен при подборе), но делаем его триггером
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
        if (itemCol) itemCol.isTrigger = false; // Возвращаем обычную физику

        BoxImpact impact = item.GetComponent<BoxImpact>();
        if (impact == null) impact = item.AddComponent<BoxImpact>();
        impact.ActivateImpact();
    }

    void FixedUpdate()
    {
        if (moveInput.magnitude > 0)
        {
            Vector2 movement = moveInput.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }

    /// <summary>
    /// Получить урон
    /// </summary>
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
    }

    /// <summary>
    /// Восстановить здоровье
    /// </summary>
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

    /// <summary>
    /// Смерть игрока
    /// </summary>
    private void Die()
    {
        Debug.Log("Игрок погиб! Показываем экран смерти...");

        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
        else
        {
            Debug.LogWarning("️ DeathScreen не назначен в PlayerController!");
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