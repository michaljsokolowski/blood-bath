using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public event System.Action<string> OnHealEnter;// gdy wejdzie sie w healing orb

    public event System.Action<Vector3> OnEnemyDeath;// death event
    public event System.Action OnEnemyHit;//
    public event System.Action<int> OnEvolutionChange; //  do zmiany prefabu gracza podczas ewolucji
    public event System.Action<SkillName, bool> OnSkillChange; // do wl i wyl umiejetnosci gracza

    public void HealEnter(string healType)
    {
        OnHealEnter?.Invoke(healType);
    }

    public void EnemyDied(Vector3 enemyPosition)
    {
        if (OnEnemyDeath != null)
            OnEnemyDeath.Invoke(enemyPosition);
    }

    public void EnemyHit()
    {
        OnEnemyHit.Invoke();
    }

    public void EvolutionChange(int evolutionIndex)//, Vector3 evolutionPosition
    {
        OnEvolutionChange.Invoke(evolutionIndex);//, evolutionPosition
    }
    public void SkillChange(SkillName skillName, bool skillState)
    {
        OnSkillChange.Invoke(skillName, skillState);
    }


}
