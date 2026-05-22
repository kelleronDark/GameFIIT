using UnityEngine;
using System.Collections;

public class GameSceneMusic : MonoBehaviour
{
    [Header("Editor Testing")]
    public GameObject audioManagerPrefab;

    void Start()
    {
        if (AudioManager.Instance == null && audioManagerPrefab != null)
        {
            Instantiate(audioManagerPrefab);
        }
        
        StartCoroutine(PlayMusicAfterHatch());
    }
    
    private IEnumerator PlayMusicAfterHatch()
    {
        yield return null;
        
        if (AudioManager.Instance != null && AudioManager.Instance.isPlayingHatch)
        {
            // Берем ссылку на источник, который сейчас крутит люк
            AudioSource sfx = AudioManager.Instance.sfxSource;

            if (sfx != null && sfx.isPlaying && sfx.clip == AudioManager.Instance.hatchOpenSound)
            {
                float remainingTime = sfx.clip.length - sfx.time;

                if (remainingTime > 0)
                {
                    yield return new WaitForSeconds(remainingTime + 0.1f);
                }
            }
            
            // В любом случае сбрасываем флаг, так как люк завершен
            AudioManager.Instance.isPlayingHatch = false;
        }
        else
        {
            // Если пришли по кнопке "Продолжить" — пролетаем без задержек
            yield return null;
        }
        
        if (AudioManager.Instance != null && AudioManager.Instance.gameSceneMusic != null)
        {
            float currentFadeDuration = AudioManager.Instance.fadeDuration;
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameSceneMusic, currentFadeDuration);
        }
    }
}