using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
  private Dictionary<string, AudioClip> clips = new();
  private AudioSource source;

  // Start is called before the first frame update
  private void Start()
  {
    AudioClip[] temp = Resources.LoadAll<AudioClip>("Sounds");
    foreach (AudioClip clip in temp)
    {
      clips[clip.name] = clip;
      // print(clip.name);
    }

    source = GetComponent<AudioSource>();
  }

  public void PlaySound(string name)
  {
    if (clips.ContainsKey(name))
    {
      source.clip = clips[name];
      source.pitch = Random.Range(0.9f, 1.1f);
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
