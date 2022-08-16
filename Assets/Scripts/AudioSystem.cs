using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSystem : MonoBehaviour
{
  Dictionary<string, AudioClip> clips = new();

  AudioSource source;

  // Start is called before the first frame update
  void Start()
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
      source.Play();
    }
    else
    {
      Debug.LogWarning("This sound does not exist!");
    }
  }
}
