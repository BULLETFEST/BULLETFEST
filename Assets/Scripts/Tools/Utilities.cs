using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Utilities : MonoBehaviour
{

  public static Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

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
    return SaveSystem.saveData.settings.keybinds.ContainsKey(key)
&& (Input.GetKey(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key])) || Input.GetKey(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key + "2"])));
  }

  public static bool GetKeybindDown(string key)
  {
    return SaveSystem.saveData.settings.keybinds.ContainsKey(key)
&& (Input.GetKeyDown(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key])) || Input.GetKeyDown(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key + "2"])));
  }

  public static bool GetKeybindUp(string key)
  {
    return SaveSystem.saveData.settings.keybinds.ContainsKey(key)
&& (Input.GetKeyUp(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key])) || Input.GetKeyUp(StringToKeyCode(SaveSystem.saveData.settings.keybinds[key + "2"])));
  }

  public static string AddSpacesToString(string text)
  {
    List<char> textList = text.ToCharArray().ToList();
    string output = "";
    for (int i = 0; i < textList.Count; i++)
    {
      char character = textList[i];
      if ((char.IsUpper(character) || char.IsNumber(character)) && i > 0)
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
    gameObject = FindObjectOfType<T>();
    return gameObject != null;
  }

  // <3 codemonkey
  public static float GetAngleFromVectorFloat(Vector3 dir)
  {
    dir = dir.normalized;
    float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    if (n < 0)
    {
      n += 360;
    }

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

  public static float CalculateDistance(Vector3 a, Vector3 b)
  {
    Vector3 diff = b - a;
    return diff.sqrMagnitude;
  }

  public static GameObject FindNearest(Transform origin, string tag, float maxDist = -1)
  {
    Transform[] objects = GameObject.FindGameObjectsWithTag(tag).Select(x => x.transform).ToArray();

    return FindNearest(origin, objects);
  }

  public static GameObject FindNearest(Transform origin, Component[] objects, float maxDist = -1)
  {
    GameObject closest = null;
    float distance = Mathf.Infinity;
    foreach (Component go in objects)
    {
      float curDistance = CalculateDistance(origin.position, go.transform.position);
      if (curDistance < distance)
      {
        closest = go.gameObject;
        distance = curDistance;
      }
    }

    // if maxDist bigger than -1: check if distance is smaller than maxDist if yes return closest otherwise null
    // else return closest
    return maxDist > -1 ? distance <= maxDist ? closest : null : closest;
  }

  public static GameObject FindFurthest(Transform origin, string tag)
  {
    Transform[] objects = GameObject.FindGameObjectsWithTag(tag).Select(x => x.transform).ToArray();

    return FindFurthest(origin, objects);
  }

  public static GameObject FindFurthest(Transform origin, Component[] objects)
  {
    GameObject furthest = null;
    float distance = 0;
    foreach (Component go in objects)
    {
      float curDistance = CalculateDistance(go.transform.position, origin.position);
<<<<<<< Updated upstream
=======
      // print(curDistance);
>>>>>>> Stashed changes
      if (curDistance > distance)
      {
        furthest = go.gameObject;
        distance = curDistance;
      }
    }

<<<<<<< Updated upstream
=======
    // print(furthest);

>>>>>>> Stashed changes
    return furthest;
  }

  public static RaycastHit2D Grounded(Transform origin, BoxCollider2D bc, LayerMask layerMask, float distance = 0.25f)
  {
    return Physics2D.BoxCast(
      origin.position,
      bc.bounds.size, 0, Vector2.down,
      distance, layerMask);
  }
}
