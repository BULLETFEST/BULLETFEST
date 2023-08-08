using EpicTransport;
using System;
using Epic.OnlineServices.P2P;

public class CustomEOSClient : Client
{
  public Action onConnectionFailed;

  public CustomEOSClient(EosTransport transport) : base(transport)
  {

  }

  protected override void OnConnectionFailed(Epic.OnlineServices.ProductUserId remoteId)
  {
    base.OnConnectionFailed(remoteId);
    onConnectionFailed?.Invoke();
  }

  public static new CustomEOSClient CreateClient(EosTransport transport, string host)
  {
    CustomEOSClient c = new(transport)
    {
      hostAddress = host,
      socketId = new SocketId() { SocketName = RandomString.Generate(20) }
    };

    c.OnConnected += () => transport.OnClientConnected.Invoke();
    c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
    c.OnReceivedData += (data, channel) => transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), channel);

    return c;
  }
}
