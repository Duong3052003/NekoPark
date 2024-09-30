using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerPrefs : MonoBehaviour
{
   /* public static MyPlayerPrefs Instance { get; private set; }

    private AudioSource source;
    private AudioSource music;
    [SerializeField] private AudioClip musicClip;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        music = transform.GetChild(0).GetComponent<AudioSource>();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        ChangedVolume(0.5f, "musicVolume", 0, music);
        ChangedVolume(1, "soundVolume", 0, source);
    }

    public void PlaySound(AudioClip _sound)
    {
        source.PlayOneShot(_sound);
    }

    public void ChangedSound(float _changed)
    {
        ChangedVolume(1, "soundVolume", _changed, source);
    }

    public void ChangedMusic(float _changed)
    {
        ChangedVolume(0.5f, "musicVolume", _changed, music);
    }

    private void ChangedVolume(float baseVolume, string volumeName, float changed, AudioSource source)
    {
        float currentVolume = PlayerPrefs.GetFloat(volumeName, 1);
        currentVolume += changed;

        if (currentVolume > 1)
        {
            currentVolume = 0;
        }
        else if (currentVolume < 0)
        {
            currentVolume = 1;
        }

        source.volume = currentVolume * baseVolume;

        PlayerPrefs.SetFloat(volumeName, currentVolume);
    }

    public void ChangedBGM(AudioClip _BGM)
    {
        music.clip = _BGM;
        music.Play();
    }

    public void ReturnBGM()
    {
        music.clip = musicClip;
        music.Play();
    }*/
}