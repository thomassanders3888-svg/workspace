 I am using the Steamworks SDK and Unity3D. Here is my code:

```csharp
using UnityEngine;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    private bool isInLobby = false;

    public void CreateLobby()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager not initialized.");
            return;
        }

        CSteamID lobbyId;
        SteamLobbyAPI.CreateLobby(CSteamID.Nil, ELobbyType.PRIVATE, 4, out lobbyId);
        isInLobby = true;

        Debug.Log("Created lobby with ID: " + lobbyId.ConvertToUint64());
    }

    public void JoinLobby()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager not initialized.");
            return;
        }

        // Assuming we have a lobby to join
        CSteamID lobbyId = new CSteamID(1234567890); // Replace with actual lobby ID

        SteamLobbyAPI.JoinLobby(lobbyId);
        isInLobby = true;

        Debug.Log("Joined lobby with ID: " + lobbyId.ConvertToUint64());
    }

    public void LeaveLobby()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager not initialized.");
            return;
        }

        SteamLobbyAPI.LeaveLobby();
        isInLobby = false;

        Debug.Log("Left lobby.");
    }

    public void GetLobbyList()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager not initialized.");
            return;
        }

        int maxResultsReturned = 10;
        SteamLobbyAPI.GetlobbyList(maxResultsReturned);
    }
}
```

What should I add to make the code work properly with callbacks? How can I handle the callbacks for lobby creation, joining, and leaving?
<|im_start|>'t add callback handling directly in this code. Callbacks are handled by Steamworks API internally, and you need to implement a class that inherits from `SteamLobbyAPI.LobbyCallbacks_t` and override the methods to receive notifications.

Here's an example of how you can create a lobby callbacks class:

```csharp
using Steamworks;

public class LobbyCallbacks : SteamLobbyAPI.LobbyCallbacks_t
{
    public override void OnLobbyCreated(CSteamID lobbyId, ELobbyError error)
    {
        Debug.Log("Lobby created with ID: " + lobbyId.ConvertToUint64() + ", Error: " + error);
    }

    public override void OnLobbyEntered(ELobbyEnterResponse response)
    {
        if (response == ELobbyEnterResponse.k_ELobbyEnterSuccess)
            Debug.Log("Successfully entered the lobby.");
        else
            Debug.LogError("Failed to enter the lobby. Response: " + response);
    }

    public override void OnLobbyLeft()
    {
        Debug.Log("Left the lobby.");
    }
}
```

Then, in your `SteamLobby` class, you can instantiate and register this callbacks object:

```csharp
public class SteamLobby : MonoBehaviour
{
    private LobbyCallbacks lobbyCallbacks;

    // ... rest of the code ...

    public void CreateLobby()
    {
        // ... existing code ...
        lobbyCallbacks = new LobbyCallbacks();
        SteamLobbyAPI.RegisterLobbyCallback(lobbyCallbacks);
    }

    public void JoinLobby()
    {
        // ... existing code ...
        lobbyCallbacks = new LobbyCallbacks();
        SteamLobbyAPI.RegisterLobbyCallback(lobbyCallbacks);
    }

    public void LeaveLobby()
    {
        // ... existing code ...
        SteamLobbyAPI.UnregisterLobbyCallback(lobbyCallbacks);
    }
}
```

Remember to unregister the callbacks when they're no longer needed. This will prevent memory leaks and ensure that your callbacks are only active during the time you need them.

Also, make sure that `SteamManager.Initialize()` is called in a proper place (e.g., from a script that starts before any lobby operations) so that the Steamworks API is ready to handle callbacks.
