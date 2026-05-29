using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine;

public enum BoothKeeperStoryState
{
    NotMet,
    AlreadyTalked
}

public class BoothmanAI : MonoBehaviour
{
    public enum Speaker
    {
        BoothKeeper,
        Merchant
    }

    [System.Serializable]
    public struct DialogueLine
    {
        public Speaker speaker;       
        [TextArea(2, 4)]
        public string text;           
    }
    
    [Header("Настройки диалога")]
    public GameObject dialoguePanel;       
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;   
    public float typingSpeed = 0.04f;      
    public AudioClip typeSound;            

    [Header("Ссылки на Лавочника и Камеру")]
    public Transform merchantTransform;   
    public float cameraPanSpeed = 3f;      
    public float merchantViewDuration = 3f;

    [Header("Реплики Лавочника (Первая встреча)")]
    public DialogueLine[] merchantCommentPhrases;

    [Header("UI Hint")]
    public GameObject hintPrefab;          
    
    private BoothKeeperStoryState storyState = BoothKeeperStoryState.NotMet;
    private bool isTalking = false;
    private bool isPlayerNearby = false;
    private Coroutine typingCoroutine;
    private int currentPhraseIndex = 0;
    private bool isCutsceneActive = false;
    
    private DialogueLine[] currentActivePhrases;
    private AudioSource audioSource;
    private GameObject currentHint;
    private Camera mainCamera;
    
    private MerchantAI merchantAI;
    
    void Start()
    {
        mainCamera = Camera.main;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        if (typeSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.3f;
        }
        
        if (merchantTransform != null)
        {
            merchantAI = merchantTransform.GetComponent<MerchantAI>();
        }

        if (SaveManager.Instance != null)
        {
            storyState = (BoothKeeperStoryState)SaveManager.Instance.GetBoothmanState();
        }
    }
    
    void Update()
    {
        if (isCutsceneActive) return;

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
            
            if (currentActivePhrases != null && currentPhraseIndex < currentActivePhrases.Length)
            {
                dialogueText.text = currentActivePhrases[currentPhraseIndex].text;
            }
            return;
        }

        if (!isTalking)
        {
            StartDialogueBranch();
        }
        else
        {
            currentPhraseIndex++;
            if (currentActivePhrases != null && currentPhraseIndex < currentActivePhrases.Length)
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

        if (storyState == BoothKeeperStoryState.NotMet)
        {
            DialogueLine silentLine = new DialogueLine { speaker = Speaker.BoothKeeper, text = "..." };
            currentActivePhrases = new DialogueLine[] { silentLine };
        }
        else
        {
            DialogueLine silentLine = new DialogueLine { speaker = Speaker.BoothKeeper, text = "..." };
            currentActivePhrases = new DialogueLine[] { silentLine };
        }

        if (currentActivePhrases != null && currentActivePhrases.Length > 0)
        {
            SetupDialogueLine(currentActivePhrases[currentPhraseIndex]);
        }
    }

    private void EndDialogueBranch()
    {
        if (storyState == BoothKeeperStoryState.NotMet && currentActivePhrases == merchantCommentPhrases)
        {
            isTalking = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            storyState = BoothKeeperStoryState.AlreadyTalked;
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetBoothmanState((int)storyState);
                SaveManager.Instance.QuickSave();
            }
            
            return;
        }

        if (storyState == BoothKeeperStoryState.NotMet)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            StartCoroutine(CutsceneLookAtMerchant());
        }
        else
        {
            isTalking = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }
    }

    private void SetupDialogueLine(DialogueLine line)
    {
        if (line.speaker == Speaker.Merchant)
        {
            if (nameText != null) nameText.text = "Лавочник";
        }
        else
        {
            if (nameText != null) nameText.text = "Будочник";
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator CutsceneLookAtMerchant()
    {
        HideHint();
        isCutsceneActive = true;
        
        if (merchantAI != null)
        {
            merchantAI.SetMovementLocked(true);
        }

        PlayerController playerCtrl = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerCtrl = playerObj.GetComponent<PlayerController>();
            if (playerCtrl != null) playerCtrl.enabled = false;
        }

        if (merchantTransform != null && mainCamera != null)
        {
            Vector3 originalCamPos = mainCamera.transform.position;
            CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.enabled = false;

            float elapsed = 0f;
            Vector3 targetCamPos = new Vector3(merchantTransform.position.x, merchantTransform.position.y, originalCamPos.z);
            while (elapsed < 1.5f)
            {
                mainCamera.transform.position = Vector3.Lerp(originalCamPos, targetCamPos, elapsed / 1.5f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            mainCamera.transform.position = targetCamPos;
            
            isCutsceneActive = false;
            
            currentActivePhrases = merchantCommentPhrases;
            currentPhraseIndex = 0;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            SetupDialogueLine(currentActivePhrases[currentPhraseIndex]);

            while (storyState == BoothKeeperStoryState.NotMet)
            {
                yield return null;
            }
            
            isCutsceneActive = true;

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
        
        if (merchantAI != null)
        {
            merchantAI.SetMovementLocked(false);
        }
        
        isCutsceneActive = false;
        ShowHint();
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
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        }
    }

    void ShowHint()
    {
        if (currentHint != null) return;
        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        TextMeshProUGUI hintText = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (hintText != null) hintText.text = "Нажмите F";

        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null && Camera.main != null) canvas.worldCamera = Camera.main;
    }

    void HideHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
    }
    
    public BoothKeeperStoryState GetStoryState() => storyState;
    public void SetStoryState(BoothKeeperStoryState newState) => storyState = newState;
}
