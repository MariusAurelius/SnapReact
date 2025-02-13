using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound {

    /// <summary>
    /// The sound's audioclip file.
    /// </summary>
    public AudioClip Clip;
    public SoundName Name;

    /// <summary>
    /// The volume of the sound.
    /// </summary> 
    [Range(0f, 1f)]
    public float Volume;

    /// <summary>
    /// The frequency of the sound.
    /// </summary>
    [Range(.1f, 3f)]
    public float Pitch;

    Sound() {
        Volume = 1;
        Pitch = 1;
    }
}

public enum SoundName
{
    Left,
    Right,
    Up,
    Down,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight,
    Red,
    Green,
    Orange,
    Blue,
    Yellow,
    Pink,
    Number1,
    Number2,
    Number3,
    Number4,
    Number5,
    Number6,
    Number7,
    Number8,
    Number9,
    LetterA,
    LetterB,
    LetterC,
    LetterD,
    LetterE,
    LetterF,
    LetterG,
    LetterH,
    Beep,
    Monkey
}