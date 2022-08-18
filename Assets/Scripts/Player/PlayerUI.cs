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

  bool focusState;

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

  private void OnApplicationFocus(bool _focusState)
  {
    // Cursor.visible = !focusStatus || SaveSystem.IsSettingsOpen;
    focusState = _focusState;
  }

  void Update()
  {
    if (!isLocalPlayer) return;

    Cursor.visible = !focusState || SaveSystem.IsSettingsOpen;

    crosshair.SetActive(!playerVars.lockWeapon);

    crosshair.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
  }

  public void UpdateAmmoText(int bullets)
  {
    uiGunAmmo.text = $"{bullets}";
  }
}
