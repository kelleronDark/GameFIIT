using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DeathScreen : MonoBehaviour
{
    [Header("References")]
    public GameObject deathPanel;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI hintText;
    public AudioSource deathSound;

    [Header("Settings")]
    public string fullMessage = "Вы погибли в сражении за запчасти в самом таинственном городе на Земле..";
    public string hintMessage = "Нажмите F, чтобы возродиться";
    public float typeSpeed = 0.05f;
    public float fadeDuration = 1.5f;
    
    private bool isDead = false;
    private bool isTyping = false;
    private bool canRestart = false;
    
    private Coroutine sequenceCoroutine;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);
        
        if (canvasGroup == null && deathPanel != null)
            canvasGroup = deathPanel.GetComponent<CanvasGroup>();

        if (mainText != null)
            mainText.text = "";
        
        if (hintText != null)
        {
            hintText.text = "";
            hintText.gameObject.SetActive(true);
        }
    }

    public void ShowDeathScreen()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("Показываем экран смерти");

        if (deathPanel != null)
            deathPanel.SetActive(true);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusicWithFade(fadeDuration);
        }

        if (deathSound != null)
            deathSound.Play();

        Time.timeScale = 0f;

        sequenceCoroutine = StartCoroutine(ShowDeathSequence());
    }

    private IEnumerator ShowDeathSequence()
    {
        yield return StartCoroutine(FadeIn());

        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(mainText, fullMessage));
        isTyping = false;

        yield return new WaitForSecondsRealtime(0.3f);
        typingCoroutine = StartCoroutine(TypeText(hintText, hintMessage));
        yield return typingCoroutine;
        
        isTyping = false;
        canRestart = true;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent, string message)
    {
        if (textComponent == null) yield break;

        textComponent.text = "";

        foreach (char letter in message.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
    }

    void Update()
    {
        if (!isDead) return;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else if (canRestart)
            {
                RestartGame();
            }
        }
    }
    
    private void SkipTyping()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (mainText != null) mainText.text = fullMessage;
        if (hintText != null) hintText.text = hintMessage;

        if (canvasGroup != null) canvasGroup.alpha = 1f;

        isTyping = false;
        canRestart = true;
    }

    public void RestartGame()
    {
        Debug.Log("Перезагружаем сцену");

        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}