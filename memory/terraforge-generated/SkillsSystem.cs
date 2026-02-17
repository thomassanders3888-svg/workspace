using UnityEngine;

public class SkillsSystem : MonoBehaviour {
    public static SkillsSystem Instance { get; private set; }
    
    public Skill Mining = new Skill("Mining");
    public Skill Woodcutting = new Skill("Woodcutting");
    public Skill Crafting = new Skill("Crafting");
    public Skill Combat = new Skill("Combat");
    
    void Awake() { Instance = this; }
    
    [System.Serializable]
    public class Skill {
        public string name;
        public int experience;
        public int Level => Mathf.FloorToInt(Mathf.Sqrt(experience / 100)) + 1;
        
        public int XPToNext => (Level * Level * 100) - experience;
        public float Progress => (float)(experience - ((Level - 1) * (Level - 1) * 100)) / (XPToNext + ((Level - 1) * (Level - 1) * 100));
        
        public Skill(string n) { name = n; }
        
        public void AddXP(int xp) {
            experience += xp;
            int newLevel = Level;
            if (XPToNext <= 0) {
                Debug.Log($"{name} leveled up to {newLevel}!");
            }
        }
    }
}
