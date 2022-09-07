using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsUI : MonoBehaviour
{
  public void ReturnToMain()
  {
    if (Utilities.FindWithType<AudioSystem>(out AudioSystem audioSystem))
    {
      audioSystem.PlaySound("Select");
    }
    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
  }
}
