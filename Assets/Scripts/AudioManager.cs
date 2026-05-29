using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Special SFX")]
    public AudioClip hatchOpenSound;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameSceneMusic;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;
    public float fadeDuration = 1.5f;
    
    private Coroutine musicFadeCoroutine;
    public bool isPlayingHatch = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = 0f;
        sfxSource.volume = sfxVolume;
    }

    public void PlayMusic(AudioClip clip, float fadeTime = 2f)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            if (musicSource.volume < musicVolume)
            {
                if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(musicVolume, fadeTime));
            }
            return;
        }

        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        
        if (musicSource.isPlaying)
        {
            musicFadeCoroutine = StartCoroutine(FadeAndSwitchMusic(clip, fadeTime));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();
            musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(musicVolume, fadeTime));
        }
    }
    
    public void StopMusicWithFade(float fadeTime)
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeOutAndStopCoroutine(fadeTime));
    }

    private IEnumerator FadeAndSwitchMusic(AudioClip newClip, float fadeTime)
    {
        float startVolume = musicSource.volume;
        while (musicSource.volume > 0f)
        {
            musicSource.volume -= startVolume / fadeTime * Time.deltaTime;
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.clip = newClip;

        if (newClip != null)
        {
            musicSource.Play();
            while (musicSource.volume < musicVolume)
            {
                musicSource.volume += (musicVolume / fadeTime) * Time.unscaledDeltaTime;
                yield return null;
            }
            musicSource.volume = musicVolume;
        }
    }

    private IEnumerator FadeMusicCoroutine(float targetVolume, float fadeTime)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }
    
    private IEnumerator FadeOutAndStopCoroutine(float fadeTime)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}