using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerVars : NetworkBehaviour
{
  private MyNetworkManager room;
  private MyNetworkManager Room
  {
    get
    {
      if (room != null) { return room; }
      return room = NetworkManager.singleton as MyNetworkManager;
    }
  }


  public TextMeshProUGUI uiName;

  public Rigidbody2D rb { get; set; }
  public BoxCollider2D bc { get; set; }

  public string playerName { get; set; }

  public Coroutine reloadRoutine { get; set; }

  public Canvas publicCanvas;
  public GameObject killfeed;

  [HideInInspector]
  public bool lockMovement = false,
              lockShooting = false,
              lockWeapon = false,
              isReloading = false;

  public SpriteGroup graphics;

  public WeaponBehavior weaponBehavior;

  [SyncVar(hook = nameof(HandleUpdateDisplayName))]
  [HideInInspector]
  public string displayName;

  // Start is called before the first frame update
  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();
    // name = PlayerPrefs.GetString("PlayerName", "Guest");
    // weaponBehavior = GetComponentInChildren<WeaponBehavior>();
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    UpdateDisplayName();
  }

  [Command]
  void UpdateDisplayName() => displayName = Room.players[connectionToClient];

  // [ServerCallback]
  void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiName.text = newName;

    Rpc_UpdateDisplayName();
  }

  [ClientRpc]
  void Rpc_UpdateDisplayName() => uiName.text = displayName;
}
