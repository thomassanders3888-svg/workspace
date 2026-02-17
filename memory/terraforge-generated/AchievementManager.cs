using UnityEngine;
using System.Collections.Generic;

[System.Serializable] public class Achievement { public string id; public string title; public bool unlocked; }

public class AchievementManager : MonoBehaviour {
    public static AchievementManager Instance;
    public List<Achievement> achievements = new();
    
    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        LoadAchievements();
    }
    
    public void Unlock(string id) {
        var a = achievements.Find(x => x.id == id);
        if (a != null && !a.unlocked) { a.unlocked = true; SaveAchievements(); Debug.Log("Achievement: " + a.title); }
    }
    
    void SaveAchievements() {
        foreach (var a in achievements) PlayerPrefs.SetInt("ach_" + a.id, a.unlocked ? 1 : 0);
    }
    
    void LoadAchievements() {
        foreach (var a in achievements) a.unlocked = PlayerPrefs.GetInt("ach_" + a.id, 0) == 1;
    }
}
