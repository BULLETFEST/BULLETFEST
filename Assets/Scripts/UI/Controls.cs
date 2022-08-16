using UnityEngine;
using UnityEngine.SceneManagement;

public class Controls : MonoBehaviour
{
  public void BackToMainMenu()
  {
    SceneManager.LoadScene("MainMenu");
  }
}
