using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuScreen : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursor(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursor(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void LoadMainMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f;
        StartCoroutine(WaitAndLoadMenu(mainMenuSceneName));
    }
    
    private System.Collections.IEnumerator WaitAndLoadMenu(string sceneName)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusicWithFade(0.3f);
        }

        yield return new WaitForSecondsRealtime(0.3f);

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Выход из приложения");
        Application.Quit();
    }
}