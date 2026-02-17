using Steamworks;
using UnityEngine;

public class SteamAchievements : MonoBehaviour {
    public static SteamAchievements Instance { get; private set; }
    
    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (SteamManager.IsInitialized) {
            Debug.Log("Steam Achievements initialized");
        }
    }
    
    // Achievement definitions
    public enum AchievementID {
        FIRST_BLOCK,
        FIRST_KILL,
        EXPLORER,
        MASTER_MINER,
        ARCHITECT,
        SURVIVOR,
        TRADER,
        LEGEND
    }
    
    // Achievement data
    private readonly (string id, string name, string desc)[] achievementData = {
        ("ACH_FIRST_BLOCK", "First Steps", "Place your first block"),
        ("ACH_FIRST_KILL", "Hunter", "Defeat your first enemy"),
        ("ACH_EXPLORER", "Explorer", "Travel 1000 blocks from spawn"),
        ("ACH_MASTER_MINER", "Master Miner", "Mine 1000 blocks"),
        ("ACH_ARCHITECT", "Architect", "Build a structure with 1000 blocks"),
        ("ACH_SURVIVOR", "Survivor", "Survive 10 days"),
        ("ACH_TRADER", "Trader", "Complete 50 trades"),
        ("ACH_LEGEND", "Legend", "Reach level 50")
    };
    
    public void UnlockAchievement(AchievementID id) {
        if (!SteamManager.IsInitialized) return;
        
        var achievement = achievementData[(int)id];
        SteamUserStats.SetAchievement(achievement.id);
        SteamUserStats.StoreStats();
        
        Debug.Log($"Achievement unlocked: {achievement.name}");
        
        // Trigger UI notification
        UIManager.Instance?.ShowNotification($"Achievement: {achievement.name}");
    }
    
    public void SetProgress(AchievementID id, int current, int target) {
        if (!SteamManager.IsInitialized) return;
        
        var achievement = achievementData[(int)id];
        float percent = (float)current / target * 100f;
        
        SteamUserStats.SetAchievementProgress(achievement.id, current, target);
        SteamUserStats.StoreStats();
    }
    
    public void ResetAllAchievements() {
        if (!SteamManager.IsInitialized) return;
        
        SteamUserStats.ResetAll(true);
        SteamUserStats.StoreStats();
        Debug.Log("All achievements reset");
    }
    
    // Specific unlock methods for hooks
    public void OnBlockPlaced() => UnlockAchievement(AchievementID.FIRST_BLOCK);
    public void OnEnemyKilled() => UnlockAchievement(AchievementID.FIRST_KILL);
    public void OnDistanceTraveled(float distance) {
        SetProgress(AchievementID.EXPLORER, (int)distance, 1000);
    }
}
