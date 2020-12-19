using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicPlayer;
    public AudioClip[] potentialMusic;
    // Start is called before the first frame update
    void Start()
    {
        musicPlayer.clip = potentialMusic[Random.Range(0, potentialMusic.Length)];
        musicPlayer.Play();   
    }
}
