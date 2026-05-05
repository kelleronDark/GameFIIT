// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
//
// public class MainMenu : MonoBehaviour
// {
//     public Button continueButton;
//     
//     void Start()
//     {
//         // Проверяем наличие файла сохранения при запуске меню
//         if (continueButton != null)
//         {
//             // Если файла нет, кнопка становится серой и неактивной
//             continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();
//         }
//     }
//     
//     // Метод для кнопки "Новая игра"
//     public void StartGame()
//     {
//         if (SaveManager.Instance != null)
//         {
//             SaveManager.Instance.DeleteSaveFile(); 
//         }
//         // Загружаем сцену игры (обычно индекс 1)
//         SceneManager.LoadScene(1);
//     }
//
//     // Метод для кнопки "Продолжить"
//     public void ContinueGame()
//     {
//         // Просто грузим сцену. SaveManager сам поймет, что нужно загрузиться, 
//         // так как у него есть метод OnSceneLoaded.
//         SceneManager.LoadScene(1);
//     }
//
//     public void ExitGame()
//     {
//         Debug.Log("Выход из игры..."); 
//         Application.Quit(); 
//     }
// }