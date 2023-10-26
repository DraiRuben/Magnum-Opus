using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class Draggable : Selectable, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int m_dragLayerOrder = 3; //if it's set to -1 that means we don't change the layer when dragging
    private static bool s_isSomethingDragging; //set as static so that other draggables immediatly get unselected when another interactable is selected
    protected Vector3 m_posBeforeDrag;
    protected Vector3 m_scaleBeforeDrag;
    protected Quaternion m_rotBeforeDrag;
    protected bool m_initialDrag = true;

    protected override bool GetIsInteractable()
    {
        return (m_canBeSelected && (!s_IsSomethingSelected||!s_isSomethingDragging))
            ||(s_IsSomethingSelected && m_isSelected);
    }

    protected override void Interact()
    {
        //resets everything selected if nothing was dragging but something still was selected & this wasn't still selected yet (e.g for multiselect)
        if(!m_isSelected && !s_isSomethingDragging && s_IsSomethingSelected)
        {
            MapManager.instance.m_unselectAll = true;
        }
        //starts dragging if it's initial input or drop if it's input release
        s_isSomethingDragging = !s_isSomethingDragging;
        m_isSelected = true;
        s_IsSomethingSelected = true;
        if (s_isSomethingDragging)
        {
            StartCoroutine(Drag());

        }
        else
        {
            TryDrop();
        }
    }
    //highlight
    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }
    //exit highlight
    public void OnPointerExit(PointerEventData eventData)
    {
    }
    protected IEnumerator Drag()
    {
        InventoryManager.instance.StopScroller(); //delays the method call
        transform.parent.parent = null;
        m_posBeforeDrag   = transform.parent.position;
        m_scaleBeforeDrag = transform.parent.localScale;
        m_rotBeforeDrag   = transform.parent.rotation;
        if (m_dragLayerOrder >= 0)
        {
            m_sprite.sortingOrder = m_dragLayerOrder;
        }
        //so that the center doesn't snap to the mouse
        Vector2 offset = transform.parent.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        while (s_isSomethingDragging)
        {
            transform.parent.position = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + offset;
            transform.parent.position += Vector3.back;  //offset z by 1 so that the raycast on mouse relase may hit this one first
            yield return null;
        }
    }
    protected abstract void TryDrop();

}
