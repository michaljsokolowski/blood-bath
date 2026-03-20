using UnityEngine;

public static class GameEvents
{
    public static event System.Action<ComboSystem.DamageType, int, ComboSystem.StatusEffect, GameObject> OnComboExecuted;

    public static void RaiseComboExecuted(ComboSystem.DamageType damageType,
                                          int totalDamage,
                                          ComboSystem.StatusEffect statusEffect, GameObject? target)
    {
        OnComboExecuted?.Invoke(damageType, totalDamage, statusEffect, target);
    }

    public static event System.Action<string> OnAbilityUsed;

    public static void RaiseAbilityUsed(string text)
    {
        OnAbilityUsed?.Invoke(text);
    }
}