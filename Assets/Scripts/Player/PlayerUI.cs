using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public class PlayerUI : NetworkBehaviour
{
  public TextMeshProUGUI uiGunAmmo;
  public TextMeshProUGUI uiTimeLeft;

  public Canvas mainCanvas;


  public GameObject crosshair;

  private PlayerVars playerVars;

  void Start()
  {
    if (!isLocalPlayer) return;

    mainCanvas.gameObject.SetActive(true);
    mainCanvas.worldCamera = Camera.main;
    crosshair.SetActive(true);
    Cursor.visible = false;

    playerVars = GetComponent<PlayerVars>();

    // StartCoroutine(UpdateTime());
  }

  private void OnApplicationFocus(bool focusStatus)
  {
    Cursor.visible = !focusStatus;
  }

  IEnumerator UpdateTime()
  {
    TimeSpan timeSpan = new TimeSpan(0, 5, 0);
    while (timeSpan.TotalSeconds >= 0)
    {
      timeSpan = playerVars.timeleft.Subtract(DateTime.UtcNow);//FindObjectOfType<PlayerSpawnSystem>().timeStamp.Subtract(DateTime.Now);
      // int secondsLeft = (int)timeSpan.Minutes;
      // uiTimeLeft.text = $"{Mathf.Floor(secondsLeft / 60)}:{Mathf.Floor(secondsLeft / Mathf.Floor(secondsLeft / 60))}";
      uiTimeLeft.text = $"{timeSpan.Minutes}:{timeSpan.Seconds}";

      yield return new WaitForSeconds(1);
    }
  }

  void Update()
  {
    if (!isLocalPlayer) return;

    crosshair.SetActive(!playerVars.lockWeapon);

    crosshair.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
  }

  public void UpdateAmmoText(int bullets)
  {
    uiGunAmmo.text = $"{bullets}";
  }
}
