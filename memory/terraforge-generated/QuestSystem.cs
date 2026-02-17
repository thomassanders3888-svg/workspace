using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewQuest", menuName = "TerraForge/Quest")]
public class Quest : ScriptableObject {
    public string questId;
    public string title;
    public string description;
    public Objective[] objectives;
    public Reward reward;
    public bool isCompleted;
    public bool isActive;
    
    [Serializable]
    public class Objective {
        public string description;
        public int targetAmount;
        public int currentAmount;
        public objectiveType type;
        
        public enum objectiveType { Gather, Kill, Explore, Craft }
        
        public void UpdateProgress(int amount) {
            currentAmount = Mathf.Min(currentAmount + amount, targetAmount);
        }
        
        public bool IsComplete => currentAmount >= targetAmount;
    }
    
    [Serializable]
    public class Reward {
        public int experience;
        public ResourceStack[] items;
    }
    
    public void CheckCompletion() {
        isCompleted = true;
        foreach (var obj in objectives) {
            if (!obj.IsComplete) { isCompleted = false; break; }
        }
    }
}

public class QuestSystem : MonoBehaviour {
    public static QuestSystem Instance { get; private set; }
    public System.Collections.Generic.List<Quest> activeQuests = new();
    
    void Awake() { Instance = this; }
    
    public void AddQuest(Quest quest) {
        quest.isActive = true;
        activeQuests.Add(quest);
    }
    
    public void UpdateObjective(string questId, Quest.objectiveType type, int amount) {
        var quest = activeQuests.Find(q => q.questId == questId && q.isActive);
        if (quest == null) return;
        
        foreach (var obj in quest.objectives) {
            if (obj.type == type && !obj.IsComplete) {
                obj.UpdateProgress(amount);
                break;
            }
        }
        quest.CheckCompletion();
    }
}
