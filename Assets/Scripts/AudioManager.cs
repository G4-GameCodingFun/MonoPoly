using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip buttonClick;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi đổi scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClick != null)
            sfxSource.PlayOneShot(buttonClick);
    }


    public void SetMusicVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void MuteMusic(bool mute)
    {
        bgmSource.mute = mute;
    }
}
