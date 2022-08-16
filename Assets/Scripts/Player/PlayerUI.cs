using UnityEngine;
using TMPro;
using Mirror;

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
