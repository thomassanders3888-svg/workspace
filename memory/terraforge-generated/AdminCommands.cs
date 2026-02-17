using System;

public class AdminCommands {
    private readonly TerraForgeHub _hub;
    
    public AdminCommands(TerraForgeHub hub) { _hub = hub; }
    
    public string Execute(string command, Player player) {
        if (!player.IsAdmin) return "Access denied.";
        
        var parts = command.Split(' ');
        var cmd = parts[0].ToLower();
        var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();
        
        return cmd switch {
            "kick" => KickPlayer(args, player),
            "ban" => BanPlayer(args, player),
            "give" => GiveItem(args, player),
            "teleport" => Teleport(args, player),
            "weather" => SetWeather(args),
            "time" => SetTime(args),
            "spawn" => SpawnEntity(args, player),
            "kill" => KillEntity(args, player),
            "broadcast" => BroadcastMessage(args),
            "save" => SaveWorld(),
            "reload" => "Config reloaded.",
            "help" => GetHelp(),
            _ => "Unknown command. Type 'help' for commands."
        };
    }
    
    string KickPlayer(string[] args, Player admin) {
        if (args.Length == 0) return "Usage: kick <player> [reason]";
        return $"Kicked player {args[0]}.";
    }
    
    string BanPlayer(string[] args, Player admin) {
        if (args.Length < 2) return "Usage: ban <player> <duration_hours> [reason]";
        return $"Banned {args[0]} for {args[1]} hours.";
    }
    
    string GiveItem(string[] args, Player admin) {
        if (args.Length < 2) return "Usage: give <item_id> <amount> [player]";
        return $"Gave {args[1]}x {args[0]}.";
    }
    
    string Teleport(string[] args, Player admin) {
        if (args.Length == 0) return "Usage: teleport <player/to <x> <y> <z>";
        return "Teleported.";
    }
    
    string SetWeather(string[] args) {
        if (args.Length == 0) return "Valid: clear rain snow storm";
        return $"Weather set to {args[0]}.";
    }
    
    string SetTime(string[] args) {
        if (args.Length == 0) return "Usage: time <0-24000>";
        return $"Time set to {args[0]}.";
    }
    
    string SpawnEntity(string[] args, Player admin) {
        if (args.Length == 0) return "Usage: spawn <entity_type> [count]";
        return $"Spawned {args.Length > 1 ? args[1] : "1"} {args[0]}.";
    }
    
    string KillEntity(string[] args, Player admin) {
        return "Killed entities.";
    }
    
    string BroadcastMessage(string[] args) {
        if (args.Length == 0) return "Usage: broadcast <message>";
        var msg = string.Join(" ", args);
        _hub.BroadcastMessage("[Server] " + msg);
        return "Broadcast sent.";
    }
    
    string SaveWorld() {
        return "World saved.";
    }
    
    string GetHelp() {
        return @"Admin Commands:
-kick <player> [reason]
-ban <player> <hours> [reason]
-give <item> <amount> [player]
-teleport <player/to <x> <y> <z>
-weather <clear/rain/snow/storm>
-time <0-24000>
-spawn <entity> [count]
-kill [radius]
-broadcast <message>
-save
-reload
-help";
    }
}
