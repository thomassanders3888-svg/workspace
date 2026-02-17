using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueNode {
    public string text;
    public string npcName;
    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice {
    public string text;
    public int nextNodeId;
    public string requiredQuestId;
    public string completeQuestId;
}

public class NPCDialogue : MonoBehaviour {
    public string npcName;
    public DialogueNode[] dialogueNodes;
    public int startNodeId = 0;
    
    private int currentNodeId;
    
    public void StartDialogue() {
        currentNodeId = startNodeId;
        DisplayCurrentNode();
    }
    
    void DisplayCurrentNode() {
        var node = dialogueNodes[currentNodeId];
        Debug.Log($"{npcName}: {node.text}");
        
        for (int i = 0; i < node.choices.Length; i++) {
            Debug.Log($"[{i+1}] {node.choices[i].text}");
        }
    }
    
    public void SelectChoice(int choiceIndex) {
        var node = dialogueNodes[currentNodeId];
        if (choiceIndex < 0 || choiceIndex >= node.choices.Length) return;
        
        var choice = node.choices[choiceIndex];
        if (!string.IsNullOrEmpty(choice.completeQuestId)) {
            QuestSystem.Instance.UpdateObjective(choice.completeQuestId, Quest.objectiveType.Explore, 1);
        }
        
        currentNodeId = choice.nextNodeId;
        if (currentNodeId >= 0) DisplayCurrentNode();
        else EndDialogue();
    }
    
    void EndDialogue() {
        Debug.Log("Conversation ended.");
    }
}
