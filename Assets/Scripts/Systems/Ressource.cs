using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private GameObject m_connector;
    [SerializeField] private bool m_isFusionRoot;
    public List<Ressource> m_fusedNodes = new();

    [HideInInspector] public bool m_isGrabbed = false;

    private List<GameObject> m_connectors = new();

    //checks recursively through the entire structure of fused connections to check if there already is one that's grabbed
    public void TryGetInvalidNodes(HashSet<Ressource> alreadyChecked, List<Ressource> problematicObjects, Ressource OriginNode)
    {
        foreach (Ressource connected in OriginNode.m_fusedNodes)
        {
            if (!alreadyChecked.Contains(connected))
            {
                alreadyChecked.Add(connected);
                if (connected.m_isGrabbed)
                {
                    problematicObjects.Add(connected);
                }
                connected.TryGetInvalidNodes(alreadyChecked, problematicObjects, connected);
            }
        }
    }
    public bool CanBeGrabbed
    {
        get
        {
            if (m_fusedNodes.Count > 0)
            {
                HashSet<Ressource> alreadyChecked = new(m_fusedNodes);
                List<Ressource> problematicObjects = m_fusedNodes.Where(x => x.m_isGrabbed).ToList();
                foreach (Ressource connected in m_fusedNodes)
                {
                    TryGetInvalidNodes(alreadyChecked, problematicObjects, connected);

                }
                if (problematicObjects.Count > 0)
                {
                    problematicObjects.Add(this);
                    ErrorManager.instance.RegisterMultiGrabException(problematicObjects);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (m_isGrabbed)
                {
                    ErrorManager.instance.RegisterMultiGrabException(new List<Ressource> { this });
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }
    }
    public void TryFuse(Ressource res2)
    {
        //fuse the 2 ressources
        if (!m_fusedNodes.Contains(res2) && transform.childCount <= 0)
        {
            m_fusedNodes.Add(res2);
            res2.m_fusedNodes.Add(this);

            float angle = Mathf.Atan2(transform.position.x - res2.transform.position.x, transform.position.y - res2.transform.position.y) * Mathf.Rad2Deg;
            GameObject connector = Instantiate(m_connector, transform.position, Quaternion.Euler(0, 0, angle), transform);
            m_connectors.Add(connector);
            res2.m_connectors.Add(connector);

        }
    }
    public void RearrangeFusionHierarchy()
    {
        if (m_fusedNodes.Count > 0) //rearrange hierarchy so that the entire fused thing pivots around this now
        {
            foreach (Ressource _toReparent in m_fusedNodes)
            {
                _toReparent.transform.parent = transform;
                _toReparent.m_isFusionRoot = false;
            }
            m_isFusionRoot = true;
        }
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
