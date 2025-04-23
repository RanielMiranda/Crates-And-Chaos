using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip MoveBox;
    public AudioClip MovePlayer;
    public AudioClip ButtonSound;
    public AudioClip ResetSound;
    public AudioClip MagnetSound;
    public AudioClip WinSound;

    private AudioSource audioSource; // Persistent AudioSource

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
        PlaySound(MovePlayer);
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