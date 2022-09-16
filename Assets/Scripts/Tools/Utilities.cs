using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using EpicTransport;
using Epic;

public class Utilities : MonoBehaviour
{
  public static void PrintArr<T>(T[] array)
  {
    string res = "[ ";
    for (int i = 0; i < array.Length; i++)
    {
      res += array[i].ToString() + ", ";
    }
    res += " ]";
    print(res);
  }

  public static void PrintDict<TKey, TElement>(IDictionary<TKey, TElement> dict)
  {
    string res = "{ ";
    for (int i = 0; i < dict.Count; i++)
    {
      res += $"{dict.ElementAt(i).Key}: {dict.ElementAt(i).Value},\n";
    }
    res += " }";
    print(res);
  }

  public static KeyCode StringToKeyCode(string key)
  {
    return (KeyCode)Enum.Parse(typeof(KeyCode), key);
  }

  public static bool GetKeybind(string key)
  {
    if (!SaveSystem.saveData.settings.keybinds.ContainsKey(key)) return false;
    return Input.GetKey(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key])) || Input.GetKey(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key + "2"]));
  }

  public static bool GetKeybindDown(string key)
  {
    if (!SaveSystem.saveData.settings.keybinds.ContainsKey(key)) return false;
    return Input.GetKeyDown(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key])) || Input.GetKeyDown(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key + "2"]));
  }

  public static bool GetKeybindUp(string key)
  {
    if (!SaveSystem.saveData.settings.keybinds.ContainsKey(key)) return false;
    return Input.GetKeyUp(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key])) || Input.GetKeyUp(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key + "2"]));
  }

  public static string AddSpacesToString(string text)
  {
    List<char> textList = text.ToCharArray().ToList();
    string output = "";
    for (int i = 0; i < textList.Count; i++)
    {
      char character = textList[i];
      if ((Char.IsUpper(character) || Char.IsNumber(character)) && i > 0)
      {
        output += $" {character}";
      }
      else
      {
        output += character;
      }
    }
    return output;
  }

  public static bool FindWithTag(string tag, out GameObject gameObject)
  {
    gameObject = GameObject.FindGameObjectWithTag(tag);
    return gameObject != null;
  }

  public static bool FindWithType<T>(out T gameObject) where T : Component
  {
    gameObject = GameObject.FindObjectOfType<T>();
    return gameObject != null;
  }

  // <3 codemonkey
  public static float GetAngleFromVectorFloat(Vector3 dir)
  {
    dir = dir.normalized;
    float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    if (n < 0) n += 360;

    return n;
  }

  public static bool AnimatorStateDonePlaying(Animator anim)
  {
    return anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
  }

  public static bool AnimatorStateDonePlaying(Animator anim, string stateName)
  {
    return AnimatorStateDonePlaying(anim, stateName) && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
  }

  public static void Disconnect()
  {
    if (FindWithType<MyNetworkManager>(out MyNetworkManager networkManager))
    {
      if (networkManager.mode == Mirror.NetworkManagerMode.ServerOnly) networkManager.StopServer();
      else if (networkManager.mode == Mirror.NetworkManagerMode.Host) networkManager.StopHost();
      else if (networkManager.mode == Mirror.NetworkManagerMode.ClientOnly) networkManager.StopClient();

      SceneManager.LoadScene("MainMenu");
    }
  }

  public static GameObject FindNearest(Transform origin, string tag)
  {
    GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

    GameObject closest = null;
    float distance = Mathf.Infinity;
    Vector3 position = origin.position;
    foreach (GameObject go in objects)
    {
      Vector3 diff = go.transform.position - position;
      float curDistance = diff.sqrMagnitude;
      if (curDistance < distance)
      {
        //if pickable can be inserted here ~Toast
        closest = go;
        distance = curDistance;
      }
    }

    return closest;
  }

  public static GameObject FindFurthest(Transform origin, string tag)
  {
    GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

    GameObject furthest = null;
    float distance = Mathf.Infinity;
    Vector3 position = origin.position;
    foreach (GameObject go in objects)
    {
      Vector3 diff = go.transform.position - position;
      float curDistance = diff.sqrMagnitude;
      if (curDistance > distance)
      {
        //if pickable can be inserted here ~Toast
        furthest = go;
        distance = curDistance;
      }
    }

    return furthest;
  }
}
