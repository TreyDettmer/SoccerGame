using UnityEngine;
using System;
public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    public Sound[] sounds;


    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.playOnAwake = false;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        DontDestroyOnLoad(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            return;
        }
        s.source.Play();
    }

    public void StopAllSounds()
    {
        foreach(Sound s in sounds)
        {
            s.source.Stop();
        }
    }
}
