using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button continueButton;
    
    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip clickSound;

    void Start()
    {
        Time.timeScale = 1f;
        
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
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void ContinueGame()
    {
        PlayClickSound();
        
        StartCoroutine(FadeAndContinue());
    }
    
    private System.Collections.IEnumerator FadeAndContinue()
    {
        if (continueButton != null) continueButton.interactable = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusicWithFade(0.6f);
        }

        yield return new WaitForSecondsRealtime(0.6f);
        
        Time.timeScale = 1f;

        SceneManager.LoadScene(2);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Выход из игры..."); 
        Application.Quit(); 
    }
}