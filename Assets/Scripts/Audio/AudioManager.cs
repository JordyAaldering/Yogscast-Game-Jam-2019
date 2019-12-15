#pragma warning disable 0649
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource musicSource;
    public AudioSource chantSource;
    public AudioSource effectSource;
    public AudioSource oreSource;

    private float musicVolume = 0.25f;
    private float effectVolume = 0.5f;
    
    [SerializeField] private float chantFadeSpeed = 1f;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = volume;
    }
    
    public void SetEffectVolume(float volume)
    {
        effectVolume = volume;
        effectSource.volume = volume;
        oreSource.volume = volume;
    }

    public IEnumerator StartChant()
    {
        chantSource.Play();

        while (chantSource.volume < musicVolume)
        {
            float diff = 0.1f * chantFadeSpeed;
            musicSource.volume -= diff;
            chantSource.volume += diff;
            
            yield return null;
        }

        musicSource.volume = 0f;
        chantSource.volume = musicVolume;
    }

    public IEnumerator EndChant()
    {
        while (musicSource.volume < musicVolume)
        {
            float diff = 0.1f * chantFadeSpeed;
            musicSource.volume += diff;
            chantSource.volume -= diff;
            
            yield return null;
        }
        
        chantSource.Stop();

        musicSource.volume = musicVolume;
        chantSource.volume = 0f;
    }
}
