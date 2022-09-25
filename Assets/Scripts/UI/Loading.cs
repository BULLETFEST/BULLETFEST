using UnityEngine;

public class Loading : MonoBehaviour
{
  static Canvas loadingCanvas;
  // Start is called before the first frame update
  void Start()
  {
    loadingCanvas = GetComponent<Canvas>();
  }

  public static void Show() => loadingCanvas.enabled = true;

  public static void Hide() => loadingCanvas.enabled = false;
}
