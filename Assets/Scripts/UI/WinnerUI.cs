using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
public class WinnerUI : NetworkBehaviour
{
  public TMP_Text winnerText;

  MyNetworkManager nm;

  private void Start()
  {
    GetComponent<Canvas>().worldCamera = Camera.main;
    Time.timeScale = 0;
  }
  //   nm = GameObject.FindObjectOfType<MyNetworkManager>();
  // }

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
