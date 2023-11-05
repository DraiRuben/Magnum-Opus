using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionCell : MonoBehaviour
{
    [HideInInspector] public bool m_isFull;
    [SerializeField] private FusingStation m_station;
    [SerializeField] private List<FusionCell> fuseWith = new();
    [HideInInspector] Ressource m_fillingRessource;
    public void FillFusionStation(Ressource _filler)
    {
        m_fillingRessource = _filler;
        m_isFull = true;
        m_station.TryFusion();
    }
    public void Fuse()
    {
        foreach(FusionCell cell in fuseWith)
        {
            m_fillingRessource.TryFuse(cell.m_fillingRessource);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        var comp = collision.gameObject.GetComponent<Ressource>();
        if ( comp != null && comp == m_fillingRessource)
        {
            m_isFull = false;
        }
    }
}
