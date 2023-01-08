using System.Collections;
using EpicTransport;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitializationUI : MonoBehaviour
{
  public TMP_Text loadingText;

  [Header("Signup UI Elements")]
  public TMP_InputField s_Email;
  public TMP_InputField s_Pass;
  public Button signupBtn;

  [Header("Login UI Elements")]
  public TMP_InputField l_Email;
  public TMP_InputField l_Pass;
  public Button loginBtn;

  public GameObject loginPanel, whyNeedAccount;


  void Start()
  {
    StartCoroutine(LoadingTextAnimation());

    EOSSDKComponent.Initialize();
  }

  IEnumerator LoadingTextAnimation()
  {
    int dots = 0;
    while (loadingText.gameObject.activeInHierarchy)
    {
      if (dots >= 4) dots = 0;
      loadingText.text = $"Loading{new string('.', dots)}";
      dots++;
      yield return new WaitForSecondsRealtime(0.25f);
    }
  }

  // void OnDestroy()
  // {
  //   FirebaseManager.AuthStateChanged -= CheckUser;
  // }

  void Update()
  {
    if (EOSSDKComponent.Initialized)
    {
      if (!loadingText.gameObject.activeInHierarchy) return;

      FirebaseManager.Response<string> v = FirebaseManager.CheckServerStatus();
      if (v.status != 200)
      {
        Message.DisplayMessage("Failed to connect to server!",
                               "The servers are currently down, try again later, or you could try notifying the developer or you could try going to https://joobot.glitch.me",
                               Application.Quit,
                               HorizontalAlignmentOptions.Center);

        loadingText.gameObject.SetActive(false);
        return;
      }
      else if (!Debug.isDebugBuild && new Version(v.data).IsMoreRecent(new Version(Application.version)))
      {
        Message.DisplayMessage("Update available!",
                               "Please update your game.",
                               Application.Quit,
                               HorizontalAlignmentOptions.Center);

        loadingText.gameObject.SetActive(false);
        return;
      }

      if (!string.IsNullOrEmpty(SaveSystem.saveData.token))
      {
        FirebaseManager.Response<bool> res = FirebaseManager.ValidateToken(SaveSystem.saveData.token);

        if (res.status != 200)
        {
          Message.DisplayMessage("Failed to connect to server!",
                               "The servers are currently down, try again later, or you could try notifying the developer or you could try going to https://joobot.glitch.me",
                               Application.Quit,
                               HorizontalAlignmentOptions.Center);

          loadingText.gameObject.SetActive(false);
          return;
        }

        if (res.data)
        {
          SceneManager.LoadScene(1);
        }
        else
        {
          SaveSystem.saveData.token = "";
          SaveSystem.SavePlayer(SaveSystem.saveData);
        }
      }

      loadingText.gameObject.SetActive(false);
      loginPanel.SetActive(true);
      whyNeedAccount.SetActive(true);
    }
  }

  public async void Login()
  {
    if (string.IsNullOrEmpty(l_Email.text) || string.IsNullOrEmpty(l_Pass.text))
    {
      Message.DisplayMessage("Invalid Form", "Please fill out the form properly and try again", HorizontalAlignmentOptions.Center);
      return;
    }

    loginBtn.interactable = false;
    signupBtn.interactable = false;
    FirebaseManager.Response<string> res = await FirebaseManager.Login(l_Email.text, l_Pass.text);

    if (res.status == 200)
    {
      SaveSystem.saveData.token = res.data;
      SaveSystem.SavePlayer(SaveSystem.saveData);
      SceneManager.LoadScene(1);
    }
    else
    {
      Message.DisplayMessage("", res.message, HorizontalAlignmentOptions.Center);
    }

    loginBtn.interactable = true;
    signupBtn.interactable = true;
  }

  public async void Signup()
  {
    if (string.IsNullOrEmpty(s_Email.text) || string.IsNullOrEmpty(s_Pass.text))
    {
      Message.DisplayMessage("Invalid Form", "Please fill out the form properly and try again", HorizontalAlignmentOptions.Center);
      return;
    }

    loginBtn.interactable = false;
    signupBtn.interactable = false;
    FirebaseManager.Response<string> res = await FirebaseManager.CreateUser(s_Email.text, s_Pass.text);

    if (res.status == 200)
    {
      SaveSystem.saveData.token = res.data;
      SaveSystem.SavePlayer(SaveSystem.saveData);
      SceneManager.LoadScene(1);
    }
    else
    {
      Message.DisplayMessage("", res.message, HorizontalAlignmentOptions.Center);
    }

    loginBtn.interactable = true;
    signupBtn.interactable = true;
  }

  public void ShowReason()
  {
    Message.DisplayMessage("Why do I need an account?", "Accounts are required because the game is cross-platform, and is being hosted on multiple places, even on the same platform. Accounts help with friendlist, keeping cheaters away, etc.");
  }
}
