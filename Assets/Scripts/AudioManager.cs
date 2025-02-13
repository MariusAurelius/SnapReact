using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource AudioSource;
    [SerializeField] private Sound[] SoundEffects;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    void Start()
    {
        AudioSource.loop = false;
    }

    public Sound GetSoundFromString(string soundNameString) {
        if (Enum.TryParse(soundNameString, out SoundName soundName))
        {
           return Array.Find(SoundEffects, soundEffect => soundEffect.Name == soundName);
        }
        else
        {
            Debug.LogWarning($"Sound name '{soundNameString}' does not exist in the SoundName enum.");
            return null;
        }
    }

    public void PlaySFXFromIndex(int index)
    {
        Sound sound = SoundEffects[index];
        AudioSource.clip = sound.Clip;
        AudioSource.volume = sound.Volume;
        AudioSource.pitch = sound.Pitch;
        AudioSource.Play();
    }

    public void PlaySFXFromName(SoundName soundName)
    {
        Sound sound = Array.Find(SoundEffects, soundEffect => soundEffect.Name == soundName);
        AudioSource.clip = sound.Clip;
        AudioSource.volume = sound.Volume;
        AudioSource.pitch = sound.Pitch;
        AudioSource.Play();
    }

    /// <summary>
    /// Plays the soundname in string format.
    /// </summary>
    /// <param name="soundNameString">the sound to play. Must be the string version of a value in the <c>SoundName</c> enum.</param>
    public void PlaySFXFromString(string soundNameString)
    {
        if (Enum.TryParse(soundNameString, out SoundName soundName))
        {
            PlaySFXFromName(soundName);
        }
        else
        {
            Debug.LogWarning($"Sound name '{soundNameString}' does not exist in the SoundName enum.");
        }
    }

    public bool IsPlaying() {
        return AudioSource.isPlaying;
    }
}
