using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static public SoundManager SM { set; get; }



    private void Awake()
    {
        SM = this;
    }

    public void PlayTheSound(AudioClip AC, float Volume)
    {
  
            for (int i = 0; i < GetComponents<AudioSource>().Length; i++)
            {
                if (!GetComponents<AudioSource>()[i].isPlaying)
                {
                    GetComponents<AudioSource>()[i].clip = AC;
                    GetComponents<AudioSource>()[i].Play();
                    GetComponents<AudioSource>()[i].volume = Volume;
                    break;
                }
            }
        
    }
}
