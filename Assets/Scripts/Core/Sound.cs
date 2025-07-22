﻿using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume;

    public float pitch;
    public bool loop;

    [Range(0f, 1f)] public float spatialBlend;

    public bool isBGM = false;

    [HideInInspector] public AudioSource source;
}