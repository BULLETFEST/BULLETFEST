using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
  public static SaveDataStructure saveData;
  private static Dictionary<string, string> defaultBinds;
  private static string appDataPath;

  public static bool IsSettingsOpen;

  private void Awake()
  {
    appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    saveData = LoadPlayer();

    defaultBinds = new Dictionary<string, string>() {
      {"fire", "Return"},
      {"fire2", "Mouse0"},

      {"altFire", "Mouse1"},
      {"altFire2", "Backspace"},

      {"jump", "Space"},
      {"jump2", "None"},

      {"weaponPickup", "E"},
      {"weaponPickup2", "None"},

      {"rgt", "D"},
      {"rgt2", "RightArrow"},

      {"lft", "A"},
      {"lft2", "LeftArrow"}
    };

    if (saveData == null || saveData.settings == null)
    {
      saveData = new SaveDataStructure(new SettingsClass(defaultBinds, 1, 1, 1, false, false, 60, 0, 0), "");
    }
    else
    {
      saveData.settings.keybinds ??= new Dictionary<string, string>();
    }

    for (int i = 0; i < defaultBinds.Count - 1; i++)
    {
      if (!saveData.settings.keybinds.ContainsKey(defaultBinds.ElementAt(i).Key))
      {
        saveData.settings.keybinds[defaultBinds.ElementAt(i).Key] = defaultBinds.ElementAt(i).Value;
      }
    }

    settingsUI = FindObjectOfType<SettingsUI>();
  }

  public static SettingsUI settingsUI;

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (!IsSettingsOpen)
      {
        if (Utilities.FindWithTag("SettingsBlocker", out GameObject gameObject))
        {
          if (gameObject.activeSelf)
          {
            gameObject.SetActive(false);
            return;
          }
        }
        // SceneManager.LoadSceneAsync("Settings", LoadSceneMode.Additive);

        settingsUI.GetComponent<Canvas>().enabled = true;
        IsSettingsOpen = true;

        //Cursor.visible = true;
      }
      else
      {
        if (settingsUI.waitingForKey)
        {
          return;
        }

        SavePlayer(new SaveDataStructure(saveData.settings, saveData.token));
        // SceneManager.UnloadSceneAsync("Settings");
        settingsUI.GetComponent<Canvas>().enabled = false;
        IsSettingsOpen = false;
        //if (SceneManager.GetActiveScene().buildIndex > MyNetworkManager.menuScenes - 1) Cursor.visible = false;

      }
    }
  }

  public static void SavePlayer(SaveDataStructure SDS)
  {
    BinaryFormatter formatter = new();
    string path = Path.Combine(appDataPath, "BULLETFEST/settings.save");
    if (!Directory.Exists(Path.Combine(appDataPath, "BULLETFEST")))
    {
      _ = Directory.CreateDirectory(Path.Combine(appDataPath, "BULLETFEST"));
    }
    FileStream stream = new(path, FileMode.Create);

    formatter.Serialize(stream, SDS);
    stream.Close();
  }

  public static SaveDataStructure LoadPlayer()
  {
    string path = Path.Combine(appDataPath, "BULLETFEST/settings.save");
    if (File.Exists(path))
    {
      BinaryFormatter formatter = new();
      FileStream stream = new(path, FileMode.Open);

      SaveDataStructure data = formatter.Deserialize(stream) as SaveDataStructure;
      stream.Close();
      return data;
    }
    else
    {
      return null;
    }
  }

  public static void DeleteData()
  {
    string path = Path.Combine(appDataPath, "BULLETFEST/settings.save");
    if (File.Exists(path))
    {
      File.Delete(path);
    }
  }
}
