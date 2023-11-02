using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class Draggable : Selectable, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected int m_dragLayerOrder = 3; //if it's set to -1 that means we don't change the layer when dragging
    [SerializeField] protected List<GameObject> m_toActivateOnDrop;
    [SerializeField] protected List<layerModificationData> m_toChangeLayer;
    [SerializeField] protected Vector3 m_scaleOutOfUI = Vector3.one;
    [SerializeField] protected bool m_IsDraggable = true;
    public bool m_isInUI = true;
    public bool m_staySelectedOnDrop = false;


    protected static bool s_isSomethingDragging; //set as static so that other draggables immediatly get unselected when another interactable is selected
    protected Vector3 m_posBeforeDrag;
    protected Vector3 m_scaleBeforeDrag;
    protected Quaternion m_rotBeforeDrag;
    protected bool m_initialDrag = true;
    public List<GameObject> m_toActivateOnSelect;
    protected override bool GetIsInteractable()
    {
        return (m_canBeSelected && (!s_IsSomethingSelected || !s_isSomethingDragging))
            || (s_IsSomethingSelected && m_isSelected);
    }

    protected override void Interact()
    {
        //resets everything selected if nothing was dragging but something still was selected & this wasn't still selected yet (e.g for multiselect)
        if (!m_isSelected && !s_isSomethingDragging && s_IsSomethingSelected)
        {
            MapManager.instance.m_unselectAll = true;
            if (!m_IsDraggable) return; //if we aren't draggable we don't want to be able to drag it but still unselect everything
        }
        //starts dragging if it's initial input or drop if it's input release
        s_isSomethingDragging = !s_isSomethingDragging;
        m_isSelected = true;
        s_IsSomethingSelected = true;

        if (s_isSomethingDragging)
        {
            // sometimes we might want to enable things when selecting like a highlight or the hand
            if (m_toActivateOnSelect != null)
            {
                foreach (GameObject _toActivate in m_toActivateOnSelect)
                {
                    _toActivate.SetActive(true);
                }
            }
            StartCoroutine(Drag());
        }
        else
        {
            StartCoroutine(TryDrop());
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
    protected virtual IEnumerator Drag()
    {

        InventoryManager.instance.m_scroller.enabled = false;
        transform.parent.parent = null;
        transform.parent.localScale = m_scaleOutOfUI;
        SaveTransform();
        m_sprite.sortingOrder = m_dragLayerOrder;

        //so that the center doesn't snap to the mouse
        Vector2 offset = transform.parent.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        while (s_isSomethingDragging)
        {
            transform.parent.position = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + offset;
            transform.parent.position += 2 * Vector3.back;  //offset z by 2 so that the raycast on mouse relase may hit this one first
            yield return null;
        }
    }
    protected void SaveTransform()
    {
        m_posBeforeDrag = transform.parent.position;
        m_scaleBeforeDrag = transform.parent.localScale;
        m_rotBeforeDrag = transform.parent.rotation;
    }
    protected override void Unselect()
    {
        if (!m_staySelectedOnDrop)
        {
            base.Unselect();

            s_isSomethingDragging = false;
            if (!m_isInUI)
            {
                if (m_toActivateOnSelect != null)
                {
                    foreach (GameObject _toDisable in m_toActivateOnSelect)
                    {
                        _toDisable.SetActive(false);
                    }
                }
            }
        }
        else
        {
            m_staySelectedOnDrop = false;
        }

    }
    protected abstract IEnumerator TryDrop();
    [Serializable]
    public struct layerModificationData
    {
        public SpriteRenderer spriteRenderer;
        public int modifier;
    }
}
