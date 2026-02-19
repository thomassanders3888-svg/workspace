using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraForge.Network
{
    /// <summary>
    /// Global chat system with channels and moderation
    /// </summary>
    public class ChatSystem : MonoBehaviour
    {
        public static ChatSystem Instance { get; private set; }
        
        [Header("Settings")]
        public int maxMessageHistory = 100;
        public float messageCooldown = 0.5f;
        public bool enableProfanityFilter = true;
        
        [Header("Channels")]
        public ChatChannel globalChannel;
        public ChatChannel whisperChannel;
        public ChatChannel tradeChannel;
        public ChatChannel guildChannel;
        
        // Events
        public event Action<ChatMessage> OnMessageReceived;
        public event Action<string, string> OnWhisperReceived;
        
        // State
        private List<ChatMessage> messageHistory = new List<ChatMessage>();
        private Dictionary<string, float> lastMessageTime = new Dictionary<string, float>();
        private HashSet<string> mutedPlayers = new HashSet<string>();
        private HashSet<string> profanityWords;
        
        void Awake()
        {
            Instance = this;
            InitializeProfanityFilter();
        }
        
        void InitializeProfanityFilter()
        {
            // Basic profanity list
            profanityWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "badword1", "badword2", "spam", "scam"
            };
        }
        
        /// <summary>
        /// Send a message to a channel
        /// </summary>
        public void SendMessage(string senderId, string senderName, string content, ChatChannel channel)
        {
            // Check cooldown
            if (lastMessageTime.TryGetValue(senderId, out float lastTime))
            {
                if (Time.time - lastTime < messageCooldown)
                {
                    SendSystemMessage(senderId, "Message sent too fast. Please slow down.");
                    return;
                }
            }
            
            // Check mute
            if (mutedPlayers.Contains(senderId))
            {
                SendSystemMessage(senderId, "You are muted.");
                return;
            }
            
            // Filter content
            string filteredContent = enableProfanityFilter ? FilterProfanity(content) : content;
            
            // Create message
            var message = new ChatMessage
            {
                SenderId = senderId,
                SenderName = senderName,
                Content = filteredContent,
                Channel = channel,
                Timestamp = DateTime.UtcNow,
                MessageId = Guid.NewGuid().ToString()
            };
            
            // Add to history
            messageHistory.Add(message);
            if (messageHistory.Count > maxMessageHistory)
                messageHistory.RemoveAt(0);
            
            // Update cooldown
            lastMessageTime[senderId] = Time.time;
            
            // Broadcast
            BroadcastMessage(message);
        }
        
        /// <summary>
        /// Send a whisper to specific player
        /// </summary>
        public void SendWhisper(string senderId, string senderName, string targetId, string content)
        {
            // Implementation would look up target player and send private message
            var message = new ChatMessage
            {
                SenderId = senderId,
                SenderName = senderName,
                Content = $"[Whisper] {content}",
                Channel = whisperChannel,
                Timestamp = DateTime.UtcNow
            };
            
            OnWhisperReceived?.Invoke(targetId, $"From {senderName}: {content}");
        }
        
        /// <summary>
        /// Send system message to player
        /// </summary>
        public void SendSystemMessage(string playerId, string message)
        {
            // Would route to specific player
            Debug.Log($"[System -> {playerId}]: {message}");
        }
        
        /// <summary>
        /// Broadcast to all players
        /// </summary>
        void BroadcastMessage(ChatMessage message)
        {
            OnMessageReceived?.Invoke(message);
            
            // Format and log
            string formatted = FormatMessage(message);
            Debug.Log(formatted);
        }
        
        string FormatMessage(ChatMessage msg)
        {
            return msg.Channel switch
            {
                ChatChannel.Global => $"[{msg.Timestamp:HH:mm}] [{msg.SenderName}]: {msg.Content}",
                ChatChannel.Trade => $"[{msg.Timestamp:HH:mm}] [Trade] [{msg.SenderName}]: {msg.Content}",
                ChatChannel.Guild => $"[{msg.Timestamp:HH:mm}] [Guild] [{msg.SenderName}]: {msg.Content}",
                ChatChannel.Whisper => $"[{msg.Timestamp:HH:mm}] [Whisper] [{msg.SenderName}]: {msg.Content}",
                _ => $"[{msg.SenderName}]: {msg.Content}"
            };
        }
        
        string FilterProfanity(string input)
        {
            string result = input;
            foreach (var word in profanityWords)
            {
                result = result.Replace(word, new string('*', word.Length), StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }
        
        /// <summary>
        /// Mute a player
        /// </summary>
        public void MutePlayer(string playerId, TimeSpan duration)
        {
            mutedPlayers.Add(playerId);
            // Could schedule unmute after duration
        }
        
        /// <summary>
        /// Get recent message history
        /// </summary>
        public IReadOnlyList<ChatMessage> GetHistory(int count = 50)
        {
            int start = Mathf.Max(0, messageHistory.Count - count);
            return messageHistory.GetRange(start, messageHistory.Count - start);
        }
        
        /// <summary>
        /// Clear chat history
        /// </summary>
        public void ClearHistory()
        {
            messageHistory.Clear();
        }
    }
    
    [Serializable]
    public struct ChatMessage
    {
        public string MessageId;
        public string SenderId;
        public string SenderName;
        public string Content;
        public ChatChannel Channel;
        public DateTime Timestamp;
    }
    
    public enum ChatChannel
    {
        Global,
        Trade,
        Guild,
        Whisper,
        System
    }
}