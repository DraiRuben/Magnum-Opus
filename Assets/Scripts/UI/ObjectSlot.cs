using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSlot : MonoBehaviour
{
    [SerializeField] private bool m_canReplenish = true;
    [SerializeField] private GameObject m_objectPrefab;

    private SpriteRenderer m_sprite;
    private void Awake()
    {
        m_sprite = GetComponent<SpriteRenderer>();
    }

    public void TryRegenObject()
    {
        if(m_canReplenish)
        {
            Instantiate(m_objectPrefab,transform);
        }
    }
    public void TryReturnObject(Draggable obj)
    {
        // called on object drop out of bounds
        // so that it comes back to the slot instead of disappearing indefinitely
        if (transform.childCount <= 0) 
        {
            obj.transform.parent.parent = transform.parent;
            obj.transform.parent.localScale = Vector3.one;
            obj.transform.parent.localPosition = Vector3.zero;
            obj.GetComponent<SpriteRenderer>().sortingOrder = m_sprite.sortingOrder + 1;
            MapManager.instance.m_unselectAll = true;
            obj.m_isInUI = true;
            
        }
        else
        {

            Destroy(obj.transform.parent.gameObject);
        }
    }
}
