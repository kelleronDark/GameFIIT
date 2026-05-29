using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    private const int MAIN_MENU_INDEX = 0;
    private const int CUTSCENES_INDEX = 1;
    private const int GAME_LEVEL_INDEX = 2;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeAutomatic()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("Automatic_CursorManager");
            Instance = go.AddComponent<CursorManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == MAIN_MENU_INDEX)
        {
            ShowCursor(true);
        }
        else
        {
            ShowCursor(false);
        }
    }

    public void ShowCursor(bool show)
    {
        if (show)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 
            Debug.Log("<color=lime>[CursorManager] Курсор ВКЛЮЧЕН</color>");
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            Debug.Log("<color=red>[CursorManager] Курсор СПРЯТАН</color>");
        }
    }
}