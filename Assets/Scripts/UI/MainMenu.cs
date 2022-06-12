using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using ParrelSync;
public class MainMenu : MonoBehaviour
{
  NetworkManager nm;

  [HideInInspector]
  public string addr;

  [HideInInspector]
  public string playerName;

  void Start()
  {
    nm = FindObjectOfType<NetworkManager>();

    addr = PlayerPrefs.GetString("Address", "localhost");
    playerName = PlayerPrefs.GetString("PlayerName", "Guest");


    if (nm.isNetworkActive)
    {

      nm.StopHost();
      nm.StopServer();
      nm.StopClient();
    }

#if UNITY_EDITOR
    if (ClonesManager.IsClone())
    {
      ChangeName("B");
      Connect();
    }
    else
    {
      ChangeName("A");
      Host();
    }
#endif

  }

  public void InitializeGame()
  {
    nm.networkAddress = addr;
    PlayerPrefs.SetString("PlayerName", playerName);
    PlayerPrefs.SetString("Address", addr);
  }

  public void Connect()
  {
    InitializeGame();
    nm.StartClient();
  }
  public void Host()
  {
    InitializeGame();
    nm.StartHost();
  }


  public void ChangeName(string newPlayerName)
  {
    playerName = newPlayerName;
  }

  public void ChangeAddr(string newAddr)
  {
    addr = newAddr;
  }
}
