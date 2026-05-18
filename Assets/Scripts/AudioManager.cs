using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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
    public float fadeDuration = 2f; // Длительность плавного перехода
    
    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        // Singleton - чтобы был только один AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать при загрузке сцен
        }
        else
        {
            Destroy(gameObject); // Если уже есть - удаляем дубликат
            return;
        }

        // Создаём AudioSource для музыки, если не назначен
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        // Настройка AudioSource для музыки
        musicSource.loop = true;
        musicSource.volume = 0f; // Начинаем с тишины (плавное появление)
        sfxSource.volume = sfxVolume;
    }

    // Метод для воспроизведения музыки с плавным переходом
    public void PlayMusic(AudioClip clip, float fadeTime = 2f)
    {
        if (clip == null) return;

        // Если этот трек УЖЕ играет, ничего не переключаем, просто проверяем громкость
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            if (musicSource.volume < musicVolume)
            {
                if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(musicVolume, fadeTime));
            }
            return;
        }

        // Если играет другой трек — плавно меняем. Если ничего не играет — просто плавно включаем.
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

    // Плавная смена музыки
    private IEnumerator FadeAndSwitchMusic(AudioClip newClip, float fadeTime)
    {
        // Плавно уменьшаем громкость до 0
        float startVolume = musicSource.volume;
        while (musicSource.volume > 0f)
        {
            musicSource.volume -= startVolume / fadeTime * Time.deltaTime;
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.clip = newClip;

        // Меняем трек
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

    // Плавное изменение громкости
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
    
    // Коruтина затухания и остановки
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

    // Воспроизвести звук (SFX)
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // Установить громкость музыки
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    // Установить громкость звуков
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}