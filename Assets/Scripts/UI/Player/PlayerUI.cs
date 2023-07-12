using Mirror;
using TMPro;
using UnityEngine;

public class PlayerUI : NetworkBehaviour
{
  public TMP_Text uiGunAmmo;
  public TMP_Text uiTimeLeft;

  public Canvas mainCanvas;

  public GameObject crosshair;

  private PlayerRefs playerRefs;
  private bool focusState;

  private void Start()
  {
    if (!isLocalPlayer)
    {
      return;
    }

    playerRefs = GetComponent<PlayerRefs>();

    mainCanvas.gameObject.SetActive(true);
    mainCanvas.worldCamera = Camera.main;
    playerRefs.publicCanvas.gameObject.SetActive(true);

    crosshair.SetActive(true);
    Cursor.visible = false;


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

    Cursor.visible = !focusState && SaveSystem.IsSettingsOpen;

    crosshair.SetActive(!playerRefs.lockWeapon);

    crosshair.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
  }

  public void UpdateAmmoText(int bullets)
  {
    uiGunAmmo.text = $"{(bullets <= -1 ? "" : bullets)}";
  }
}
