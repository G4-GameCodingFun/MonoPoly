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
            DontDestroyOnLoad(gameObject); // giữ lại qua các scene
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // huỷ nếu có bản trùng
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

    public void SetMusicEnabled(bool enable)
    {
        // enable = true  ➜ nhạc phát     (mute = false)
        // enable = false ➜ nhạc im lặng (mute = true)
        bgmSource.mute = !enable;

        // Nếu vừa bật lại mà AudioSource không chạy thì Play lại
        if (enable && !bgmSource.isPlaying && bgmSource.clip != null)
        {
            bgmSource.Play();
        }

        Debug.Log($"SetMusicEnabled({enable}) ➜ bgmSource.mute = {!enable}, isPlaying = {bgmSource.isPlaying}");
    }

    public void SetSFXEnabled(bool enable)
    {
        sfxSource.mute = !enable;
        Debug.Log($"SetSFXEnabled({enable}) → sfxSource.mute = {!enable}");

        // TẠM THỜI: mỗi khi bật SFX, phát thử tiếng click để bạn nghe
        if (enable && buttonClick != null)
            sfxSource.PlayOneShot(buttonClick);
    }

    public void MuteMusic(bool mute)
    {
        // mute = true  ➜ enable = false
        // mute = false ➜ enable = true
        SetMusicEnabled(!mute);
    }

}
