using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerUI : NetworkBehaviour
{
  public TMP_Text winnerText;
  public Image playerImage;
  private MyNetworkManager nm;

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

    MyNetworkManager.instance.CycleMap();
  }
}
