using UnityEngine;

public class GameSceneMusic : MonoBehaviour
{
    void Start()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.gameSceneMusic != null)
        {
            // Плавный переход за 2 секунды
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameSceneMusic, 2f);
        }
    }
}