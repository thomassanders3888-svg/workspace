 

```csharp
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

public class Player
{
    public Guid PlayerId { get; private set; }
    public string Username { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public Dictionary<int, int> Inventory { get; set; } // Assuming item ID and quantity
    public DateTime LastActive { get; private set; }

    public Player(string username)
    {
        PlayerId = Guid.NewGuid();
        Username = username;
        Position = new Vector3(0, 0, 0);
        Velocity = new Vector3(0, 0, 0);
        Health = MaxHealth = 100f; // Default health and max health
        Inventory = new Dictionary<int, int>();
        LastActive = DateTime.Now;
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        Position = newPosition;
        LastActive = DateTime.Now;
    }

    public void UpdateVelocity(Vector3 newVelocity)
    {
        Velocity = newVelocity;
        LastActive = DateTime.Now;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0)
            Health = 0;
        LastActive = DateTime.Now;
    }

    public bool ValidateAction()
    {
        // Example validation logic: check if player is still active
        return DateTime.Now - LastActive <= TimeSpan.FromMinutes(5);
    }
}

public class GameServerHub : Hub
{
    private readonly Dictionary<Guid, Player> _players = new Dictionary<Guid, Player>();
    private readonly HashSet<string> _activeGroups = new HashSet<string>();

    public void JoinGroup(string groupName)
    {
        Groups.Add(Context.ConnectionId, groupName).Wait();
        if (!_activeGroups.Contains(groupName))
        {
            _activeGroups.Add(groupName);
        }
    }

    public void LeaveGroup(string groupName)
    {
        Groups.Remove(Context.ConnectionId, groupName).Wait();
        if (_activeGroups.Contains(groupName) && !Context.ConnectionIds.ContainsAny(_players.Keys.Select(p => p.ToString())))
        {
            _activeGroups.Remove(groupName);
        }
    }

    public Player GetPlayer(Guid playerId)
    {
        return _players.ContainsKey(playerId) ? _players[playerId] : null;
    }

    public List<Player> GetAllPlayers()
    {
        return new List<Player>(_players.Values);
    }

    public void UpdatePlayerPosition(Guid playerId, Vector3 newPosition)
    {
        if (_players.ContainsKey(playerId))
        {
            _players[playerId].UpdatePosition(newPosition);
        }
    }

    public void UpdatePlayerVelocity(Guid playerId, Vector3 newVelocity)
    {
        if (_players.ContainsKey(playerId))
        {
            _players
