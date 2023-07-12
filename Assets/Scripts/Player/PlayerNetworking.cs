using Mirror;

public class PlayerNetworking : NetworkBehaviour
{
  private PlayerRefs playerRefs;

  public override void OnStartClient()
  {
    base.OnStartClient();
    if (isLocalPlayer)
    {
      gameObject.layer = 30;
    }

    playerRefs = GetComponent<PlayerRefs>();

    // string playerName = PlayerPrefs.GetString("PlayerName", "Guest");
    // ClientRpc_InitializePlayer(playerName);
  }

  [ClientRpc]
  private void ClientRpc_InitializePlayer(string playerName)
  {
    playerRefs.uiName.text = playerName;
  }

}
