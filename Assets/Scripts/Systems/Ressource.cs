using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private GameObject m_connector;
    [SerializeField] private bool m_isFusionRoot;
    [SerializeField] private List<Ressource> FusionNodes = new();

    [HideInInspector] public bool m_isGrabbed = false;

    private List<GameObject> m_connectors = new();
    private void Start()
    {
        if (FusionNodes.Count > 0)
        {
            foreach (Ressource fusionNode in FusionNodes)
            {
                TryFuse(fusionNode);
            }
        }
    }
    public bool CanBeGrabbed
    {
        get
        {
            if (FusionNodes.Count > 0)
            {
                List<Ressource> problematicObjects = FusionNodes.Where(x => x.m_isGrabbed).ToList();
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
        if (!FusionNodes.Contains(res2))
        {
            FusionNodes.Add(res2);
            res2.FusionNodes.Add(this);

            float angle = Mathf.Atan2(transform.position.x - res2.transform.position.x, transform.position.y - res2.transform.position.y) * Mathf.Rad2Deg;
            GameObject connector = Instantiate(m_connector, transform.position, Quaternion.Euler(0, 0, angle), transform);
            m_connectors.Add(connector);
            res2.m_connectors.Add(connector);

        }
    }
    public void RearrangeFusionHierarchy()
    {
        if(!m_isFusionRoot && FusionNodes.Count>0) //rearrange hierarchy so that the entire fused thing pivots around this now
        {
            foreach(Ressource _toReparent in FusionNodes)
            {
                _toReparent.transform.parent = transform;
                _toReparent.m_isFusionRoot = false;
            }
            m_isFusionRoot =true;
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
