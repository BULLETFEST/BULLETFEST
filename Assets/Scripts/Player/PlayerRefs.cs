using Mirror;
using UnityEngine;

public class PlayerRefs : ComponentRefs
{
  [SyncVar]
  public System.DateTime timeleft;

  public GameObject killfeed,
                    crown;

  [HideInInspector] public PlayerUI uiController;

  [HideInInspector]
  [SyncVar(hook = nameof(HandleUpdateDisplayName))]
  public string displayName;

  // Start is called before the first frame update
  protected override void Awake()
  {
    base.Awake();
    uiController = GetComponent<PlayerUI>();
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    UpdateDisplayName(PlayerPrefs.GetString("PlayerName", "Guest"));
  }

  [Command]
  private void UpdateDisplayName(string fallbackName)
  {
    if (GameManager.Instance.players.ContainsKey(connectionToClient.connectionId))
    {
      displayName = GameManager.Instance.players[connectionToClient.connectionId].displayName;
      return;
    }

    string tempName = fallbackName;
    if (tempName.Length > 16) tempName = tempName[..16];
    if (string.IsNullOrEmpty(tempName)) tempName = "Guest";
    GameManager.Instance.players.Add(connectionToClient.connectionId, new PlayerData(tempName, connectionToClient.connectionId));

    displayName = tempName;
    Message.HideMessage();
  }

  private void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiName.text = newName;
  }

  [ClientRpc]
  private void Rpc_UpdateDisplayName()
  {
    uiName.text = displayName;
    name = displayName;
  }
}
