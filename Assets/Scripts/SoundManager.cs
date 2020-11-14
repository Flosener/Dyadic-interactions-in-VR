using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip backgrondSound, doorOpen, doorClose;
    private static AudioSource audioSrc;
    
    void Start()
    {
        backgrondSound = Resources.Load<AudioClip>("HarpMusicSound");
        doorOpen = Resources.Load<AudioClip>("DoorOpensWithDoorknobSound");
        doorClose = Resources.Load<AudioClip>("DoorCloseSound");

        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip)
    {
        switch (clip)
        {
            case "backgroundSound":
                audioSrc.PlayOneShot(backgrondSound);
                break;
            case "doorOpen":
                audioSrc.PlayOneShot(doorOpen);
                break;
            case "doorClose":
                audioSrc.PlayOneShot(doorClose);
                break;
        }
    }
}


// use with SoundManager.PlaySound("blabla");