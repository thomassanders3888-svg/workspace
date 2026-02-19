using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraForge.Admin
{
    /// <summary>
    /// Server command handler for admin operations
    /// </summary>
    public class ServerCommandHandler : MonoBehaviour
    {
        public static ServerCommandHandler Instance { get; private set; }
        
        private Dictionary<string, IConsoleCommand> commands = new Dictionary<string, IConsoleCommand>();
        
        void Awake()
        {
            Instance = this;
            RegisterDefaultCommands();
        }
        
        void RegisterDefaultCommands()
        {
            RegisterCommand(new KickCommand());
            RegisterCommand(new BanCommand());
            RegisterCommand(new TeleportCommand());
            RegisterCommand(new GiveCommand());
            RegisterCommand(new TimeCommand());
            RegisterCommand(new HelpCommand(this));
        }
        
        public void RegisterCommand(IConsoleCommand command)
        {
            commands[command.Name.ToLower()] = command;
            foreach (var alias in command.Aliases)
            {
                commands[alias.ToLower()] = command;
            }
        }
        
        public bool ExecuteCommand(string input, string executorId = null)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            var parts = input.Trim().Split(' ');
            var commandName = parts[0].ToLower();
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();
            
            if (commands.TryGetValue(commandName, out var command))
            {
                try
                {
                    return command.Execute(args, executorId);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Command] Error executing '{commandName}': {ex.Message}");
                    return false;
                }
            }
            
            Debug.Log($"[Command] Unknown command: {commandName}");
            return false;
        }
    }
    
    public interface IConsoleCommand
    {
        string Name { get; }
        string Description { get; }
        string[] Aliases { get; }
        bool Execute(string[] args, string executorId);
    }
    
    public class KickCommand : IConsoleCommand
    {
        public string Name => "kick";
        public string Description => "Kick a player from the server";
        public string[] Aliases => new[] { "k" };
        
        public bool Execute(string[] args, string executorId)
        {
            if (args.Length < 1) return false;
            var playerName = args[0];
            var reason = args.Length > 1 ? string.Join(" ", args[1..]) : "Kicked by admin";
            
            Debug.Log($"[Admin] Kicking player: {playerName}, Reason: {reason}");
            // NetworkManager.Instance.KickPlayer(playerName, reason);
            return true;
        }
    }
    
    public class BanCommand : IConsoleCommand
    {
        public string Name => "ban";
        public string Description => "Ban a player from the server";
        public string[] Aliases => new[] { "b" };
        
        public bool Execute(string[] args, string executorId)
        {
            if (args.Length < 1) return false;
            var playerName = args[0];
            var duration = args.Length > 1 ? args[1] : "permanent";
            
            Debug.Log($"[Admin] Banning player: {playerName}, Duration: {duration}");
            return true;
        }
    }
    
    public class TeleportCommand : IConsoleCommand
    {
        public string Name => "teleport";
        public string Description => "Teleport to coordinates or player";
        public string[] Aliases => new[] { "tp", "tele" };
        
        public bool Execute(string[] args, string executorId)
        {
            if (args.Length < 1) return false;
            
            if (float.TryParse(args[0], out float x))
            {
                // Coordinates
                if (args.Length < 3) return false;
                float y = float.Parse(args[1]);
                float z = float.Parse(args[2]);
                Debug.Log($"[Admin] Teleporting to: {x}, {y}, {z}");
            }
            else
            {
                // Player name
                Debug.Log($"[Admin] Teleporting to player: {args[0]}");
            }
            return true;
        }
    }
    
    public class GiveCommand : IConsoleCommand
    {
        public string Name => "give";
        public string Description => "Give items to a player";
        public string[] Aliases => new[] { "g", "item" };
        
        public bool Execute(string[] args, string executorId)
        {
            if (args.Length < 2) return false;
            var playerName = args[0];
            var itemName = args[1];
            int amount = args.Length > 2 && int.TryParse(args[2], out int a) ? a : 1;
            
            Debug.Log($"[Admin] Giving {amount}x {itemName} to {playerName}");
            return true;
        }
    }
    
    public class TimeCommand : IConsoleCommand
    {
        public string Name => "time";
        public string Description => "Set or get game time";
        public string[] Aliases => new[] { "t" };
        
        public bool Execute(string[] args, string executorId)
        {
            if (args.Length < 1)
            {
                Debug.Log("[Admin] Current time: [time]");
                return true;
            }
            
            switch (args[0].ToLower())
            {
                case "day":
                case "d":
                    Debug.Log("[Admin] Setting time to day");
                    break;
                case "night":
                case "n":
                    Debug.Log("[Admin] Setting time to night");
                    break;
                default:
                    if (int.TryParse(args[0], out int time))
                    {
                        Debug.Log($"[Admin] Setting time to: {time}");
                    }
                    break;
            }
            return true;
        }
    }
    
    public class HelpCommand : IConsoleCommand
    {
        private ServerCommandHandler handler;
        
        public HelpCommand(ServerCommandHandler h) { handler = h; }
        
        public string Name => "help";
        public string Description => "Show available commands";
        public string[] Aliases => new[] { "h", "?" };
        
        public bool Execute(string[] args, string executorId)
        {
            Debug.Log("=== Available Commands ===");
            Debug.Log("kick <player> [reason] - Kick a player");
            Debug.Log("ban <player> [duration] - Ban a player");
            Debug.Log("teleport <x> <y> <z> OR <player> - Teleport");
            Debug.Log("give <player> <item> [amount] - Give items");
            Debug.Log("time [day/night/hour] - Set time");
            Debug.Log("help - Show this help");
            return true;
        }
    }
}