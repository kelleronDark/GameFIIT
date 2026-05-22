using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button continueButton;
    
    [Header("Audio Settings")]
    public AudioSource sfxSource;   // Ссылка на источник звуков (кликов)
    public AudioClip clickSound;    // Сам файл звука клика

    void Start()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.mainMenuMusic != null)
        {
            float currentFadeDuration = AudioManager.Instance.fadeDuration;
            AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic, currentFadeDuration);
        }
        
        if (continueButton != null)
        {
            continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();
        }
    }

    // Универсальный метод для проигрывания звука кнопки
    public void PlayClickSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null && clickSound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(clickSound);
        }
    }

    public void StartGame()
    {
        PlayClickSound();
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusicWithFade(0.5f);
        }
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveFile(); 
        }
        SceneManager.LoadScene(1);
    }

    public void ContinueGame()
    {
        PlayClickSound();
        
        StartCoroutine(FadeAndContinue());
    }
    
    private System.Collections.IEnumerator FadeAndContinue()
    {
        if (continueButton != null) continueButton.interactable = false; // Защита от спама кнопкой

        if (AudioManager.Instance != null)
        {
            // Плавно гасим музыку меню за 0.6 секунды
            AudioManager.Instance.StopMusicWithFade(0.6f);
        }

        // Ждем, пока музыка затихнет
        yield return new WaitForSecondsRealtime(0.6f);

        // Загружаем сцену самой игры
        SceneManager.LoadScene(2);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Выход из игры..."); 
        Application.Quit(); 
    }
}