using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Для работы с текстом

public class MerchantAI : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 2f;
    public float walkDistance = 3f;

    [Header("Настройки диалога")]
    public GameObject dialoguePanel;   // Панель (Image)
    public TextMeshProUGUI dialogueText; // Текст (TMP)
    [TextArea(3, 5)]
    public string merchantPhrase = "Привет, путник! Есть пара золотых?";
    public float typingSpeed = 0.04f; // Скорость появления букв

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool movingRight = true;
    private bool isTalking = false;
    private bool isPlayerNearby = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        // Выключаем панель в начале игры
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        // Логика нажатия F
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            isTalking = !isTalking;

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(isTalking);

                if (isTalking)
                {
                    // Если уже печатаем — останавливаем и начинаем заново
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    typingCoroutine = StartCoroutine(TypeText(merchantPhrase));
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isTalking)
        {
            rb.linearVelocity = Vector2.zero; // Стоим во время разговора
            return;
        }

        // Твоя рабочая логика движения
        float leftBoundary = startPos.x - walkDistance;
        float rightBoundary = startPos.x + walkDistance;

        if (movingRight && transform.position.x >= rightBoundary) movingRight = false;
        else if (!movingRight && transform.position.x <= leftBoundary) movingRight = true;

        float direction = movingRight ? 1 : -1;
        Vector2 nextPos = rb.position + new Vector2(direction * speed * Time.fixedDeltaTime, 0);
        rb.MovePosition(nextPos);

        // Разворот
        float scaleX = Mathf.Abs(transform.localScale.x) * direction;
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

    // Эффект печатной машинки
    System.Collections.IEnumerator TypeText(string line)
    {
        dialogueText.text = ""; 
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            isTalking = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        }
    }
}
