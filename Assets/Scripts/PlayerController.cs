using UnityEngine;
using UnityEngine.InputSystem;

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
    
    public SaveNotification saveUI;
    public static Vector3 lastCheckPointPos;

    void Start()
    {
        Debug.Log("Игрок создан! Проверка New Input System.");
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    
        // Проверяем, есть ли вообще запись о сохранении в памяти
        if (PlayerPrefs.HasKey("p_x"))
        {
            // Если запись есть — загружаем координаты
            float savedX = PlayerPrefs.GetFloat("p_x");
            float savedY = PlayerPrefs.GetFloat("p_y");
            lastCheckPointPos = new Vector3(savedX, savedY, 0);
            transform.position = lastCheckPointPos;
            Debug.Log("Позиция загружена из сохранения.");
        }
        else
        {
            // Если сохранений нет (первый запуск), то текущая точка — наш первый чекпоинт
            lastCheckPointPos = transform.position;
            Debug.Log("Сохранений не найдено. Игрок на стартовой позиции сцены.");
        }
    
        currentHealth = maxHealth;
        Debug.Log($"Здоровье игрока: {currentHealth}/{maxHealth}");
    }

    void Update()
    {
        HandleMovementInput();
        HandleInteraction();
        
        // Кнопка для ручного сохранения (например, на F5 или просто при выходе)
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            SaveGame();
            Debug.Log("Игра сохранена вручную!");
        }
    }
    
    void HandleMovementInput()
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

        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x) * Mathf.Abs(transform.localScale.y), transform.localScale.y, 1);
        }
    }
    
    void HandleInteraction()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            bool interactedWithObject = false;
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, 2f);
            
            foreach (var hit in nearbyObjects)
            {
                if (hit.CompareTag("Interactable"))
                {
                    Chest chest = hit.GetComponent<Chest>();
                    if (chest != null && !chest.isOpened)
                    {
                        chest.OpenChest();
                        interactedWithObject = true;
                        break;
                    }

                    Door door = hit.GetComponent<Door>();
                    if (door != null && !door.isOpened)
                    {
                        door.TryOpen();
                        interactedWithObject = true;
                        break;
                    }
                }
            }

            if (!interactedWithObject)
            {
                if (carriedItem == null) TryPickUp();
                else DropItem();
            }
        }
    }
    
    public void SaveGame()
    {
        PlayerPrefs.SetFloat("p_x", transform.position.x);
        PlayerPrefs.SetFloat("p_y", transform.position.y);
        PlayerPrefs.Save();

        // Если мы привязали скрипт уведомления, показываем его
        if (saveUI != null)
        {
            saveUI.Show();
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
                carriedItem.transform.SetParent(holdPoint);
                carriedItem.transform.localPosition = Vector3.zero;

                if (carriedItem.GetComponent<Rigidbody2D>())
                    carriedItem.GetComponent<Rigidbody2D>().simulated = false;

                break;
            }
        }
    }

    void DropItem()
    {
        carriedItem.transform.SetParent(null);

        if (carriedItem.GetComponent<Rigidbody2D>())
            carriedItem.GetComponent<Rigidbody2D>().simulated = true;

        carriedItem = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }

    void FixedUpdate()
    {
        if (moveInput.magnitude > 0)
        {
            Vector2 movement = moveInput.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }

    // === НОВЫЕ МЕТОДЫ ДЛЯ ЗДОРОВЬЯ ===

    /// <summary>
    /// Получить урон
    /// </summary>
    /// <param name="damage">Количество урона</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Игрок получил {damage} урона. Здоровье: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Смерть игрока
    /// </summary>
    private void Die()
    {
        Debug.Log("Игрок погиб! Возврат на чекпоинт.");
        
        // Сбрасываем здоровье
        currentHealth = maxHealth;

        // Перемещаем на последний чекпоинт
        transform.position = lastCheckPointPos;
        
        // Сбрасываем инерцию, чтобы игрока не "несло" после респауна
        if(rb != null) rb.linearVelocity = Vector2.zero;

        // Если нес предмет — бросаем его
        if (carriedItem != null) DropItem();
    }

    /// <summary>
    /// Восстановить здоровье (на будущее)
    /// </summary>
    /// <param name="amount">Сколько восстановить</param>
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        Debug.Log($"Игрок восстановил {amount} HP. Здоровье: {currentHealth}/{maxHealth}");
    }
    
    public void SetNewCheckPoint(Vector3 pos)
    {
        lastCheckPointPos = pos;
        SaveGame(); // Автоматически сохраняем позицию в PlayerPrefs
        Debug.Log("Чекпоинт обновлен в скрипте игрока!");
    }
}