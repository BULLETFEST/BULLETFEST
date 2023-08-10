using Mirror;
using TMPro;
using UnityEngine;

public class PlayerUI : NetworkBehaviour
{
  [SerializeField] TMP_Text uiGunAmmo;
  [SerializeField] TMP_Text uiTimeLeft;

  [SerializeField] Canvas uiMainCanvas;

  [SerializeField] GameObject uiCrosshair;
  [SerializeField] GameObject uiMobileControls;
  [SerializeField] GameObject uiAltFire;

  private PlayerRefs playerRefs;
  private bool focusState;

  private void Start()
  {
    if (!isLocalPlayer)
    {
      return;
    }

    playerRefs = GetComponent<PlayerRefs>();

    uiMainCanvas.gameObject.SetActive(true);
    // mainCanvas.worldCamera = Camera.main;

    Cursor.visible = false;

    if (SystemInfo.deviceType == DeviceType.Handheld) uiMobileControls.SetActive(true);
    else uiCrosshair.SetActive(true);



    // StartCoroutine(UpdateTime());
  }

  private void OnApplicationFocus(bool _focusState)
  {
    // Cursor.visible = !focusStatus || SaveSystem.IsSettingsOpen;
    focusState = _focusState;
  }

  private void Update()
  {
    if (!isLocalPlayer)
    {
      return;
    }

    if (SystemInfo.deviceType == DeviceType.Handheld)
    {
      return;
    }

    Cursor.visible = !focusState && SettingsUI.IsSettingsOpen;

    uiCrosshair.SetActive(!playerRefs.lockWeapon);

    uiCrosshair.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
  }

  public void UpdateAmmoText(int bullets)
  {
    uiGunAmmo.text = $"{(bullets <= -1 ? "" : bullets)}";
  }

  public void UpdateTimer(string timeString)
  {
    uiTimeLeft.text = timeString;
  }

  public void AltFireBtnVisibility(bool visible)
  {
    uiAltFire.SetActive(visible);
  }
}
