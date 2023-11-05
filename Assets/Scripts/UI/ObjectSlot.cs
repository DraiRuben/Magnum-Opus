using UnityEngine;

public class ObjectSlot : MonoBehaviour
{
    [SerializeField] private bool m_canReplenish = true;
    [SerializeField] private GameObject m_objectPrefab;
    [SerializeField] private int m_slotCount = 1;
    [SerializeField] private int m_slotMultiplier = 1; //for ressource slots that have both the slot and the ressource

    private void Start()
    {
        RegenObject();
    }
    public void TryRegenObject()
    {
        if (m_canReplenish && transform.parent.childCount < m_slotCount * m_slotMultiplier * 2)
        {
            RegenObject();
        }
    }
    private void RegenObject()
    {
        GameObject spawned = Instantiate(m_objectPrefab, transform.parent);

        SetSlot(spawned);
        Ressource comp = spawned.GetComponentInChildren<Ressource>();
        if (comp != null)
        {
            if (comp.m_fusedNodes.Count > 0)
            {
                ObjectDraggable[] list = spawned.GetComponentsInChildren<ObjectDraggable>();
                if (list.Length > 0)
                {
                    foreach (ObjectDraggable obj in list)
                    {
                        obj.m_slot = this;
                    }
                }
            }
        }
    }
    private void SetSlot(GameObject _toSet)
    {
        if (_toSet.transform.GetChild(0).TryGetComponent(out ActionDraggable actionDraggable))
        {
            actionDraggable.m_slot = this;
        }
        else if (_toSet.transform.GetChild(0).TryGetComponent(out ObjectDraggable objectDraggable))
        {
            objectDraggable.m_slot = this;
        }
    }
    public void TryReturnObject(Draggable obj)
    {
        // called on object drop out of bounds
        // so that it comes back to the slot instead of disappearing indefinitely
        if (transform.parent.childCount <= m_slotCount * m_slotMultiplier)
        {
            obj.transform.parent.parent = transform.parent;
            obj.transform.parent.localScale = Vector3.one;
            obj.transform.parent.localPosition = Vector3.zero;
            MapManager.instance.m_unselectAll = true;
            obj.m_isInUI = true;

        }
        else
        {

            Destroy(obj.transform.parent.gameObject);
        }
    }
}
