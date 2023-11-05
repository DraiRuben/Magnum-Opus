using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class FusingStation : MonoBehaviour
{
    [SerializeField] private List<FusionCell> m_cells;

    public void TryFusion()
    {
        if (m_cells.All(x => x.m_isFull))
        {
            foreach (var cell in m_cells)
            {
                cell.Fuse();
            }
        }
    }


}
