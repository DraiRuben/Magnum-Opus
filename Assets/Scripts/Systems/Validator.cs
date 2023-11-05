using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Validator : MonoBehaviour
{
    [SerializeField] List<GameObject> ValidatorCells;

    public void TryValidate(Ressource origin, List<Ressource> connectedRessources)
    {
        HashSet<Ressource> alreadyChecked = new HashSet<Ressource>();
        if (connectedRessources.Count == 0 )
        {
            if (ValidatorCells.Count == 1 && Vector3.Distance(origin.transform.position, ValidatorCells[0].transform.position) <= 1f)
            {
                //TODO :add score
                Destroy(origin.gameObject);
            }
        }
        //don't try to validate something that has more connections than the validatorCells count, to avoid wasting some computing power
        else if (connectedRessources.Count + 1 <= ValidatorCells.Count) //+1 since we need to account for the origin ressource too
        {
            //if all connected aren't close enough to a validator cell
            if (!connectedRessources.All(a => ValidatorCells.First(b => Vector3.Distance(a.transform.position, b.transform.position) <= 1f) != null))
                return;

            GameObject originCell = ValidatorCells.First(x => Vector3.Distance(x.transform.position, origin.transform.position) <= 1f);
            if (originCell != null)
            {
                bool result = true;
                CompareConnections(connectedRessources, alreadyChecked, ref result);
                if (result)
                {
                    //TODO: add score
                    // since the origin is the parent gameobject, the whole fused thing gets destroyed
                    Destroy(origin.gameObject);
                }
            }
            else return;
        }
    }
    private void CompareConnections(List<Ressource> connections, HashSet<Ressource> alreadyChecked, ref bool result)
    {
        foreach (var connection in connections)
        {
            if (!alreadyChecked.Contains(connection))
            {
                bool localResult = ValidatorCells.First(x => Vector3.Distance(connection.transform.position, x.transform.position) <= 1f) != null;
                // only set result if we are true, meaning if at least one check is false, the bool will stay false even if a later check is true
                if (result)
                    result = localResult;

                alreadyChecked.Add(connection);
                CompareConnections(connections, alreadyChecked, ref result);
            }
        }
    }
}
