using UnityEngine;

public class CreditsUI : MonoBehaviour
{
  public void ReturnToMain()
  {
    if (Utilities.FindWithType(out AudioSystem audioSystem))
    {
      audioSystem.PlaySound("Select");
    }
    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
  }
}
