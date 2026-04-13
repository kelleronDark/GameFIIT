using UnityEngine;
using UnityEngine.SceneManagement; // Нужно для переключения уровней

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Загружает следующую сцену в списке (вашу игру)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры..."); // Это сработает в редакторе
        Application.Quit(); // Это закроет готовую программу (.exe)
    }
}