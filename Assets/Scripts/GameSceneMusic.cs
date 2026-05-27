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
            AudioSource sfx = AudioManager.Instance.sfxSource;

            if (sfx != null && sfx.isPlaying && sfx.clip == AudioManager.Instance.hatchOpenSound)
            {
                float remainingTime = sfx.clip.length - sfx.time;

                if (remainingTime > 0)
                {
                    yield return new WaitForSeconds(remainingTime + 0.1f);
                }
            }
            
            AudioManager.Instance.isPlayingHatch = false;
        }
        else
        {
            yield return null;
        }
        
        if (AudioManager.Instance != null && AudioManager.Instance.gameSceneMusic != null)
        {
            float currentFadeDuration = AudioManager.Instance.fadeDuration;
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameSceneMusic, currentFadeDuration);
        }
    }
}