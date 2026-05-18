using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuScreen : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel; // Ссылка на PauseMenuPanel

    private bool isPaused = false;

    void Start()
    {
        // При старте игры панель паузы обязательно скрыта
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        // Ловим нажатие Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // 1. Продолжить игру
    public void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f; // Возвращаем нормальное время
        isPaused = false;

        // Прячем курсор обратно (если в игре он не нужен)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Активация паузы
    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f; // Замораживаем физику и Update'ы
        isPaused = true;

        // Показываем курсор, чтобы кликать по кнопкам
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 2. Выйти в главное меню
    public void LoadMainMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f; // ОБЯЗАТЕЛЬНО возвращаем время в 1 перед сменой сцены!
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 3. Выйти из игры
    public void QuitGame()
    {
        Debug.Log("Выход из приложения...");
        Application.Quit();
    }
}