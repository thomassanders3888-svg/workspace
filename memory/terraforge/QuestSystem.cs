// QuestSystem.cs - TerraForge Quest Management System
// Handles quest database, objectives, rewards, and progression tracking

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerraForge
{
    // ==================== ENUMS ====================

    /// <summary>
    /// Represents the current state of a quest
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QuestState
    {
        Locked,         // Quest is locked and unavailable
        Available,      // Quest can be started
        Active,         // Quest is currently being tracked
        Completed,      // Quest objectives fulfilled, awaiting turn-in
        TurnedIn,       // Quest finished and rewards claimed
        Failed          // Quest was failed or timed out
    }

    /// <summary>
    /// Types of quest objectives
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ObjectiveType
    {
        Gather,         // Collect items
        Kill,           // Defeat enemies
        Exploration,    // Visit locations
        Interact,       // Talk to NPCs or interact with objects
        Craft,          // Craft items
        Escort,         // Escort an NPC
        Defend,         // Defend a location or NPC
        Custom          // Custom objective handled by script
    }

    /// <summary>
    /// Types of quest rewards
    /// </summary>
    public enum RewardType
    {
        Experience,
        Gold,
        Item,
        Reputation,
        Ability,
        Title
    }

    // ==================== DATA CLASSES ====================

    /// <summary>
    /// Represents a single quest objective
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        [JsonPropertyName("type")]
        public ObjectiveType Type { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("targetId")]
        public string TargetId { get; set; } = string.Empty;

        [JsonPropertyName("requiredAmount")]
        public int RequiredAmount { get; set; } = 1;

        [JsonPropertyName("currentAmount")]
        public int CurrentAmount { get; set; } = 0;

        [JsonPropertyName("isOptional")]
        public bool IsOptional { get; set; } = false;

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [JsonPropertyName("customData")]
        public Dictionary<string, string> CustomData { get; set; } = new();

        /// <summary>
        /// Check if this objective is completed
        /// </summary>
        public bool CheckCompletion()
        {
            if (!IsCompleted)
            {
                IsCompleted = CurrentAmount >= RequiredAmount;
            }
            return IsCompleted;
        }

        /// <summary>
        /// Update current amount bounded by required amount
        /// </summary>
        public void UpdateProgress(int amount)
        {
            CurrentAmount = Math.Min(amount, RequiredAmount);
            CheckCompletion();
        }

        /// <summary>
        /// Add to current progress
        /// </summary>
        public void AddProgress(int amount)
        {
            CurrentAmount = Math.Min(CurrentAmount + amount, RequiredAmount);
            CheckCompletion();
        }

        /// <summary>
        /// Get objective progress as a float 0-1
        /// </summary>
        public float GetProgressPercent() => 
            RequiredAmount > 0 ? (float)CurrentAmount / RequiredAmount : 0f;

        public QuestObjective Clone()
        {
            return new QuestObjective
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = this.Type,
                Description = this.Description,
                TargetId = this.TargetId,
                RequiredAmount = this.RequiredAmount,
                CurrentAmount = 0,
                IsOptional = this.IsOptional,
                IsCompleted = false,
                CustomData = new Dictionary<string, string>(this.CustomData)
            };
        }
    }

    /// <summary>
    /// Represents a quest reward
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        [JsonPropertyName("type")]
        public RewardType Type { get; set; }

        [JsonPropertyName("itemId")]
        public string? ItemId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; } = 0;

        [JsonPropertyName("customId")]
        public string? CustomId { get; set; }

        [JsonPropertyName("given")]
        public bool Given { get; set; } = false;

        public QuestReward() { }

        public QuestReward(RewardType type, int amount)
        {
            Type = type;
            Amount = amount;
        }

        public QuestReward(RewardType type, string itemId, int amount = 1)
        {
            Type = type;
            ItemId = itemId;
            Amount = amount;
        }
    }

    /// <summary>
    /// Quest definition template
    /// </summary>
    [Serializable]
    public class QuestTemplate
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("levelRequired")]
        public int LevelRequired { get; set; } = 1;

        [JsonPropertyName("prerequisites")]
        public List<string> Prerequisites { get; set; } = new();

        [JsonPropertyName("objectives")]
        public List<QuestObjective> Objectives { get; set; } = new();

        [JsonPropertyName("rewards")]
        public List<QuestReward> Rewards { get; set; } = new();

        [JsonPropertyName("category")]
        public string Category { get; set; } = "Main";

        [JsonPropertyName("isRepeatable")]
        public bool IsRepeatable { get; set; } = false;

        [JsonPropertyName("timeLimit")]
        public float TimeLimit { get; set; } = 0f; // 0 = no limit

        /// <summary>
        /// Create an instance of this quest for a player
        /// </summary>
        public QuestInstance CreateInstance(string playerId)
        {
            return new QuestInstance
            {
                QuestId = this.Id,
                PlayerId = playerId,
                State = QuestState.Active,
                Objectives = this.Objectives.Select(o => o.Clone()).ToList(),
                Rewards = this.Rewards.Select(r => new QuestReward 
                { 
                    Type = r.Type, 
                    ItemId = r.ItemId, 
                    Amount = r.Amount,
                    CustomId = r.CustomId
                }).ToList(),
                StartTime = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Active/in-progress quest instance
    /// </summary>
    [Serializable]
    public class QuestInstance
    {
        [JsonPropertyName("instanceId")]
        public string InstanceId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("questId")]
        public string QuestId { get; set; } = string.Empty;

        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; } = string.Empty;

        [Json