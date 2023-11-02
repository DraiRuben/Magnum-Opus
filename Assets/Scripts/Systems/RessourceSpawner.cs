using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RessourceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_ressourcePrefab;
    public List<RessourceSpawner> m_fusedSpawners;
    private void Start()
    {
        //fuck unity for doing this crap
    }
    private void TryRegenRessource(List<bool> returnValues, HashSet<RessourceSpawner> alreadyChecked)
    {
        //check for the colliders of all ressource spawners of this fused element

        foreach (RessourceSpawner connected in m_fusedSpawners)
        {
            if (!alreadyChecked.Contains(connected))
            {
                alreadyChecked.Add(connected);
                ContactFilter2D filter = new ContactFilter2D();
                filter.useTriggers = true;
                List<Collider2D> results = new();
                Physics2D.OverlapCollider(connected.GetComponent<Collider2D>(), filter, results);
                List<Collider2D> FilteredResult = results.Where(x => x.CompareTag("ArmGrabbable")).ToList();

                returnValues.Add(FilteredResult.Count <= 0);
            }
        }
        //if nothing is overlapping the list is full of true so we can then regenerate the ressource
        if (returnValues.All(x => x))
        {
            RegenRessource();
            foreach (RessourceSpawner ressourceSpawner in m_fusedSpawners)
            {
                ressourceSpawner.RegenRessource();
            }
        }
    }
    public void RegenRessource()
    {
        Instantiate(m_ressourcePrefab, transform.position, Quaternion.identity, transform.parent);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (ExecutionControls.instance.m_isPlaying && collision.collider.GetComponent<Ressource>() != null)
        {
            HashSet<RessourceSpawner> alreadyChecked = new() { this };
            List<bool> ReturnValues = new();
            //a ressource exited the tile we are on, so we need to try to generate a new ressource
            TryRegenRessource(ReturnValues, alreadyChecked);
        }
    }
}
