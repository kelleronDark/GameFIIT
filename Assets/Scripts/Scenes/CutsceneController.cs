using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CutsceneController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public int nextSceneIndex = 2; // Индекс сцены самой игры
    
    [Header("Delay Settings")]
    [SerializeField] private float delayBeforeLoad = 1.3f; // <--- ЗАДЕРЖКА В СЕКУНДАХ (настрой в инспекторе)

    private bool isTransitioning = false; // Защита от двойного срабатывания кнопки пропуска

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

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
        if (AudioManager.Instance != null && AudioManager.Instance.hatchOpenSound != null)
        {
            AudioManager.Instance.isPlayingHatch = true; 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.hatchOpenSound, 0.5f); // Твоя громкость 50%
        }
        
        yield return new WaitForSeconds(delayBeforeLoad);

        // 3. И только после этого Unity начинает грузить уровень
        SceneManager.LoadScene(nextSceneIndex);
    }
}