using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public enum MerchantStoryState
{
    NotMet,
    LookingAtSubmarine,
    PostSubmarineOffer,
    SearchingForParts,
    ReadyToRepair
}

public class MerchantAI : MonoBehaviour
{
    public enum Speaker
    {
        Merchant,
        Hero
    }
    
    [System.Serializable]
    public struct DialogueLine
    {
        public Speaker speaker;
        [TextArea(2, 4)]
        public string text;
    }
    
    [Header("Настройки движения")]
    public float speed = 2f;
    public float walkDistance = 3f;

    [Header("Настройки диалога")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.04f;
    public AudioClip typeSound;
    
    [Header("Настройки Сюжета & Камеры")]
    public Transform submarineTransform;
    public float cameraPanSpeed = 3f;
    public float submarineViewDuration = 2.5f;
    private bool isExternalMovementLock = false;
    [Header("Финальный Маркер")]
    public Transform finalPlayerMarker;
    
    [Header("Настройки Инвентаря")]
    [Tooltip("Имена спрайтов деталей в инвентаре, которые нужно собрать")]
    public string[] partItemNames = new string[] { "Part1", "Part2", "Part3", "Part4" };

    [Header("Диалоговые реплики")]
    public DialogueLine[] introPhrases;
    public DialogueLine[] repairOfferPhrases;
    public DialogueLine[] tutorialPhrases;
    public DialogueLine[] idleNoPartsPhrases;
    public DialogueLine[] idleWithPartsPhrases;
    public DialogueLine[] finalPhrases;
    
    private MerchantStoryState storyState = MerchantStoryState.NotMet;
    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool movingRight = true;
    private bool isTalking = false;
    private bool isPlayerNearby = false;
    private Coroutine typingCoroutine;
    private int currentPhraseIndex = 0;
    
    private DialogueLine[] currentActivePhrases;

    private AudioSource audioSource;
    private GameObject currentHint;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        mainCamera = Camera.main;
        
        if (SaveManager.Instance != null)
        {
            storyState = (MerchantStoryState)SaveManager.Instance.GetMerchantState();
        }

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (typeSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.3f;
        }
    }

    void Update()
    {
        if (storyState == MerchantStoryState.LookingAtSubmarine && !isTalking)
        {
            return; 
        }
        
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }
    
    private void HandleInteraction()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            
            dialogueText.text = currentActivePhrases[currentPhraseIndex].text;
            return;
        }

        if (!isTalking)
        {
            StartDialogueBranch();
        }
        else
        {
            currentPhraseIndex++;
            if (currentPhraseIndex < currentActivePhrases.Length)
            {
                SetupDialogueLine(currentActivePhrases[currentPhraseIndex]);
            }
            else
            {
                EndDialogueBranch();
            }
        }
    }
    
    private void StartDialogueBranch()
    {
        isTalking = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        currentPhraseIndex = 0;
        
        if (CheckPlayerHasAllParts() && storyState != MerchantStoryState.NotMet && storyState != MerchantStoryState.LookingAtSubmarine)
        {
            storyState = MerchantStoryState.ReadyToRepair;
            SaveFinalProgress();
            currentActivePhrases = finalPhrases;
        }
        else if (storyState == MerchantStoryState.NotMet)
        {
            currentActivePhrases = introPhrases;
        }
        else if (storyState == MerchantStoryState.PostSubmarineOffer)
        {
            currentActivePhrases = tutorialPhrases;
        }
        else if (storyState == MerchantStoryState.SearchingForParts 
                 || storyState == MerchantStoryState.LookingAtSubmarine)
        {
            if (storyState == MerchantStoryState.LookingAtSubmarine && CheckPlayerHasAllParts())
            {
                storyState = MerchantStoryState.ReadyToRepair;
                SaveFinalProgress();
                currentActivePhrases = finalPhrases;
            }
            else if (CheckPlayerHasAnyParts())
            {
                currentActivePhrases = idleWithPartsPhrases;
            }
            else
            {
                if (idleNoPartsPhrases != null && idleNoPartsPhrases.Length > 0)
                {
                    int randomIndex = Random.Range(0, idleNoPartsPhrases.Length);
                    currentActivePhrases = new DialogueLine[] { idleNoPartsPhrases[randomIndex] };
                }
                else
                {
                    currentActivePhrases = new DialogueLine[] 
                    { 
                        new DialogueLine { speaker = Speaker.Merchant, text = "Эх, скукотища..." } 
                    };
                }
            }
        }
        else if (storyState == MerchantStoryState.ReadyToRepair)
        {
            currentActivePhrases = finalPhrases;
        }

        if (currentActivePhrases != null && currentActivePhrases.Length > 0)
        {
            SetupDialogueLine(currentActivePhrases[currentPhraseIndex]);
        }
    }
    
    private void EndDialogueBranch()
    {
        isTalking = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (storyState == MerchantStoryState.NotMet)
        {
            storyState = MerchantStoryState.LookingAtSubmarine;
            StartCoroutine(CutsceneLookAtSubmarine());
        }
        else if (storyState == MerchantStoryState.LookingAtSubmarine)
        {
            storyState = MerchantStoryState.PostSubmarineOffer;
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMerchantState((int)storyState);
                SaveManager.Instance.QuickSave();
            }
            
            Debug.Log("[MerchantAI] Сюжет обновлен: Лавочник ждет детали, туториал пройден.");
        }
        else if (storyState == MerchantStoryState.PostSubmarineOffer)
        {
            storyState = MerchantStoryState.SearchingForParts;
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMerchantState((int)storyState);
                SaveManager.Instance.QuickSave();
            }
        }
        else if (storyState == MerchantStoryState.ReadyToRepair)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }
    
    private void UpdatePlayerCheckpointToMarker()
    {
        if (finalPlayerMarker != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerController playerCtrl = playerObj.GetComponent<PlayerController>();
                if (playerCtrl != null)
                {
                    playerCtrl.SetCheckpoint(finalPlayerMarker.position);
                    Debug.Log($"Чекпоинт игрока принудительно перезаписан на координаты маркера: {finalPlayerMarker.position}");
                }
            }
        }
        else
        {
            Debug.LogError("[MerchantAI] Поле finalPlayerMarker пустое!");
        }
    }
    
    private void SaveFinalProgress()
    {
        UpdatePlayerCheckpointToMarker();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetMerchantState((int)storyState);
            SaveManager.Instance.playFinalCutsceneNext = true;
            SaveManager.Instance.QuickSave();
            Debug.Log("[MerchantAI] Финальный прогресс и маркер успешно засейвлены.");
        }
    }
    
    private void SetupDialogueLine(DialogueLine line)
    {
        if (line.speaker == Speaker.Hero)
        {
            if (nameText != null) nameText.text = "Герой";
        }
        else
        {
            if (nameText != null) nameText.text = "Лавочник";
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }
    
    private IEnumerator CutsceneLookAtSubmarine()
    {
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
            
            CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.enabled = false;

            float elapsed = 0f;
            Vector3 targetCamPos = new Vector3(submarineTransform.position.x, submarineTransform.position.y, originalCamPos.z);
            while (elapsed < 1.5f)
            {
                mainCamera.transform.position = Vector3.Lerp(originalCamPos, targetCamPos, elapsed / 1.5f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            mainCamera.transform.position = targetCamPos;

            yield return new WaitForSeconds(submarineViewDuration);

            elapsed = 0f;
            while (elapsed < 1.5f)
            {
                mainCamera.transform.position = Vector3.Lerp(targetCamPos, originalCamPos, elapsed / 1.5f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            mainCamera.transform.position = originalCamPos;

            if (camFollow != null) camFollow.enabled = true;
        }

        if (playerCtrl != null) playerCtrl.enabled = true;
        
        storyState = MerchantStoryState.LookingAtSubmarine; 
        currentActivePhrases = repairOfferPhrases;
        
        isTalking = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        currentPhraseIndex = 0;
        
        SetupDialogueLine(currentActivePhrases[currentPhraseIndex]);
    }
    
    private bool CheckPlayerHasAnyParts()
    {
        if (InventoryManager.Instance == null) return false;

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

        foreach (string partName in partItemNames)
        {
            if (!InventoryManager.Instance.HasItem(partName))
            {
                return false;
            }
        }
        return true;
    }

    void FixedUpdate()
    {
        if (isTalking || storyState == MerchantStoryState.LookingAtSubmarine || isExternalMovementLock)
        {
            rb.linearVelocity = Vector2.zero; 
            return;
        }

        float leftBoundary = startPos.x - walkDistance;
        float rightBoundary = startPos.x + walkDistance;

        if (movingRight && transform.position.x >= rightBoundary) 
            movingRight = false;
        else if (!movingRight && transform.position.x <= leftBoundary) 
            movingRight = true;

        float direction = movingRight ? 1 : -1;
        Vector2 nextPos = rb.position + new Vector2(direction * speed * Time.fixedDeltaTime, 0);
        rb.MovePosition(nextPos);

        float scaleX = Mathf.Abs(transform.localScale.x) * direction;
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

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
        typingCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = false;
            isTalking = false;
            if (dialoguePanel != null) 
                dialoguePanel.SetActive(false);
            if (typingCoroutine != null) 
                StopCoroutine(typingCoroutine);
        }
    }
    
    public MerchantStoryState GetStoryState() => storyState;
    public void SetStoryState(MerchantStoryState newState) => storyState = newState;
    
    public void SetMovementLocked(bool locked)
    {
        isExternalMovementLock = locked;
    }
}