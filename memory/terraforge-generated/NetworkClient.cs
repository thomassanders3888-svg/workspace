 

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

public class NetworkClient : MonoBehaviour
{
    private HubConnection connection;
    
    public delegate void ConnectedEventHandler();
    public event ConnectedEventHandler OnConnected;
    
    public delegate void DisconnectedEventHandler(string reason);
    public event DisconnectedEventHandler OnDisconnected;
    
    public enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected
    }
    
    private ConnectionState currentState;

    async Task Start()
    {
        connection = new HubConnectionBuilder()
            .WithUrl("https://yourserver.com/hub")
            .Build();
        
        connection.OnConnected += () =>
        {
            OnConnected?.Invoke();
            currentState = ConnectionState.Connected;
        };

        connection.OnDisconnected((reason) =>
        {
            OnDisconnected?.Invoke(reason);
            currentState = ConnectionState.Disconnected;
        });

        await ConnectAsync();
    }
    
    public async Task ConnectAsync()
    {
        try
        {
            await connection.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect: " + ex.Message);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await connection.StopAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to disconnect: " + ex.Message);
        }
    }

    public async Task SendPlayerPosition(Vector3 pos)
    {
        try
        {
            await connection.InvokeAsync("SendPlayerPosition", pos);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to send player position: " + ex.Message);
        }
    }

    public void ReceiveChunkData(Action<byte[]> callback)
    {
        connection.On("ReceiveChunkData", (byte[] data) => 
        {
            callback(data);
        });
    }

    public async Task ReconnectAsync()
    {
        try
        {
            currentState = ConnectionState.Disconnecting;
            await connection.StopAsync();
            
            int delay = 1000; // initial delay in ms
            while (currentState != ConnectionState.Connected)
            {
                currentState = ConnectionState.Connecting;
                try
                {
                    await ConnectAsync();
                    currentState = ConnectionState.Connected;
                }
                catch (Exception ex)
                {
                    Debug.LogError("Reconnection failed: " + ex.Message);
                    await Task.Delay(delay);
                    delay *= 2; // double the delay for next attempt
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to reconnect: " + ex.Message);
        }
    }
}
```
