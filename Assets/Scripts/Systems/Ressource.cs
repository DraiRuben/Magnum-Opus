using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private GameObject m_connector;
    [SerializeField] public List<FusionData> m_data;
    private List<GameObject> m_connectors = new();
    public void Fuse(GameObject res1, GameObject res2)
    {
        //fuse the 2 ressources
        var data = m_data.Where(x => x.OriginNode == res1 || x.OriginNode == res2).ToList();
        if (data.Count == 1)
        {
            //add the missing one to data
        }
        else
        {
            //add both to data
        }
        //don't do anything if count ==2 since we already have data on both, this means the connector is already instantiated
    }

    public void FuseNodes()
    {
        foreach(FusionData data in m_data)
        {
            foreach(GameObject targetNode in data.FusionNodes)
            {
                float angle = Mathf.Atan2(transform.position.x - targetNode.transform.position.x, transform.position.y - targetNode.transform.position.y) * Mathf.Rad2Deg;
                m_connectors.Add(Instantiate(m_connector,transform.position,Quaternion.Euler(0,0,angle), transform));
            }
        }
    }
    [Serializable]
    public struct FusionData
    {

        public GameObject OriginNode;
        public List<GameObject> FusionNodes;
    }
    private static List<Vector3Int> s_CloseTiles = new()
    {
        new Vector3Int(1,0,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(0,1,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(-1,1,0),
        new Vector3Int(-1,-1,0),

    };
}
