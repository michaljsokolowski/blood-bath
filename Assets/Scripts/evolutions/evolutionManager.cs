using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;


public enum SkillName {FirePower, AcidPower, ShadowPower, Wings, Evade, Blocking, DamageTypes} //enum wszystkich skillow
[System.Serializable]
public class EvolutionSkills
{
    public SkillName skillName;
    public bool isSkillActive; //czy aktywny
}
[System.Serializable]
public class Evolution
{
    public int evolutionIndex; //numer ewolucji mekka
    public int cameraDistance; //ustawienie dystansu kamery od mekka, im wieksza ewolucja tym dalej
    public List<EvolutionSkills> skills; //lista wszystkich skilli w evo

}


public class evolutionManager : MonoBehaviour
{
    public Evolution currentEvolution; //do latwiejszego szukania
    public List<Evolution> allEvolutions; //wszystkie dostepne evo

    private GameEvents events;
    private CinemachineFreeLook camComponent;
    private GameObject player;
    void Awake()
    {
        events = FindObjectOfType<GameEvents>();
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        FindPlayerParent(allPlayers);
    }

    private void OnEnable()
    {
        events.OnEvolutionChange += Evolve;
        events.OnSkillChange += ChangeSkills;
    }
    private void OnDisable()
    {
        events.OnEvolutionChange -= Evolve;
        events.OnSkillChange -= ChangeSkills;
    }
    private void Evolve(int index)
    {
        currentEvolution = allEvolutions[index];
        ChangePlayer();
    }

    private void ChangePlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");//potrzebne bo kilka rzeczy ma tag
        FindPlayerParent(allPlayers);

        ChangePlayerLook(player);

        //STARY POMYSŁ Z USUWANIEM PREFABÓW
        //Destroy(player);      
        //GameObject newPlayerPref = Instantiate(currentEvolution.playerPrefab, position, Quaternion.identity);

        ChangeCamera(player);

        Debug.Log("New Player spawned");
    }
    private void FindPlayerParent(GameObject[] objects) //do wyszukania konkretnego obiektu z tagiem player
    {
            foreach (GameObject obj in objects)
        {
            Transform parent = obj.transform.parent;
            bool hasPlayerParent = false;
            while (parent != null)
            {
                if (parent.CompareTag("Player"))
                {
                    hasPlayerParent = true;
                    break;
                }
                parent = parent.parent;
            }
            if (!hasPlayerParent)
            {
                player = obj;
                Debug.Log("Old Player found" + player.name);
                break;
            }
        }

        if (player == null)
        {
            Debug.LogError("Nie znaleziono głównego obiektu gracza!");
            return;
        }
    }
    private void ChangePlayerLook(GameObject player)
    {
        Debug.Log("player looks diefferent ig");
    }
    private void ChangeCamera(GameObject newPlayerPref)
    {
        GameObject camera = GameObject.FindGameObjectWithTag("Camera");
        camComponent = camera.GetComponent<CinemachineFreeLook>();

        camComponent.Follow = newPlayerPref.transform;
        camComponent.LookAt = newPlayerPref.transform;
        if (camComponent != null)
        {
            camComponent.m_Lens.FieldOfView = currentEvolution.cameraDistance;
        }
    }
    private void ChangeSkills(SkillName searchedSkillName, bool skillState)
    {
        for (int i = 0; i < currentEvolution.skills.Count; i++)
        {
            if (currentEvolution.skills[i].skillName == searchedSkillName)
            {

                switch (searchedSkillName)
                {
                    case SkillName.FirePower:
                        FirePower firePower = FindAnyObjectByType<FirePower>();
                        if(firePower != null)
                        {
                            firePower.enabled = skillState;
                            Debug.Log("skill changed");
                        }
                        else
                        {
                            Debug.Log("skill not changed");
                        }
                        currentEvolution.skills[i].isSkillActive = skillState;
                        break;
                    case SkillName.AcidPower:
                        AcidPower acid = FindAnyObjectByType<AcidPower>();
                        if(acid != null)
                        {
                            acid.enabled = skillState;
                        }
                        currentEvolution.skills[i].isSkillActive = skillState;
                        break;
                    case SkillName.ShadowPower:
                        Debug.Log("not made");
                        break;
                    case SkillName.Wings:
                        Debug.Log("not made");
                        break;
                    case SkillName.Evade:
                        PlayerEvade evade = FindAnyObjectByType<PlayerEvade>();
                        if(evade != null)
                        {
                            evade.enabled = skillState;
                        }
                        currentEvolution.skills[i].isSkillActive = skillState;
                        break;
                    case SkillName.Blocking:
                        Debug.Log("not made");
                        break;
                    case SkillName.DamageTypes:
                        Debug.Log("not made");
                        break;
                    default:
                        Debug.Log("not made");
                        break;
                }
            }
            
        }      
           
    }


}
