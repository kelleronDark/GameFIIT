using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class MerchantAI : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 2f;
    public float walkDistance = 3f;

    [Header("Настройки диалога")]
    public GameObject dialoguePanel;       // Панель для текста (можно без фона)
    public TextMeshProUGUI dialogueText;   // Текст TMP

    [TextArea(3, 5)]
    public string[] merchantPhrases = new string[]
    {
        "Привет, путник! Есть пара золотых?",
        "У меня есть зелья, ключи... даже секреты.",
        "Но только если ты заслужишь доверие...",
        "Нажми F ещё раз — я расскажу больше."
    };

    public float typingSpeed = 0.04f;      // Скорость печати
    public AudioClip typeSound;            // Звук клика при печати (опционально)

    [Header("UI Hint")]
    public GameObject hintPrefab;          // Префаб подсказки "Нажмите F"

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool movingRight = true;
    private bool isTalking = false;
    private bool isPlayerNearby = false;
    private Coroutine typingCoroutine;
    private int currentPhraseIndex = 0;

    private AudioSource audioSource;
    private GameObject currentHint;

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
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Настраиваем звук (если есть)
        if (typeSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.3f;
        }
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
                    // Если уже печатаем — останавливаем
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);

                    // Запускаем печать текущей фразы
                    typingCoroutine = StartCoroutine(TypeText(merchantPhrases[currentPhraseIndex]));

                    // Переходим к следующей фразе (циклически)
                    currentPhraseIndex = (currentPhraseIndex + 1) % merchantPhrases.Length;

                    // АЛЬТЕРНАТИВА: если хочешь, чтобы диалог заканчивался после последней фразы:
                    /*
                    if (currentPhraseIndex < merchantPhrases.Length - 1)
                        currentPhraseIndex++;
                    else
                        isTalking = false; // Закрываем диалог
                    */
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

        // Логика движения туда-сюда
        float leftBoundary = startPos.x - walkDistance;
        float rightBoundary = startPos.x + walkDistance;

        if (movingRight && transform.position.x >= rightBoundary) 
            movingRight = false;
        else if (!movingRight && transform.position.x <= leftBoundary) 
            movingRight = true;

        float direction = movingRight ? 1 : -1;
        Vector2 nextPos = rb.position + new Vector2(direction * speed * Time.fixedDeltaTime, 0);
        rb.MovePosition(nextPos);

        // Разворот спрайта
        float scaleX = Mathf.Abs(transform.localScale.x) * direction;
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

    // Эффект печатной машинки со звуком
    IEnumerator TypeText(string line)
    {
        dialogueText.text = ""; 
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            
            if (audioSource != null && typeSound != null)
                audioSource.PlayOneShot(typeSound);

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = true;
            ShowHint();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = false;
            HideHint();
            isTalking = false;
            if (dialoguePanel != null) 
                dialoguePanel.SetActive(false);
            if (typingCoroutine != null) 
                StopCoroutine(typingCoroutine);
        }
    }

    // === МЕТОДЫ ДЛЯ ПОДСКАЗКИ ===

    void ShowHint()
    {
        if (currentHint != null) return;

        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        TextMeshProUGUI hintText = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (hintText != null)
            hintText.text = "Нажмите F";

        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }

    void HideHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
    }
}