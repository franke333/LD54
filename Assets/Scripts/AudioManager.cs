using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonClass<AudioManager>
{

    [SerializeField]
    private AudioClip pickUpSound, placeDownSound, walkSound, questAccepted, questFinished, music;

    private AudioSource pickUpSource,placeDownSource,walkSource,questAcceptedSource,questFinishedSource,musicSource;

    public bool Walking;

    public float Volume
    {
        get
        {
            return AudioListener.volume;
        }
        set
        {
            AudioListener.volume = value;
        }
    }

    public void PlayPickUp()
    {
        pickUpSource.clip = pickUpSound;
        pickUpSource.Play();
    }

    public void PlayPlaceDown()
    {
        placeDownSource.clip = placeDownSound;
        placeDownSource.Play();
    }

    private void Update()
    {
        if (Walking)
        {
            if (!walkSource.isPlaying)
            {
                walkSource.clip = walkSound;
                walkSource.Play();
            }
        }
        else
        {
            walkSource.Stop();
        }
    }

    public void PlayQuestAccepted()
    {
        questAcceptedSource.clip = questAccepted;
        questAcceptedSource.Play();
    }

    public void PlayQuestFinished()
    {
        questFinishedSource.clip = questFinished;
        questFinishedSource.Play();
    }

    public void PlayMusic()
    {
        musicSource.clip = music;
        musicSource.Play();
    }

    private void Start()
    {
        pickUpSource = gameObject.AddComponent<AudioSource>();
        placeDownSource = gameObject.AddComponent<AudioSource>();
        walkSource = gameObject.AddComponent<AudioSource>();
        questAcceptedSource = gameObject.AddComponent<AudioSource>();
        questFinishedSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = 0.5f;

        walkSource.volume = 0.05f;
        placeDownSource.volume = 0.3f;
        pickUpSource.volume = 0.3f;

        walkSource.loop = true;

        Volume = 0.5f;

        PlayMusic();
    }

}
