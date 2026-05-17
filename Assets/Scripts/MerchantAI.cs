using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public enum MerchantStoryState
{
    NotMet,             // Еще не разговаривали (Первая встреча)
    LookingAtSubmarine, // Камера летит к подлодке (блокировка управления)
    PostSubmarineOffer, // Рассказал про поломку лодки, готов объяснять правила
    SearchingForParts,  // Игрок бегает по уровню и ищет детали
    ReadyToRepair       // Все детали собраны, пора чинить!
}

public class MerchantAI : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 2f;
    public float walkDistance = 3f;

    [Header("Настройки диалога")]
    public GameObject dialoguePanel;       // Панель для текста (можно без фона)
    public TextMeshProUGUI dialogueText;   // Текст TMP
    public float typingSpeed = 0.04f;      // Скорость печати
    public AudioClip typeSound;            // Звук клика при печати (опционально)
    
    [Header("Настройки Сюжета & Камеры")]
    public Transform submarineTransform;   // Ссылка на объект подлодки на сцене
    public float cameraPanSpeed = 3f;      // Скорость полета камеры к лодке
    public float submarineViewDuration = 4f; // Сколько секунд показываем лодку игроку
    
    [Header("Настройки Инвентаря")]
    [Tooltip("Имена спрайтов деталей в инвентаре, которые нужно собрать (например, engine, propeller, battery)")]
    public string[] partItemNames = new string[] { "Part1", "Part2", "Part3", "Part4" };

    [Header("Диалоговые ветки (Реплики)")]
    [TextArea(2, 4)]
    public string[] introPhrases = new string[]
    {
        "Лавочник: Добрый день! Добро пожаловать в мою лавку! Желаешь прикупить свежих... а, стоп.",
        "Лавочник: Тьфу ты, привычка. Я ж уже лет двести ничем не торгую. Да и некому.",
        "Лавочник: Погоди-ка... Так это ты сейчас так знатно громыхнул на всю округу?",
        "Герой: (Молча смотрит на Лавочника)",
        "Лавочник: Молчишь? Ну, логично. Пойдем-ка глянем, обо что ты там врезался..."
    };
    
    [TextArea(2, 4)]
    public string[] repairOfferPhrases = new string[]
    {
        "Лавочник: Да уж... Твоя жестянка знатно пострадала. Сама она никуда не поплывет.",
        "Герой: (Молча смотрит на Лавочника)",
        "Лавочник: Не смотри так. Не буду чинить",
        "Лавочник: Хотя руки помнят! Ладно уж, починю. Но заранее говорю, запчастей нет",
        "Лавочник: Все они остались в городе. А там, понимаешь, случилась кошмарная беда! Армагедон!",
        "Лавочник: Все жители озверели, стали мутантами. Так что тебе там трындец будет.",
        "Лавочник: Короче, если пойдешь, будь осторожнее. Можешь поспрашивать меня, я подскажу, как устроен этот город."
    };
    
    [TextArea(2, 4)]
    public string[] tutorialPhrases = new string[]
    {
        "Лавочник: Как тут выживать? Слушай внимательно.",
        "Лавочник: Во-первых, двери. Железные двери открываются рычагами, дёрни со всей дури по рычагу, железо откроется.",
        "Лавочник: Деревянные открываются ключами, найди их где-нибудь. И ключ в дверь засунь.",
        "Лавочник: Обычно мой народ хранил ключи в сундуках, потому что так весело. Только дурак будет хранить ключ не в сундуке",
        "Лавочник: Во-вторых, обращай внимание на интерактивные штуки - они слегка светятся, если подойти.",
        "лавочник: Мой народ их красил блёстками, чтобы понимать что трогать можно. Ящики всякие, которые швырять можно в кого-то, например.",
        "Лавочник: Ну и в-третьих, если найдешь детали для лодки - тащи их мне да поживей!"
    };
    
    [TextArea(2, 4)]
    public string[] idleNoPartsPhrases = new string[]
    {
        "Лавочник: Эх, как же хочется продать что-нибудь кому-нибудь...",
        "Лавочник: Я знаю кучу секретов этого места, но бесплатно их не раздам! Шучу, раздам, мне просто скучно.",
        "Лавочник: Подводный мир красив, если не обращать внимания на мутантов вокруг."
    };
    
    [TextArea(2, 4)]
    public string[] idleWithPartsPhrases = new string[]
    {
        "Лавочник: Ого! Я гляжу, ты уже раздобыл какую-то железку! Тащи ее сюда!",
        "Лавочник: Прогресс налицо. Еще немного - и мы заставим эту ванну плавать."
    };
    
    [TextArea(2, 4)]
    public string[] finalPhrases = new string[]
    {
        "Лавочник: Невероятно! Ты собрал все запчасти!",
        "Лавочник: Ну что, юнга, отходи в сторону. Сейчас старый мастер покажет класс! Любуйся давай",
        "Лавочник: Руки щас покрутят тут с душой конкретно. Задраить люки! Мы отплываем из этого проклятого места!"
    };

    [Header("UI Hint")]
    public GameObject hintPrefab;          // Префаб подсказки "Нажмите F"
    
    // Внутренние переменные состояния (объявлены для контроля)
    private MerchantStoryState storyState = MerchantStoryState.NotMet;
    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool movingRight = true;
    private bool isTalking = false;
    private bool isPlayerNearby = false;
    private Coroutine typingCoroutine;
    private int currentPhraseIndex = 0;
    private string[] currentActivePhrases; // Ссылка на текущую активную ветку диалога

    private AudioSource audioSource;
    private GameObject currentHint;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        mainCamera = Camera.main;

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
        // Игрок нажимает F рядом с Лавочником
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }
    
    private void HandleInteraction()
    {
        // 1. Если текст сейчас печатается — при нажатии F мгновенно отображаем всю фразу целиком
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            dialogueText.text = currentActivePhrases[currentPhraseIndex];
            return;
        }

        // 2. Если разговор еще не был начат — запускаем его
        if (!isTalking)
        {
            StartDialogueBranch();
        }
        else
        {
            // 3. Если разговор уже идет — переходим к следующей фразе
            currentPhraseIndex++;
            if (currentPhraseIndex < currentActivePhrases.Length)
            {
                typingCoroutine = StartCoroutine(TypeText(currentActivePhrases[currentPhraseIndex]));
            }
            else
            {
                // Если фразы закончились — закрываем диалоговое окно
                EndDialogueBranch();
            }
        }
    }
    
    private void StartDialogueBranch()
    {
        isTalking = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        currentPhraseIndex = 0;

        if (storyState == MerchantStoryState.NotMet)
        {
            currentActivePhrases = introPhrases;
        }
        else if (storyState == MerchantStoryState.PostSubmarineOffer)
        {
            // Сюда мы попадем ОДИН раз, когда игрок заговорит ПОСЛЕ катсцены лодки
            currentActivePhrases = tutorialPhrases;
        }
        else if (storyState == MerchantStoryState.SearchingForParts 
                 || storyState == MerchantStoryState.LookingAtSubmarine)
        {
            // Обучение позади, теперь гоняем проверки инвентаря
            bool hasAnyParts = CheckPlayerHasAnyParts(); 
            bool hasAllParts = CheckPlayerHasAllParts(); 

            if (hasAllParts)
            {
                storyState = MerchantStoryState.ReadyToRepair;
                currentActivePhrases = finalPhrases;
            }
            else if (hasAnyParts)
            {
                currentActivePhrases = idleWithPartsPhrases;
            }
            else
            {
                if (idleNoPartsPhrases != null && idleNoPartsPhrases.Length > 0)
                {
                    int randomIndex = Random.Range(0, idleNoPartsPhrases.Length);
                    currentActivePhrases = new string[] { idleNoPartsPhrases[randomIndex] };
                }
                else
                {
                    currentActivePhrases = new string[] { "Лавочник: Мне оень нравится, когда нажимают клавишу F. Смак" };
                }
            }
        }
        else if (storyState == MerchantStoryState.ReadyToRepair)
        {
            currentActivePhrases = finalPhrases;
        }

        typingCoroutine = StartCoroutine(TypeText(currentActivePhrases[currentPhraseIndex]));
    }
    
    private void EndDialogueBranch()
    {
        isTalking = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Что происходит ПОСЛЕ завершения диалога?
        if (storyState == MerchantStoryState.NotMet)
        {
            // Закончили первое приветствие — запускаем полет камеры к подлодке
            storyState = MerchantStoryState.LookingAtSubmarine;
            StartCoroutine(CutsceneLookAtSubmarine());
        }
        else if (storyState == MerchantStoryState.LookingAtSubmarine)
        {
            // Шаг 3: Игрок дослушал туториал "Как тут выживать?" до конца -> отправляем искать детали
            storyState = MerchantStoryState.PostSubmarineOffer;
            Debug.Log("[MerchantAI] Сюжет обновлен: Лавочник ждет детали, туториал пройден.");
        }
        else if (storyState == MerchantStoryState.PostSubmarineOffer)
        {
            storyState = MerchantStoryState.SearchingForParts;
            // Debug.Log("[MerchantAI] Инструкция прослушана. Переход в режим случайных фраз.");
        }
    }
    
    private IEnumerator CutsceneLookAtSubmarine()
    {
        HideHint();

        // 1. Блокируем управление игроком (ищем PlayerController и выключаем его)
        PlayerController playerCtrl = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerCtrl = playerObj.GetComponent<PlayerController>();
            if (playerCtrl != null) playerCtrl.enabled = false;
        }

        if (submarineTransform != null && mainCamera != null)
        {
            Vector3 originalCamPos = mainCamera.transform.position;
            
            // 2. Находим и отключаем скрипт следования камеры, чтобы он нам не мешал
            CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.enabled = false;

            // 3. Плавно ведем камеру К подлодке
            float elapsed = 0f;
            Vector3 targetCamPos = new Vector3(submarineTransform.position.x, submarineTransform.position.y, originalCamPos.z);
            while (elapsed < 1.5f)
            {
                mainCamera.transform.position = Vector3.Lerp(originalCamPos, targetCamPos, elapsed / 1.5f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            mainCamera.transform.position = targetCamPos;

            // 4. Держим камеру на подлодке
            yield return new WaitForSeconds(submarineViewDuration);

            // 5. Плавно возвращаем камеру назад к игроку/лавочнику
            elapsed = 0f;
            while (elapsed < 1.5f)
            {
                // Если игрок двигался или позиция изменилась, возвращаемся к исходной точке камеры
                mainCamera.transform.position = Vector3.Lerp(targetCamPos, originalCamPos, elapsed / 1.5f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            mainCamera.transform.position = originalCamPos;

            // 6. Включаем следование камеры обратно
            if (camFollow != null) camFollow.enabled = true;
        }

        // 7. Возвращаем управление игроку
        if (playerCtrl != null) playerCtrl.enabled = true;
        
        storyState = MerchantStoryState.LookingAtSubmarine; 
        currentActivePhrases = repairOfferPhrases;
        
        isTalking = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        currentPhraseIndex = 0;
        typingCoroutine = StartCoroutine(TypeText(currentActivePhrases[currentPhraseIndex]));
        
        ShowHint();
    }
    
    private bool CheckPlayerHasAnyParts()
    {
        if (InventoryManager.Instance == null) return false;

        // Проверяем, есть ли хотя бы один предмет из списка деталей в инвентаре
        foreach (string partName in partItemNames)
        {
            if (InventoryManager.Instance.HasItem(partName))
            {
                return true;
            }
        }
        return false; 
    }
    
    private bool CheckPlayerHasAllParts()
    {
        if (InventoryManager.Instance == null) return false;

        // Проверяем, все ли детали из списка собраны
        foreach (string partName in partItemNames)
        {
            if (!InventoryManager.Instance.HasItem(partName))
            {
                return false; // Нашли деталь, которой нет — значит собраны не все
            }
        }
        return true; // Все детали на месте!
    }

    void FixedUpdate()
    {
        if (isTalking || storyState == MerchantStoryState.LookingAtSubmarine)
        {
            rb.linearVelocity = Vector2.zero; 
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
        typingCoroutine = null; // Закончили печать
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