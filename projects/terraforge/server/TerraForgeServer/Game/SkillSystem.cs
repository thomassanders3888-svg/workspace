using System;
using System.Collections.Generic;

namespace TerraForgeServer.Game
{
    public enum SkillType
    {
        Mining,
        Digging,
        Carpentry,
        Smithing
    }

    public class SkillSystem
    {
        public Dictionary<string, float> Skills { get; private set; }
        
        private const float SKILL_GAIN_BASE = 0.01f;
        private const float SKILL_SOFT_CAP = 70.0f;
        private const float SKILL_HARD_CAP = 100.0f;
        private const float DIFFICULTY_CURVE = 1.5f;

        public SkillSystem()
        {
            Skills = new Dictionary<string, float>();
            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                Skills[skill.ToString()] = 1.0f;
            }
        }

        public float GetSkill(SkillType skill)
        {
            string key = skill.ToString();
            return Skills.ContainsKey(key) ? Skills[key] : 1.0f;
        }

        public void SkillGain(SkillType skill, float difficulty)
        {
            string key = skill.ToString();
            float currentSkill = Skills[key];
            
            if (currentSkill >= SKILL_HARD_CAP)
                return;

            float gain = CalculateGain(currentSkill, difficulty);
            
            Skills[key] = Math.Min(currentSkill + gain, SKILL_HARD_CAP);
        }

        public async Task ProcessTickAsync()
    {
        // Process hourly skill ticks - decay or other time-based effects
        await Task.CompletedTask;
    }

    private float CalculateGain(float skill, float difficulty)
        {
            float skillDifference = Math.Abs(skill - difficulty);
            float multiplier = 1.0f;
            
            // Optimal difficulty is slightly above current skill
            if (skillDifference < 5.0f)
                multiplier = 1.5f;
            else if (skillDifference > 20.0f)
                multiplier = 0.1f;
            else if (skillDifference > 10.0f)
                multiplier = 0.5f;

            // Soft cap reduces gains
            if (skill > SKILL_SOFT_CAP)
            {
                float overCap = skill - SKILL_SOFT_CAP;
                multiplier /= (1.0f + overCap / DIFFICULTY_CURVE);
            }

            return SKILL_GAIN_BASE * multiplier;
        }

        public float CalculateQuality(SkillType skill, float baseQuality)
        {
            float level = GetSkill(skill);
            float variance = (float)new Random().NextDouble() * 0.2f - 0.1f;
            
            // Quality capped by skill level
            float maxQuality = Math.Min(baseQuality, level);
            float finalQuality = maxQuality * (1.0f + variance);
            
            return Math.Max(1.0f, Math.Min(100.0f, finalQuality));
        }

        public float CalculateQuality(SkillType primarySkill, SkillType toolSkill, float baseQuality, float toolQuality)
        {
            float primaryLevel = GetSkill(primarySkill);
            float toolLevel = GetSkill(toolSkill);
            
            // Average primary skill with tool quality and tool skill
            float effectiveSkill = (primaryLevel + toolQuality + toolLevel * 0.5f) / 2.5f;
            
            return CalculateQuality(baseQuality, effectiveSkill);
        }

        private float CalculateQuality(float baseQuality, float effectiveSkill)
        {
            float variance = (float)new Random().NextDouble() * 0.2f - 0.1f;
            float maxQuality = Math.Min(baseQuality, effectiveSkill);
            float finalQuality = maxQuality * (1.0f + variance);
            
            return Math.Max(1.0f, Math.Min(100.0f, finalQuality));
        }
    }
}
