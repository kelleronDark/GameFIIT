using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button continueButton;
    
    [Header("Audio Settings")]
    public AudioSource musicSource; // Ссылка на источник музыки
    public AudioSource sfxSource;   // Ссылка на источник звуков (кликов)
    public AudioClip clickSound;    // Сам файл звука клика

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();
        }
    }

    // Универсальный метод для проигрывания звука кнопки
    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    public void StartGame()
    {
        PlayClickSound();
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveFile(); 
        }
        SceneManager.LoadScene(1);
    }

    public void ContinueGame()
    {
        PlayClickSound();
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Выход из игры..."); 
        Application.Quit(); 
    }
}