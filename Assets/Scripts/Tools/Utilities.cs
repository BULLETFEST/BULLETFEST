using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

  // <3 codemonkey
  public static float GetAngleFromVectorFloat(Vector3 dir)
  {
    dir = dir.normalized;
    float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    if (n < 0) n += 360;

    return n;
  }
}
