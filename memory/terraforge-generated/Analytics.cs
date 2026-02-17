using System;
using System.Collections.Generic;

public class Analytics {
    private static Analytics _instance;
    public static Analytics Instance => _instance ??= new Analytics();
    
    private Dictionary<string, int> _eventCounts = new();
    private Dictionary<string, float> _sessionTimes = new();
    private DateTime _sessionStart;
    
    public void StartSession() {
        _sessionStart = DateTime.UtcNow;
    }
    
    public void EndSession(string playerId) {
        var duration = (DateTime.UtcNow - _sessionStart).TotalSeconds;
        _sessionTimes[playerId] = (float)duration;
    }
    
    public void TrackEvent(string eventName, Dictionary<string, object> properties = null) {
        _eventCounts[eventName] = _eventCounts.GetValueOrDefault(eventName) + 1;
    }
    
    public void TrackBlockPlaced(string blockType) => TrackEvent("block_placed", new() { { "type", blockType } });
    public void TrackBlockRemoved(string blockType) => TrackEvent("block_removed", new() { { "type", blockType } });
    public void TrackDeath(string cause) => TrackEvent("player_death", new() { { "cause", cause } });
    public void TrackQuestCompleted(string questId) => TrackEvent("quest_completed", new() { { "quest", questId } });
    public void TrackTrade(string itemName, int amount) => TrackEvent("trade", new() { { "item", itemName }, { "amount", amount } });
    
    public Dictionary<string, int> GetEventCounts() => new(_eventCounts);
    public float GetAverageSessionTime() {
        if (_sessionTimes.Count == 0) return 0;
        float total = 0;
        foreach (var time in _sessionTimes.Values) total += time;
        return total / _sessionTimes.Count;
    }
}
