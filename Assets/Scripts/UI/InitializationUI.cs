using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EpicTransport;

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

  public GameObject loginPanel;

  void Start()
  {
    StartCoroutine(LoadingTextAnimation());

    // FirebaseManager.AuthStateChanged += CheckUser;
    FirebaseManager.InitializeFirebase();
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
    if (EOSSDKComponent.Initialized && FirebaseManager.Initialized)
    {
      if (!loadingText.gameObject.activeInHierarchy) return;

      string v = FirebaseManager.CheckServerStatus();
      if (v == null)
      {
        Message.DisplayMessage("Failed to connect to server!",
                               "The servers are currently down, try again later, or you could try notifying the developer or you could try going to https://joobot.glitch.me",
                               Application.Quit,
                               HorizontalAlignmentOptions.Center);

        loadingText.gameObject.SetActive(false);
        return;
      }
      else if (new Version(v).IsMoreRecent(new Version(Application.version)))
      {
        Message.DisplayMessage("Update available!",
                               "Please update your game.",
                               Application.Quit,
                               HorizontalAlignmentOptions.Center);

        loadingText.gameObject.SetActive(false);
        return;
      }

      if (FirebaseManager.user != null) SceneManager.LoadScene("MainMenu");

      loadingText.gameObject.SetActive(false);
      loginPanel.SetActive(true);
    }
  }

  public void Login()
  {
    if (string.IsNullOrEmpty(l_Email.text) || string.IsNullOrEmpty(l_Pass.text))
    {
      Message.DisplayMessage("Invalid Form", "Please fill out the form properly and try again", HorizontalAlignmentOptions.Center);
      return;
    }

    loginBtn.interactable = false;
    signupBtn.interactable = false;
    FirebaseManager.LoginWithCredentials(l_Email.text, l_Pass.text).ContinueWith(task =>
    {
      if (task.Result) SceneManager.LoadScene("MainMenu");

      loginBtn.interactable = true;
      signupBtn.interactable = true;
    });
  }

  public void Signup()
  {
    if (string.IsNullOrEmpty(s_Email.text) || string.IsNullOrEmpty(s_Pass.text))
    {
      Message.DisplayMessage("Invalid Form", "Please fill out the form properly and try again", HorizontalAlignmentOptions.Center);
      return;
    }

    loginBtn.interactable = false;
    signupBtn.interactable = false;
    FirebaseManager.CreateUser(s_Email.text, s_Pass.text).ContinueWith(task =>
    {
      if (task.Result) SceneManager.LoadScene("MainMenu");

      loginBtn.interactable = true;
      signupBtn.interactable = true;
    });
  }
}
