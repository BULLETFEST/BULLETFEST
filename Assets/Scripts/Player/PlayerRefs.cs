using Mirror;
using UnityEngine;

public class PlayerRefs : ComponentRefs
{
  [SyncVar]
  public System.DateTime timeleft;

  public Canvas publicCanvas;

  public GameObject killfeed,
                    crown;

  [HideInInspector] public string playerName;
  [HideInInspector] public AudioSystem audioSystem;
  [HideInInspector] public PlayerMovement playerMovement;

  [HideInInspector]
  [SyncVar(hook = nameof(HandleUpdateDisplayName))]
  public string displayName;

  // Start is called before the first frame update
  protected override void Awake()
  {
    base.Awake();

    playerMovement = GetComponent<PlayerMovement>();
    audioSystem = FindObjectOfType<AudioSystem>();
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    UpdateDisplayName(connectionToClient);
  }

  [Command]
  private void UpdateDisplayName(NetworkConnectionToClient conn)
  {
    displayName = MyNetworkManager.instance.players[conn].displayName;
  }

  // [ServerCallback]
  private void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiName.text = newName;

    // Rpc_UpdateDisplayName();
  }

  [ClientRpc]
  private void Rpc_UpdateDisplayName()
  {
    uiName.text = displayName;
    name = displayName;
  }
}
