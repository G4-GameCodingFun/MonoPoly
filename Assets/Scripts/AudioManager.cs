using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip buttonClick;

    [Header("Dice & Movement")]
    public AudioClip diceRoll;
    public AudioClip stepTile;
    public AudioClip tileLand;

    [Header("Transaction & Property")]
    public AudioClip buyProperty;
    public AudioClip buildHouse;
    public AudioClip buildHotel;
    public AudioClip payRent;
    public AudioClip receiveMoney;
    public AudioClip errorMoney;
    public AudioClip mortgage;

    [Header("Special Tiles & Events")]
    public AudioClip goToJail;
    public AudioClip getOutOfJail;
    public AudioClip cardDraw;

    [Header("System & Notifications")]
    public AudioClip turnStart;
    public AudioClip bankrupt;
    public AudioClip winGame;
    public AudioClip loseGame;

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
    public void PlayDiceRoll() => PlaySFX(diceRoll);
    public void PlayStepTile() => PlaySFX(stepTile);
    public void PlayTileLand() => PlaySFX(tileLand);

    public void PlayBuyProperty() => PlaySFX(buyProperty);
    public void PlayBuildHouse() => PlaySFX(buildHouse);
    public void PlayBuildHotel() => PlaySFX(buildHotel);
    public void PlayPayRent() => PlaySFX(payRent);
    public void PlayReceiveMoney() => PlaySFX(receiveMoney);
    public void PlayErrorMoney() => PlaySFX(errorMoney);
    public void PlayMortgage() => PlaySFX(mortgage);

    public void PlayGoToJail() => PlaySFX(goToJail);
    public void PlayGetOutOfJail() => PlaySFX(getOutOfJail);
    public void PlayCardDraw() => PlaySFX(cardDraw);

    public void PlayTurnStart() => PlaySFX(turnStart);
    public void PlayBankrupt() => PlaySFX(bankrupt);
    public void PlayWinGame() => PlaySFX(winGame);
    public void PlayLoseGame() => PlaySFX(loseGame);

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
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
