using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] destroyNoice;
    public void playRandomDestroyNoise()
    {
        //choose a random number
        int clipToPlay = Random.Range(0, destroyNoice.Length);
        //play that clip
        destroyNoice[clipToPlay].Play();
        
        
    }
}
