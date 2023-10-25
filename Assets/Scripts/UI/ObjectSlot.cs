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

    public void UpdateContent(bool objectSelectionExit = false)
    {
        if(objectSelectionExit && m_canReplenish)
        {
            Instantiate(m_objectPrefab,transform);
        }
    }
    public void TryReturnObject(ObjectDraggable obj)
    {
        // called on object drop out of bounds
        // so that it comes back to the slot instead of disappearing indefinitely
        if (transform.childCount <= 0) 
        {
            obj.transform.parent.parent = transform;
            obj.transform.parent.localPosition = Vector3.zero;
            obj.GetComponent<SpriteRenderer>().sortingOrder = m_sprite.sortingOrder + 1;
        }
        else
        {
            Destroy(obj.transform.parent.gameObject);
        }
    }
}
