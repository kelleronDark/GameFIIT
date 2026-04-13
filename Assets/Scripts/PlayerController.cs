using UnityEngine;
using UnityEngine.InputSystem; // 1. Обязательно добавляем этот namespace

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    public Transform holdPoint;
    private GameObject carriedItem;

    void Start()
    {
        Debug.Log("Скрипт запущен! Использую New Input System.");
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 2. Новый способ получения ввода для WASD/Стрелок
        // Это самый простой способ быстро починить код в новой системе
        if (Keyboard.current != null)
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y = 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y = -1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x = 1;

            moveInput = input;
        }

        // 3. Анимации (твой рабочий код без изменений)
        anim.SetFloat("MoveX", Mathf.Abs(moveInput.x));
        anim.SetFloat("MoveY", moveInput.y);

        // 4. Поворот персонажа (твой рабочий код без изменений)
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

        if (Keyboard.current.fKey.wasPressedThisFrame) // Новая система ввода
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

    void TryPickUp()
    {
        // Ищем ВСЕ коллайдеры в радиусе
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        foreach (var hit in hits)
        {
            // Проверяем, что это не мы сами и что у объекта есть тег Item
            if (hit.gameObject != gameObject && hit.CompareTag("Item"))
            {
                carriedItem = hit.gameObject;
                carriedItem.transform.SetParent(holdPoint);
                carriedItem.transform.localPosition = Vector3.zero;

                if (carriedItem.GetComponent<Rigidbody2D>())
                    carriedItem.GetComponent<Rigidbody2D>().simulated = false;

                break; // Нашли предмет — выходим из цикла
            }
        }
    }

    void DropItem()
    {
        carriedItem.transform.SetParent(null); // Убираем родство

        if (carriedItem.GetComponent<Rigidbody2D>())
            carriedItem.GetComponent<Rigidbody2D>().simulated = true;

        carriedItem = null;
    }

    private void OnDrawGizmosSelected()
    {
        // Рисует красный круг в окне Scene, показывающий радиус подбора
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }

    void FixedUpdate()
    {
        // 5. Исправленная логика движения для 2D
        if (moveInput.magnitude > 0)
        {
            Vector2 movement = moveInput.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }
}