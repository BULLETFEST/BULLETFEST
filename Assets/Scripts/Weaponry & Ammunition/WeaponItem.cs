using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponItem : NetworkBehaviour
{
  public string WeaponID;

  // private void OnTriggerEnter2D(Collider2D other)
  // {
  //   if (other.gameObject.tag != "Player") return;

  //   CanPickUp(other.gameObject.GetComponent<NetworkIdentity>().connectionToClient);
  // }

  // [TargetRpc]
  // void CanPickUp(NetworkConnection conn)
  // {

  // }

  // private void OnTriggerExit2D(Collider2D other)
  // {
  //   if (other.gameObject.tag != "Player") return;

  // }
}
