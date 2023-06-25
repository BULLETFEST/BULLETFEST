using Mirror;
using UnityEngine;

public class Gravestone : NetworkBehaviour
{
  public GameObject dirt;
  public ParticleSystem ps;

  [SyncVar]
  private bool played = false;

  [Command(requiresAuthority = false)]
  public void Collided()
  {
    if (played)
    {
      return;
    }

    played = true;
    Rpc_Collided();
  }

  [ClientRpc]
  private void Rpc_Collided()
  {
    dirt.SetActive(true);
    ps.Play();
  }
}
