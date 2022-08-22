using UnityEngine;
using TMPro;
using Mirror;

public class PlayerVars : NetworkBehaviour
{
  [SyncVar]
  public System.DateTime timeleft;

  public TMP_Text uiName;

  public Rigidbody2D rb { get; set; }
  public BoxCollider2D bc { get; set; }

  public string playerName { get; set; }

  public Canvas publicCanvas;

  public GameObject killfeed,
                    crown;

  [HideInInspector]

  [SyncVar]
  public bool lockMovement = false,
              lockShooting = false,
              lockWeapon = false;

  public SpriteGroup graphics;

  public WeaponBehavior weaponBehavior;

  public AudioSystem audioSystem;

  // [HideInInspector]
  [SyncVar(hook = nameof(HandleUpdateDisplayName))]
  public string displayName;

  // Start is called before the first frame update
  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();
    // name = PlayerPrefs.GetString("PlayerName", "Guest");
    // weaponBehavior = GetComponentInChildren<WeaponBehavior>();

    audioSystem = FindObjectOfType<AudioSystem>();
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    UpdateDisplayName(connectionToClient);
  }

  [Command]
  void UpdateDisplayName(NetworkConnectionToClient conn)
  {
    displayName = MyNetworkManager.instance.players[conn].displayName;
  }

  // [ServerCallback]
  void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiName.text = newName;

    // Rpc_UpdateDisplayName();
  }

  [ClientRpc]
  void Rpc_UpdateDisplayName()
  {
    uiName.text = displayName;
    name = displayName;
  }
}
