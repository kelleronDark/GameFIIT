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
    public TextMeshProUGUI hintText; // Теперь будем печатать и его
    public AudioSource deathSound;

    [Header("Settings")]
    public string fullMessage = "Вы погибли в сражении за запчасти в самом таинственном городе на Земле..";
    public string hintMessage = "Нажмите F, чтобы возродиться"; // <-- Новое поле
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
            hintText.text = ""; // Очищаем изначально
            hintText.gameObject.SetActive(true); // Оставляем активным, но пустым
        }
    }

    public void ShowDeathScreen()
    {
        if (isDead) return; // Если уже мертвы — ничего не делаем
        isDead = true;
        
        Debug.Log("Показываем экран смерти...");

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
        // 1. Плавное затемнение
        yield return StartCoroutine(FadeIn());

        // 2. Печатаем главный текст
        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(mainText, fullMessage));
        isTyping = false;

        // 3. Печатаем подсказку
        yield return new WaitForSecondsRealtime(0.3f); // Небольшая пауза для эффекта
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

    // Универсальный метод печати для любого TextMeshProUGUI
    private IEnumerator TypeText(TextMeshProUGUI textComponent, string message)
    {
        if (textComponent == null) yield break;

        textComponent.text = ""; // Очищаем

        foreach (char letter in message.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
    }

    void Update()
    {
        if (!isDead) return;

        // Если игрок нажал F
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            // Скип во время печати текста
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
        // Насильно останавливаем корутины вывода текста
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // Мгновенно заполняем весь текст
        if (mainText != null) mainText.text = fullMessage;
        if (hintText != null) hintText.text = hintMessage;

        // Насильно выставляем альфу панели на максимум (на случай, если скипнули во время фейда)
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        isTyping = false;
        canRestart = true; // Теперь можно рестартить следующим нажатием
    }

    public void RestartGame()
    {
        Debug.Log("Перезагружаем сцену...");

        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}