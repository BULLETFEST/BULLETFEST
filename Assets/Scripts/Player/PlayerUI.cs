using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PlayerUI : NetworkBehaviour
{
  [Header("Weapon UI Elements")]
  public Image uiGunIcon;
  public Image uiReloadCircle;
  public Image uiGunPanel;
  public TextMeshProUGUI uiGunAmmo, uiGunName;

  public Canvas mainCanvas;

  public CanvasGroup infoGroup;

  public Slider uiHealthSlider;

  public GameObject crosshair;

  void Start()
  {
    if (!isLocalPlayer) return;

    mainCanvas.gameObject.SetActive(true);
    crosshair.SetActive(true);
    Cursor.visible = false;
  }

  private void OnApplicationFocus(bool focusStatus)
  {
    Cursor.visible = !focusStatus;
  }

  void Update()
  {
    if (!isLocalPlayer) return;

    crosshair.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
  }


  // Updates Weapon UI Elements
  // ! MUST BE CALLED ONLY ON GUN CHANGE
  public void UpdateWeaponUI(WeaponClass weapon)
  {
    uiGunAmmo.text = $"{weapon.bulletsInMag} / {weapon.magazineSize}";
    uiGunName.text = weapon.weaponName;

    uiGunAmmo.ForceMeshUpdate();
    uiGunName.ForceMeshUpdate();

    uiGunPanel.rectTransform.sizeDelta = new Vector2(Mathf.Max(uiGunAmmo.GetRenderedValues(false).x + 67, uiGunName.GetRenderedValues(false).x + 50) + 25, 85);
    uiReloadCircle.rectTransform.anchoredPosition = new Vector2(uiGunAmmo.GetRenderedValues(false).x + 42, -21);

    uiGunIcon.sprite = weapon.weaponSprite;
    uiGunIcon.rectTransform.sizeDelta = new Vector2(weapon.weaponSprite.bounds.size.x / (weapon.weaponSprite.bounds.size.y / 160), 160);
  }

  public void UpdateAmmoText(int bulletsInMag, int magazineSize)
  {
    uiGunAmmo.text = $"{bulletsInMag} / {magazineSize}";
    uiGunAmmo.ForceMeshUpdate();
    uiReloadCircle.rectTransform.anchoredPosition = new Vector2(uiGunAmmo.GetRenderedValues(false).x + 58, -21);
  }
}
