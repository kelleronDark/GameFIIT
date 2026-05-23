using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CutsceneController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    
    [Header("Настройки Начальной Катсцены")]
    public VideoClip introVideoClip;
    public int introNextSceneIndex = 2; // Индекс сцены самой игры

    [Header("Настройки Финальной Катсцены")]
    public VideoClip finalVideoClip;
    public int finalNextSceneIndex = 0; // Индекс Главного меню
    
    [Header("Delay Settings")]
    [SerializeField] private float delayBeforeLoad = 1.3f; // <--- ЗАДЕРЖКА В СЕКУНДАХ (настрой в инспекторе)

    private bool isTransitioning = false; // Защита от двойного срабатывания кнопки пропуска
    private int sceneToLoadIndex;
    private bool isFinalCutscene = false; // Флаг, чтобы разделять логику переходов
    
    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusicWithFade(0.5f);
        }
        
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
        
        if (SaveManager.Instance != null && SaveManager.Instance.playFinalCutsceneNext)
        {
            isFinalCutscene = true;
            if (finalVideoClip != null) videoPlayer.clip = finalVideoClip;
            sceneToLoadIndex = finalNextSceneIndex;
        }
        else
        {
            isFinalCutscene = false;
            if (introVideoClip != null) videoPlayer.clip = introVideoClip;
            sceneToLoadIndex = introNextSceneIndex;
        }
        
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void Update()
    {
        if (isTransitioning) return;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                StartTransition();
            }
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartTransition();
    }
    
    void StartTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        videoPlayer.loopPointReached -= OnVideoFinished;
        
        // Останавливаем видео, чтобы картинка замерла или пропала в черноту
        if (videoPlayer.isPlaying) videoPlayer.Stop(); 

        // Запускаем корутину задержки
        StartCoroutine(WaitAndLoadRoutine());
    }

    private IEnumerator WaitAndLoadRoutine()
    {
        if (!isFinalCutscene)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.hatchOpenSound != null)
            {
                AudioManager.Instance.isPlayingHatch = true; 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.hatchOpenSound, 0.5f); 
            }
        }
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoadIndex);
        asyncLoad.allowSceneActivation = false;
        
        float elapsed = 0f;
        while (elapsed < delayBeforeLoad)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (isFinalCutscene && SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveFile();
        }

        asyncLoad.allowSceneActivation = true;
    }
}