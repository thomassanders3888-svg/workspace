using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraForge.Quests
{
    public class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }
        public List<Quest> ActiveQuests = new List<Quest>();
        public List<Quest> CompletedQuests = new List<Quest>();
        
        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestCompleted;
        public event Action<QuestObjective> OnObjectiveUpdated;
        
        void Awake() { Instance = this; }
        
        public void StartQuest(Quest quest)
        {
            if (!ActiveQuests.Exists(q => q.QuestId == quest.QuestId))
            {
                quest.State = QuestState.Active;
                ActiveQuests.Add(quest);
                OnQuestStarted?.Invoke(quest);
            }
        }
        
        public void UpdateObjective(string questId, string objectiveId, int amount)
        {
            if (ActiveQuests.Find(q => q.QuestId == questId) is Quest quest)
            {
                if (quest.Objectives.Find(o => o.ObjectiveId == objectiveId) is QuestObjective obj)
                {
                    obj.CurrentAmount = Mathf.Min(obj.CurrentAmount + amount, obj.RequiredAmount);
                    OnObjectiveUpdated?.Invoke(obj);
                    CheckQuestCompletion(quest);
                }
            }
        }
        
        void CheckQuestCompletion(Quest quest)
        {
            if (quest.Objectives.TrueForAll(o => o.IsComplete))
            {
                quest.State = QuestState.Completed;
                ActiveQuests.Remove(quest);
                CompletedQuests.Add(quest);
                OnQuestCompleted?.Invoke(quest);
            }
        }
    }
    
    [Serializable]
    public class Quest
    {
        public string QuestId, Title, Description;
        public QuestState State;
        public List<QuestObjective> Objectives;
        public List<QuestReward> Rewards;
    }
    
    [Serializable]
    public class QuestObjective
    {
        public string ObjectiveId, Description;
        public int RequiredAmount, CurrentAmount;
        public bool IsComplete => CurrentAmount >= RequiredAmount;
    }
    
    [Serializable]
    public class QuestReward
    {
        public ItemType ItemType;
        public int Quantity, Experience;
    }
    
    public enum QuestState { Inactive, Active, Completed, Failed }
}