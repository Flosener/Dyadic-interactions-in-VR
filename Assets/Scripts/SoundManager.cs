using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static AudioClip _backgroundSound, _doorOpen;
    private static AudioSource _audioSrc;
    
    private void Start()
    {    
        // Get audio source from the SoundManager and necessary sounds.
        _audioSrc = GetComponent<AudioSource>();
        _backgroundSound = Resources.Load<AudioClip>("HarpMusicSound");
        _doorOpen = Resources.Load<AudioClip>("DoorOpensWithDoorknobSound");
    }

    // Use with SoundManager.PlaySound("CLIP NAME");
    public static void PlaySound(string clip)
    {
        switch (clip)
        {
            case "backgroundSound":
                _audioSrc.PlayOneShot(_backgroundSound);
                break;
            case "doorOpen":
                _audioSrc.PlayOneShot(_doorOpen);
                break;
        }
    }
}