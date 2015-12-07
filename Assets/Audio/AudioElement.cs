using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioElement
{
    private GameObject element;
    private Dictionary<AudioClip, GameObject> soundObjects = new Dictionary<AudioClip, GameObject>();
    private AudioSource _audio;

    public AudioElement(List<AudioClip> sounds, List<float> volumes, string id, Transform parentTransform)
    {
        if (sounds == null || sounds.Count == 0 || volumes == null || volumes.Count == 0 || sounds.Count != volumes.Count)
        {
            return;
        }

        element = new GameObject("AudioElement_" + id);
        if (parentTransform)
        {
            element.transform.parent = parentTransform;
        }
        else
        {
            //attach it to the game object list (since we know there should be one present)
            //do so to keep the inspector cleaner - this saves making a sounds object
            var list = Object.FindObjectOfType(typeof(GameObjectList)) as GameObjectList;
            if (list)
            {
                element.transform.parent = list.transform;
            }
        }
        Add(sounds, volumes);
    }

    public void Add(List<AudioClip> sounds, List<float> volumes)
    {
        for (var i = 0; i < sounds.Count; i++)
        {
            var sound = sounds[i];
            if (!sound)
            {
                continue;
            }
            var temp = new GameObject(sound.name);
            temp.AddComponent(typeof(AudioSource));
            _audio = temp.GetComponent<AudioSource>();
            if (_audio == null)
            {
                Debug.LogError("Audio padło");
                return;
            }

            _audio.clip = sound;
            _audio.volume = volumes[i];
            temp.transform.parent = element.transform;
            soundObjects.Add(sound, temp);
        }
    }

    public void Play(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            _audio = temp.GetComponent<AudioSource>();
            if (_audio != null && !_audio.isPlaying)
            {
                _audio.Play();
            }
        }
    }

    public void Pause(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            _audio = temp.GetComponent<AudioSource>();
            if (_audio != null && !_audio.isPlaying)
            {
                _audio.Pause();
            }
        }
    }

    public void Stop(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            _audio = temp.GetComponent<AudioSource>();
            if (_audio != null && !_audio.isPlaying)
            {
                _audio.Stop();
            }
        }
    }

    public bool IsPlaying(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            _audio = temp.GetComponent<AudioSource>();
            if (_audio != null && !_audio.isPlaying)
            {
                return _audio.isPlaying;
            }
        }
        return false;
    }
}
