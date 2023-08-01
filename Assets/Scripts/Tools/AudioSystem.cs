using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
  private Dictionary<string, AudioClip> clips = new();
  private AudioSource source;

  public static AudioSystem Instance { get; private set; }

  // Start is called before the first frame update
  private void Awake()
  {
    AudioClip[] temp = Resources.LoadAll<AudioClip>("Sounds");
    foreach (AudioClip clip in temp)
    {
      clips[clip.name] = clip;
      // print(clip.name);
    }

    source = GetComponent<AudioSource>();

    Instance = this;
  }

  public void PlaySound(string name, bool interruptPrevious = false, bool varyPitch = false)
  {
    if (clips.ContainsKey(name))
    {
      if (interruptPrevious)
      {
        source.Stop();
      }

      source.clip = clips[name];
      source.pitch = varyPitch ? Random.Range(0.9f, 1.1f) : 1;
      source.Play();
    }
    else
    {
      Debug.LogWarning("This sound does not exist!");
    }
  }

  public void PlaySound(AudioClip sound)
  {
    source.clip = sound;
    source.pitch = Random.Range(0.9f, 1.1f);
    source.Play();
  }
}
