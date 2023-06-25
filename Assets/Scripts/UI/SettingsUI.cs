using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
  public Button generalBtn, displayBtn, soundBtn, keybindsBtn;
  public Image btnLine, waitForKeyImage;
  public GameObject[] settingsCategoryPanels;
  public GameObject[] keybindsObjects;
  public float lineMoveT;

  [Header("UI Elements that require loading")]
  public Toggle fpsToggle, invertControlsToggle;
  public TMP_InputField targetFpsField;
  public TMP_Dropdown screenModeDropdown, resolutionDropdown;
  public Slider sfxVolume;

  public AudioMixer sfx;

  [Header("Prefabs")]
  public GameObject fpsCounter;

  public bool waitingForKey;
  private List<string> resolutionsList = new();

  private void Awake()
  {
    SettingsClass settings = SaveSystem.saveData.settings;

    foreach (Resolution res in Screen.resolutions.Reverse().ToArray())
    {
      // Get rid of duplicates
      if (resolutionsList.IndexOf($"{res.width}x{res.height}") < 0)
      {
        resolutionsList.Add($"{res.width}x{res.height}");
      }
    }
    resolutionDropdown.AddOptions(resolutionsList);

    // Load saved settings UI
    // fpsToggle.isOn = settings.fpsCounter;
    invertControlsToggle.isOn = settings.invertControls;
    targetFpsField.text = (settings.targetFps != 0 ? settings.targetFps : 60).ToString();
    screenModeDropdown.value = settings.screenMode;
    resolutionDropdown.value = settings.resolution;

    // Load saved settings
    Application.targetFrameRate = Mathf.Clamp(settings.targetFps, 60, 360);

    if (settings.screenMode != 0)
    {
      string[] chosenRes = resolutionsList[settings.resolution].Split('x');
      Screen.SetResolution(int.Parse(chosenRes[0]), int.Parse(chosenRes[1]), (FullScreenMode)settings.screenMode);
    }
    else
    {
      Screen.fullScreenMode = (FullScreenMode)settings.screenMode;
      resolutionDropdown.interactable = false;
    }

    resolutionDropdown.onValueChanged.AddListener(delegate
    {
      DropdownResolution(resolutionDropdown.value);
    });

    UpdateBindUI();

    if (settings.fpsCounter)
    {
      _ = Instantiate(fpsCounter);
    }
  }

  private void Start()
  {
    sfxVolume.value = SaveSystem.saveData.settings.sfxVolume;
  }

  private void UpdateBindUI()
  {
    Dictionary<string, string> binds = SaveSystem.saveData.settings.keybinds;
    for (int i = 0, bindIdx = 0; i < keybindsObjects.Length; i++, bindIdx++)
    {
      keybindsObjects[i].transform.GetChild(1).GetComponentInChildren<TMP_Text>().text = Utilities.AddSpacesToString(binds.ElementAt(bindIdx).Value);
      if (binds.Count > bindIdx + 1 && binds.ElementAt(bindIdx + 1).Key.EndsWith("2"))
      {
        bindIdx++;
        keybindsObjects[i].transform.GetChild(2).GetComponentInChildren<TMP_Text>().text = Utilities.AddSpacesToString(binds.ElementAt(bindIdx).Value);
      }
      else
      {
        keybindsObjects[i].transform.GetChild(2).GetComponentInChildren<TMP_Text>().text = "None";
      }
    }
  }

  private void Update()
  {
    // if (Input.GetKeyDown(KeyCode.Escape) && !waitingForKey)
    // {
    //   Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    //   thisCanvas.enabled = !thisCanvas.enabled;
    //   isPaused = !isPaused;
    //   if (!isPaused)
    //   {
    //     SaveSystem.SavePlayer(new SaveDataStructure(SaveSystem.saveData.settings));
    //   }
    // }

    btnLine.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(btnLine.rectTransform.anchoredPosition.x, 0, Time.unscaledDeltaTime * lineMoveT), btnLine.rectTransform.anchoredPosition.y);
  }

  public void MoveLine(RectTransform btn)
  {
    btnLine.rectTransform.SetParent(btn);
  }

  public void EnablePanel(int index)
  {
    for (int i = 0; i < settingsCategoryPanels.Length; i++)
    {
      if (i == index)
      {
        settingsCategoryPanels[i].SetActive(true);
      }
      else
      {
        settingsCategoryPanels[i].SetActive(false);
      }
    }
  }

  private string AwaitKey()
  {
    foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
    {
      if (Input.GetKey(kcode))
      {
        return kcode == KeyCode.Escape ? "cancel" : kcode.ToString();
      }
    }
    return null;
  }

  public void ChangeBind(string bind)
  {
    waitingForKey = true;
    waitForKeyImage.gameObject.SetActive(true);

    _ = StartCoroutine(ChangeBindEnum(bind));
  }

  private IEnumerator ChangeBindEnum(string bind)
  {
    string key = null;
    while (key == null)
    {
      key = AwaitKey();
      yield return null;
    }

    waitingForKey = false;
    waitForKeyImage.gameObject.SetActive(false);

    if (key == "cancel")
    {
      yield break;
    }

    Dictionary<string, string> binds = SaveSystem.saveData.settings.keybinds;
    if (binds.ContainsValue(key))
    {
      binds[binds.ElementAt(Array.IndexOf(binds.Values.ToArray(), key)).Key] = "None";
      binds[bind] = key;
    }
    else
    {
      binds[bind] = key;
    }

    SaveSystem.saveData.settings.keybinds = binds;


    UpdateBindUI();
  }

  /* TOOGLE FUNCTIONS , ETC */

  public void ToggleFPSCounter(bool value)
  {
    SaveSystem.saveData.settings.fpsCounter = value;

    if (value)
    {
      _ = Instantiate(fpsCounter);
    }
    else
    {
      GameObject[] objs = GameObject.FindGameObjectsWithTag("FPSCounter");
      foreach (GameObject obj in objs)
      {
        Destroy(obj);
      }
    }
  }


  public void ToggleInvertControls(bool value)
  {
    SaveSystem.saveData.settings.invertControls = value;
  }

  public void TargetFPSFieldOnEdit(string value)
  {
    if (value.Contains('-'))
    {
      targetFpsField.text = value.Replace("-", "");
    }
  }

  public void TargetFPSFieldEndEdit(string value)
  {
    int intVal;
    try
    {
      intVal = Mathf.Clamp(int.Parse(value), 60, 360);
    }
    catch
    {
      intVal = 60;
    }

    SaveSystem.saveData.settings.targetFps = intVal;
    Application.targetFrameRate = intVal;
  }

  public void DropdownScreenMode(int value)
  {
    if (value == 2)
    {
      value = 3;
    }
    else if (value == 0)
    {
      resolutionDropdown.value = 0;
    }

    SaveSystem.saveData.settings.screenMode = value;
    Screen.fullScreenMode = (FullScreenMode)value;
    resolutionDropdown.interactable = value != 0;
  }

  public void DropdownResolution(int value)
  {
    string[] chosenRes = resolutionsList[value].Split('x');
    Screen.SetResolution(int.Parse(chosenRes[0]), int.Parse(chosenRes[1]), (FullScreenMode)SaveSystem.saveData.settings.screenMode);
    SaveSystem.saveData.settings.resolution = value;
  }

  public void ChangeSFXVolume(float v)
  {
    _ = sfx.SetFloat("SFX_Vol", v);
    SaveSystem.saveData.settings.sfxVolume = v;
  }
}
