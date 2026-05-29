using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class BoxTutorialManager : MonoBehaviour
{
    [Header("References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    
    [Header("Settings")]
    public float typingSpeed = 0.04f;
    
    private bool isTutorialActive = false;
    private Coroutine typingCoroutine;
    private bool ignoreInputForOneFrame = false;

    void Update()
    {
        if (isTutorialActive)
        {
            if (ignoreInputForOneFrame)
            {
                ignoreInputForOneFrame = false;
                return;
            }

            if (Keyboard.current != null && 
                (Keyboard.current.fKey.wasPressedThisFrame || 
                 Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                CloseTutorial();
            }
        
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                CloseTutorial();
            }
        }
    }

    public void ShowBoxTutorial()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.IsBoxTutorialSeen())
        {
            Debug.Log("BoxTutorial: Уже был показан в этом сохранении.");
            return;
        }
        
        if (dialoguePanel == null)
        {
            Debug.LogError("BoxTutorial: dialoguePanel не назначен!");
            return;
        }
        
        Debug.Log("BoxTutorial: Показываем туториал!");
        
        isTutorialActive = true;
        ignoreInputForOneFrame = true;
        dialoguePanel.SetActive(true);
        
        if (nameText != null)
            nameText.text = "Коробка";
        
        if (dialogueText != null)
        {
            if (typingCoroutine != null) 
                StopCoroutine(typingCoroutine);
            
            string tutorialText = "Коробка подобрана!\n" +
                                 "- Прицелиться: удерживай ПКМ\n" +
                                 "- Бросить: нажми ЛКМ\n" +
                                 "Нажмите F или кликните, чтобы закрыть";
            
            typingCoroutine = StartCoroutine(TypeText(tutorialText));
        }
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = ""; 
        
        foreach (char c in text)
        {
            if (dialogueText != null)
                dialogueText.text += c;
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typingCoroutine = null;
    }

    private void CloseTutorial()
    {
        if (!isTutorialActive) return;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetBoxTutorialSeen(true);
            Debug.Log("Состояние туториала изменено в памяти. Ждем чекпоинт.");
        }
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        isTutorialActive = false;
    }
}