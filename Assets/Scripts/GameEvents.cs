
public static class GameEvents
{
    public static event System.Action<ComboSystem.DamageType, int, ComboSystem.StatusEffect> OnComboExecuted;

    public static void RaiseComboExecuted(ComboSystem.DamageType damageType,
                                          int totalDamage,
                                          ComboSystem.StatusEffect statusEffect)
    {
        OnComboExecuted?.Invoke(damageType, totalDamage, statusEffect);
    }
}