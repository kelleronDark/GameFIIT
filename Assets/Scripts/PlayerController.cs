using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    
    [Header("UI")]
    public UnityEngine.UI.Slider healthSlider; // <-- сюда перетащишь Slider из Unity

    void Start()
    {
        Debug.Log("Игрок создан! Проверка New Input System.");
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // Инициализация здоровья
        currentHealth = maxHealth;
        Debug.Log($"Здоровье игрока: {currentHealth}/{maxHealth}");
        
        UpdateHealthUI(); // <-- ДОБАВЬ ЭТУ СТРОКУ
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
                if (hit.CompareTag("Interactable"))
                {
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
            }

            // 2. Если ничего не открыли, выполняем старую логику с предметами
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

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            // Рассчитываем процент здоровья (от 0 до 1)
            float healthPercent = (float)currentHealth / maxHealth;
            healthSlider.value = healthPercent;

            // Меняем цвет заполнения: зелёный → жёлтый → красный
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (healthPercent > 0.6f)
                    fillImage.color = Color.green;
                else if (healthPercent > 0.3f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }
    }

    /// <summary>
    /// Получить урон
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI(); // обновляем полоску

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

        UpdateHealthUI(); // обновляем полоску

        Debug.Log($"Игрок восстановил {amount} HP. Здоровье: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Смерть игрока
    /// </summary>
    private void Die()
    {
        Debug.Log("Игрок погиб!");
        enabled = false; // отключаем управление (временно)
        // Или: Destroy(gameObject); если хочешь удалить игрока
    }
}