using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private AudioSource source;
    private AudioSource music;

    [SerializeField] private GameObject sourceObj;
    [SerializeField] private GameObject musicObj;

    //[SerializeField] private AudioClip musicClip;

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

        ChangedVolume(0.5f, "musicVolume", PlayerPrefs.GetFloat("musicVolume", 1), music);
        ChangedVolume(1, "soundVolume", PlayerPrefs.GetFloat("soundVolume", 1), source);
    }

    public void PlaySound(AudioClip _sound)
    {
        if (_sound == null) return;
        source.PlayOneShot(_sound);
    }

    public void ChangedSound()
    {
        ChangedVolume(1, "soundVolume", sourceObj.GetComponent<Slider>().value, source);
    }

    public void ChangedMusic()
    {
        ChangedVolume(0.5f, "musicVolume", musicObj.GetComponent<Slider>().value, music);
    }

    private void ChangedVolume(float baseVolume, string volumeName, float changed, AudioSource source)
    {
        float currentVolume = PlayerPrefs.GetFloat(volumeName, 1);
        currentVolume = changed;

        /*if (currentVolume > 1)
        {
            currentVolume = 0;
        }
        else if (currentVolume < 0)
        {
            currentVolume = 1;
        }*/

        source.volume = currentVolume * baseVolume;

        PlayerPrefs.SetFloat(volumeName, currentVolume);
    }

    public void ChangedBGM(AudioClip _BGM)
    {
        music.clip = _BGM;
        music.Play();
    }

    /*public void ReturnBGM()
    {
        music.clip = musicClip;
        music.Play();
    }*/
}