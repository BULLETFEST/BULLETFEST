using System.Collections;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class WinnerUI : NetworkBehaviour
{
  public TMP_Text winnerText;
  public Image playerImage;

  MyNetworkManager nm;

  private void Start()
  {
    GetComponent<Canvas>().worldCamera = Camera.main;
    Time.timeScale = 0;
  }

  public void AnimationOver()
  {
    StartCoroutine(SwitchScene());
  }

  [ServerCallback]
  public IEnumerator SwitchScene()
  {
    yield return new WaitForSecondsRealtime(5);

    GameObject.FindObjectOfType<MyNetworkManager>().CycleMap();
  }
}
