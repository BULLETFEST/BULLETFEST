using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Server : NetworkBehaviour
{
  private Coroutine timerRoutine;

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    if (!isServer)
    {
      // Destroy(GetComponent<PlayerVars>().publicCanvas.gameObject);
      Destroy(GetComponent<Server>());
    }
    if (MyNetworkManager.instance.settings.gameMode == GameSettings.GameMode.Deathmatch)
    {
      timerRoutine = StartCoroutine(CalcTimeLeft());
    }
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
      timeSpan = FindObjectOfType<PlayerSpawnSystem>().timeStamp.Subtract(DateTime.UtcNow);//FindObjectOfType<PlayerSpawnSystem>().timeStamp.Subtract(DateTime.Now);
      // int secondsLeft = (int)timeSpan.Minutes;
      // uiTimeLeft.text = $"{Mathf.Floor(secondsLeft / 60)}:{Mathf.Floor(secondsLeft / Mathf.Floor(secondsLeft / 60))}";
      Cmd_UpdateTimer($"{timeSpan.Minutes}:{(timeSpan.Seconds < 10 ? "0" + timeSpan.Seconds.ToString() : timeSpan.Seconds)}");

      yield return new WaitForSeconds(1);
    }

    GameManager.Instance.AnnounceWinner(null, false);
  }

  [Command(requiresAuthority = false)]
  private void Cmd_UpdateTimer(string timeString)
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
