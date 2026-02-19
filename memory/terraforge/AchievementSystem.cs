using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TerraForge.Progression
{
    /// <summary>
    /// Achievement tracking and rewards system
    /// </summary>
    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }
        
        [Header("Achievement Database")]
        public List<AchievementDefinition> achievementDatabase;
        
        // Events
        public event Action<AchievementDefinition> OnAchievementUnlocked;
        public event Action<AchievementDefinition, float> OnAchievementProgress;
        
        // Player data
        private Dictionary<string, AchievementProgress> playerAchievements = new Dictionary<string, AchievementProgress>();
        
        void Awake()
        {
            Instance = this;
            LoadAchievementDatabase();
        }
        
        void LoadAchievementDatabase()
        {
            achievementDatabase = new List<AchievementDefinition>
            {
                // Explorer achievements
                new AchievementDefinition
                {
                    Id = "explorer_1",
                    Title = "First Steps",
                    Description = "Travel 100 blocks from spawn",
                    Category = AchievementCategory.Explorer,
                    MaxProgress = 100,
                    Reward = new AchievementReward { Experience = 50, Gold = 10 },
                    Icon = "🚶"
                },
                new AchievementDefinition
                {
                    Id = "explorer_10",
                    Title = "Wanderer",
                    Description = "Travel 1000 blocks from spawn",
                    Category = AchievementCategory.Explorer,
                    MaxProgress = 1000,
                    Prerequisites = new List<string> { "explorer_1" },
                    Reward = new AchievementReward { Experience = 200, Gold = 50 },
                    Icon = "🚶"
                },
                
                // Builder achievements
                new AchievementDefinition
                {
                    Id = "builder_1",
                    Title = "Novice Builder",
                    Description = "Place 100 blocks",
                    Category = AchievementCategory.Builder,
                    MaxProgress = 100,
                    Reward = new AchievementReward { Experience = 50, Gold = 10 },
                    Icon = "🏗️"
                },
                new AchievementDefinition
                {
                    Id = "builder_100",
                    Title = "Master Builder",
                    Description = "Place 10,000 blocks",
                    Category = AchievementCategory.Builder,
                    MaxProgress = 10000,
                    Prerequisites = new List<string> { "builder_1" },
                    Reward = new AchievementReward { Experience = 1000, Gold = 500, Title = "Architect" },
                    Icon = "🏗️"
                },
                
                // Miner achievements
                new AchievementDefinition
                {
                    Id = "miner_stone",
                    Title = "Stone Collector",
                    Description = "Mine 1000 stone blocks",
                    Category = AchievementCategory.Miner,
                    MaxProgress = 1000,
                    TargetItem = "stone",
                    Reward = new AchievementReward { Experience = 100, Gold = 25 },
                    Icon = "⛏️"
                },
                new AchievementDefinition
                {
                    Id = "miner_diamond",
                    Title = "Diamonds!",
                    Description = "Find your first diamond",
                    Category = AchievementCategory.Miner,
                    MaxProgress = 1,
                    TargetItem = "diamond",
                    Reward = new AchievementReward { Experience = 500, Gold = 100, Item = "diamond_pickaxe", ItemCount = 1 },
                    Icon = "💎"
                },
                
                // Crafting achievements
                new AchievementDefinition
                {
                    Id = "craft_1",
                    Title = "Beginner Crafter",
                    Description = "Craft 50 items",
                    Category = AchievementCategory.Crafter,
                    MaxProgress = 50,
                    Reward = new AchievementReward { Experience = 50 },
                    Icon = "🔨"
                },
                new AchievementDefinition
                {
                    Id = "craft_1000",
                    Title = "Factory Worker",
                    Description = "Craft 1000 items",
                    Category = AchievementCategory.Crafter,
                    MaxProgress = 1000,
                    Prerequisites = new List<string> { "craft_1" },
                    Reward = new AchievementReward { Experience = 500, Gold = 200 },
                    Icon = "🔨"
                },
                
                // Combat achievements
                new AchievementDefinition
                {
                    Id = "combat_1",
                    Title = "First Blood",
                    Description = "Defeat your first enemy",
                    Category = AchievementCategory.Combat,
                    MaxProgress = 1,
                    Reward = new AchievementReward { Experience = 100 },
                    Icon = "⚔️"
                },
                new AchievementDefinition
                {
                    Id = "combat_100",
                    Title = "Warrior",
                    Description = "Defeat 100 enemies",
                    Category = AchievementCategory.Combat,
                    MaxProgress = 100,
                    Prerequisites = new List<string> { "combat_1" },
                    Reward = new AchievementReward { Experience = 1000, Gold = 500, Title = "Warrior" },
                    Icon = "⚔️"
                },
                
                // Survival achievements
                new AchievementDefinition
                {
                    Id = "survival_1d",
                    Title = "Survivor",
                    Description = "Survive 1 day",
                    Category = AchievementCategory.Survival,
                    MaxProgress = 1,
                    Reward = new AchievementReward { Experience = 100 },
                    Icon = "🌙"
                },
                new AchievementDefinition
                {
                    Id = "survival_7d",
                    Title = "Week Survivor",
                    Description = "Survive 7 days",
                    Category = AchievementCategory.Survival,
                    MaxProgress = 7,
                    Prerequisites = new List<string> { "survival_1d" },
                    Reward = new AchievementReward { Experience = 500, Title = "Survivor" },
                    Icon = "🌙"
                }
            };
        }
        
        /// <summary>
        /// Initialize achievements for a player
        /// </summary>
        public void InitializePlayer(string playerId)
        {
            if (!playerAchievements.ContainsKey(playerId))
            {
                playerAchievements[playerId] = new AchievementProgress();
            }
        }
        
        /// <summary>
        /// Update achievement progress
        /// </summary>
        public void UpdateProgress(string playerId, string achievementId, float progress)
        {
            InitializePlayer(playerId);
