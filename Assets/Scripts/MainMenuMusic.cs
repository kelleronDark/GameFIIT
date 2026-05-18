using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    void Start()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.mainMenuMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic, 2f);
        }
    }
}