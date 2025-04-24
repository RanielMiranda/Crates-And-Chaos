using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip MoveBox;
    public AudioClip MovePlayerOne;
    public AudioClip MovePlayerTwo;
    public AudioClip MovePlayerThree;
    public AudioClip MovePlayerFour;
    public AudioClip ButtonSound;
    public AudioClip ResetSound;
    public AudioClip MagnetSound;
    public AudioClip WinSound; 

    private AudioSource audioSource; // Persistent AudioSource

    private int AlternatePlayerStep = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("AudioClip is null!");
            return;
        }

        audioSource.PlayOneShot(audioClip);
    }

    public void PlayMoveBox()
    {
        PlaySound(MoveBox);
    }

    public void PlayMovePlayer()
    {
        if(AlternatePlayerStep == 0) {
            PlaySound(MovePlayerOne);
            AlternatePlayerStep++;
        }
        else if (AlternatePlayerStep == 1) {
            PlaySound(MovePlayerTwo);
            AlternatePlayerStep++;
        }
        else if (AlternatePlayerStep == 2) {
            PlaySound(MovePlayerThree);
            AlternatePlayerStep++;
        }
        else if (AlternatePlayerStep == 3) {
            PlaySound(MovePlayerFour);
            AlternatePlayerStep = 0;
        }
    }

    public void PlayButtonSound()
    {
        PlaySound(ButtonSound);
    }

    public void PlayResetSound()
    {
        PlaySound(ResetSound);
    }

    public void PlayMagnetSound() {
        PlaySound(MagnetSound);
    }

    public void PlayWinSound() {
        PlaySound(WinSound);
    }
}