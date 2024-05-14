using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
    public AudioSource _audioSource;
    public AudioClip _clickSound;
    public AudioClip _catSound;
    public AudioClip _bgmMusic;
    void Awake()
    {
        if( _audioSource == null)
        {
            _audioSource = this.GetComponent<AudioSource>();
        }
    }

    
   public void PlayBgMusic()
    {
        _audioSource.clip = _bgmMusic;
        _audioSource.loop = true;
        _audioSource.volume = 0.5f;
        _audioSource.Play();
    }

    public void PlayButtonSound()
    {
        _audioSource.PlayOneShot(_clickSound);
    }

    public void PlayCatSound()
    {
        _audioSource.PlayOneShot(_catSound);
        _audioSource.volume = 0.1f;
    }
}
