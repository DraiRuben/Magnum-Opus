using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSlot : MonoBehaviour
{
    [SerializeField] private bool m_canReplenish = true;
    [SerializeField] private GameObject m_objectPrefab;

    private bool m_isEmpty = false;
    public void UpdateContent(bool objectSelectionExit = false)
    {
        m_isEmpty = transform.childCount <= 0;
        if(objectSelectionExit && m_canReplenish)
        {
            Instantiate(m_objectPrefab,transform);
        }
    }
}
