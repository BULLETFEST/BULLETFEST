using Mirror;
using UnityEngine;

public class Gravestone : NetworkBehaviour
{
  [SerializeField] private GameObject dirt;
  [SerializeField] private ParticleSystem ps;

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
