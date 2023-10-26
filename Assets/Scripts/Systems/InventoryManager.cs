using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    [SerializeField] private ScrollRect m_scroller;
    public GameObject m_armPreviewer;
    private Vector3 m_poolPos = new(0, -50000, 0);
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ReturnToPool(GameObject _toReturn)
    {
        _toReturn.transform.position = m_poolPos;
        _toReturn.transform.parent = transform;
        _toReturn.SetActive(false);
    }
    public void StopScroller()
    {
        Invoke(nameof(StopScrollerDelayed), 0.1f); //calls method delayed
    }
    private void StopScrollerDelayed()
    {
        //creates fake data to stop dragging
        var forceEndDragData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left
        };
        m_scroller.OnEndDrag(forceEndDragData);
    }
}
