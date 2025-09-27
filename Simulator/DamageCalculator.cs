using System;
using System.Linq;

public static class DamageCalculator
{
    private static readonly Random random = new Random();

    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static float CalculateOutgoingDamage(
        float baseDamage, 
        float criticalChance, 
        float criticalDamageBonus, 
        float monsterDefenseRate, 
        float playerIgnoreDefenseRate)
    {
        float finalBaseDamage = baseDamage;
        bool isCritical = random.NextDouble() < criticalChance;
        if (isCritical)
        {
            finalBaseDamage *= (2.0f + criticalDamageBonus);
        }
        float effectiveMonsterDefenseRate = monsterDefenseRate * (1f - playerIgnoreDefenseRate);
        float finalDamageReductionRate = Clamp(effectiveMonsterDefenseRate / 100f, 0f, 1f);
        float outgoingFinalDamage = finalBaseDamage * (1f - finalDamageReductionRate);
        float randomModifier = (float)random.NextDouble() * (1.05f - 0.95f) + 0.95f;
        outgoingFinalDamage *= randomModifier;

        return outgoingFinalDamage;
    }
}