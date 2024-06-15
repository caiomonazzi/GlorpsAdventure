using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton

    public AudioSource backgroundMusic;
    public bool isMusicPlay; // For checking music status

    [Header("AudioClips")] // List of sounds
    public AudioClip aiDamage;
    public AudioClip playerDamage;
    public AudioClip pickUpKey;
    public AudioClip pickUpItems;
    public AudioClip pickUpGear;
    public AudioClip pickUpCoin;
    public AudioClip breakableTile;

    public AudioClip openDoor;
    public AudioClip drinkBottle;
    public AudioClip music;

    // Singleton
    private void Awake()
    {
        if (AudioManager.Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        backgroundMusic = GetComponent<AudioSource>();
        if (backgroundMusic == null)
        {
            Debug.LogError("AudioSource component is missing on AudioManager.");
        }
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null)
        {
            Debug.LogError("Music clip is null.");
            return;
        }

        if (!isMusicPlay) // If the music is not playing
        {
            isMusicPlay = true;
            Play(backgroundMusic, music, true); // Play music
        }
        else
        {
            backgroundMusic.Stop();
            backgroundMusic.clip = music;
            backgroundMusic.Play();
        }
    }

    public void Play(AudioSource audioSource, AudioClip audioClip, bool loop)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null.");
            return;
        }
        if (audioClip == null)
        {
            Debug.LogError("AudioClip is null.");
            return;
        }

        audioSource.clip = audioClip;
        audioSource.loop = loop;
        audioSource.Play();
    }
}
