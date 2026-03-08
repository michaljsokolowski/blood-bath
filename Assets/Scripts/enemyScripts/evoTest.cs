using UnityEngine;

public class evoTest : MonoBehaviour
{
    private GameEvents events;
    private bool canChange = true;
    [SerializeField] private bool isEvo;
    [SerializeField] private bool isSkillChange;
    [SerializeField] private bool isSkillActive;
    [SerializeField] private int evoIndex;
    [SerializeField] private SkillName skillname;


    void Start()
    {
        events = FindObjectOfType<GameEvents>();
        
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision is BoxCollider && collision.CompareTag("Player") && canChange && isEvo)
        {
            events.EvolutionChange(evoIndex);
            canChange = false;
        }
        else if (collision is BoxCollider && collision.CompareTag("Player") && canChange && isSkillChange)
        {
            events.SkillChange(skillname, isSkillActive);
            canChange = false;
        }


    }
    
}
