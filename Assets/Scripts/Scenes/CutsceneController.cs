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
    public int introNextSceneIndex = 2;

    [Header("Настройки Финальной Катсцены")]
    public VideoClip finalVideoClip;
    public int finalNextSceneIndex = 0;
    
    [Header("Delay Settings")]
    [SerializeField] private float delayBeforeLoad = 1.3f;

    private bool isTransitioning = false;
    private int sceneToLoadIndex;
    private bool isFinalCutscene = false;
    
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
        
        if (videoPlayer.isPlaying) videoPlayer.Stop(); 

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

        asyncLoad.allowSceneActivation = true;
    }
}