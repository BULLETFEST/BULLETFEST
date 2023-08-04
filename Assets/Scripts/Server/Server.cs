using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Server : NetworkBehaviour
{
  PlayerSpawnSystem system;

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    if (!isServer)
    {
      // Destroy(GetComponent<PlayerVars>().publicCanvas.gameObject);
      Destroy(GetComponent<Server>());
    }
    if (GameManager.settings.gameMode == GameSettings.GameMode.Deathmatch)
    {
      StartCoroutine(CalcTimeLeft());
    }

    system = FindObjectOfType<PlayerSpawnSystem>();
  }

  [ClientRpc]
  public void PlaySoundAll(string sound, bool interrupt, bool varyPitch)
  {
    AudioSystem.Instance.PlaySound(sound, interrupt, varyPitch);
  }


  [ClientRpc]
  public void PlaySoundAll(string sound)
  {
    AudioSystem.Instance.PlaySound(sound);
  }

  private IEnumerator CalcTimeLeft()
  {
    TimeSpan timeSpan = new(0, 5, 0);
    while (timeSpan.TotalSeconds >= 0)
    {
      timeSpan = system.timeStamp.Subtract(DateTime.UtcNow);
      UpdateTimer($"{timeSpan.Minutes}:{(timeSpan.Seconds < 10 ? "0" + timeSpan.Seconds.ToString() : timeSpan.Seconds)}");

      yield return new WaitForSeconds(1);
    }

    GameManager.Instance.AnnounceWinner(null, false);
  }

  [Command(requiresAuthority = false)]
  private void UpdateTimer(string timeString)
  {
    foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values.ToArray())
    {
      Rpc_UpdateTimer(conn, timeString);
    }
  }

  [TargetRpc]
  private void Rpc_UpdateTimer(NetworkConnection conn, string timeString)
  {
    gameObject.GetComponent<PlayerUI>().uiTimeLeft.text = timeString;
  }
}
