using Mirror;

public class PlayerNetworking : NetworkBehaviour
{
  private PlayerVars playerVars;

  public override void OnStartClient()
  {
    base.OnStartClient();
    if (isLocalPlayer)
    {
      gameObject.layer = 30;
    }

    playerVars = GetComponent<PlayerVars>();

    // string playerName = PlayerPrefs.GetString("PlayerName", "Guest");
    // ClientRpc_InitializePlayer(playerName);
  }

  [ClientRpc]
  private void ClientRpc_InitializePlayer(string playerName)
  {
    playerVars.uiName.text = playerName;
  }

}
