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

  void Start()
  {
    if (isLocalPlayer)
      mainCanvas.gameObject.SetActive(true);
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

  public void UpdateAmmoText(WeaponClass weapon)
  {
    uiGunAmmo.text = $"{weapon.bulletsInMag} / {weapon.magazineSize}";
    uiGunAmmo.ForceMeshUpdate();
    uiReloadCircle.rectTransform.anchoredPosition = new Vector2(uiGunAmmo.GetRenderedValues(false).x + 58, -21);
  }
}
