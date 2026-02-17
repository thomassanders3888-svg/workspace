 When player moves into a new zone, join group. When leaving a zone, leave group.

To implement the above functionalities in a PlayerHub.cs class using SignalR, you would follow these steps:

1. **Set up the PlayerHub class**: Inherit from `Hub` and define methods for handling connections, disconnections, player actions, and chat messages.
2. **Manage player groups for chunk zones**: Join or leave groups based on the player's movement.

Here is a sample implementation of the PlayerHub.cs class:

```csharp
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class PlayerHub : Hub
{
    // Dictionary to keep track of players and their current chunk zone group
    private static readonly ConcurrentDictionary<string, string> playerChunkZones = new ConcurrentDictionary<string, string>();

    // Method called when a player connects
    public override async Task OnConnectedAsync()
    {
        var playerId = Context.ConnectionId;
        await base.OnConnectedAsync();
        Console.WriteLine($"Player {playerId} connected.");
    }

    // Method called when a player disconnects
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var playerId = Context.ConnectionId;
        if (playerChunkZones.TryRemove(playerId, out _))
        {
            await Groups.RemoveFromGroupAsync(playerId, "DefaultZone"); // Remove from default zone if not in a specific one
        }
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Player {playerId} disconnected.");
    }

    // Method to handle player movement
    public async Task PlayerMove(string newChunkZone)
    {
        var playerId = Context.ConnectionId;

        // Remove from the old group if connected to any zone
        if (playerChunkZones.TryRemove(playerId, out var oldChunkZone))
        {
            await Groups.RemoveFromGroupAsync(playerId, oldChunkZone);
        }

        // Join new chunk zone group
        playerChunkZones[playerId] = newChunkZone;
        await Groups.AddToGroupAsync(playerId, newChunkZone);

        Console.WriteLine($"Player {playerId} moved to zone {newChunkZone}");
    }

    // Method to handle placing a block
    public async Task PlaceBlock(string blockType, int x, int y, int z)
    {
        var playerId = Context.ConnectionId;
        await Clients.Group(playerChunkZones[playerId]).SendAsync("ReceiveBlockPlace", playerId, blockType, x, y, z);
        Console.WriteLine($"Player {playerId} placed a {blockType} at ({x}, {y}, {z})");
    }

    // Method to handle breaking a block
    public async Task BreakBlock(int x, int y, int z)
    {
        var playerId = Context.ConnectionId;
        await Clients.Group(playerChunkZones[playerId]).SendAsync("ReceiveBlockBreak", playerId, x, y, z);
        Console.WriteLine($"Player {playerId} broke a block at ({x}, {y}, {z})");
    }

    // Method to handle chat messages
    public async Task SendChat(string message)
    {
        var playerId = Context.ConnectionId;
        await Clients.Group(playerChunkZones[playerId]).SendAsync("ReceiveChat", playerId, message);
        Console.WriteLine($"Player {playerId} sent a chat message: {message}");
    }
}
```

### Explanation:

1. **OnConnectedAsync and OnDisconnectedAsync**: These methods handle the connection and disconnection of players. When a player disconnects, they are removed from any chunk zone groups.
2. **PlayerMove**: This method updates the player's position and moves them to a new chunk zone group.
3. **PlaceBlock, BreakBlock, SendChat**: These methods broadcast actions to other players in the same chunk zone.

### Groups Management:

- **PlayerChunkZones**: A dictionary that keeps track of each player and their current chunk zone group.
- **Joining/Leaving Groups**: When a player moves, they are removed from their old group (if any) and added to their new group.

This setup ensures that actions like placing blocks or sending chat messages are only broadcasted to players in the same chunk zone.
