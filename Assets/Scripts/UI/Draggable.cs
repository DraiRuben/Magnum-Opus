using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Draggable : Selectable,IPointerEnterHandler,IPointerExitHandler
{
    protected override bool GetIsInteractable()
    {
        return m_canBeSelected && !s_IsSomethingSelected;
    }

    protected override void Interact()
    {
        m_isSelected = !m_isSelected;
        if (m_isSelected)
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
    private IEnumerator Drag()
    {
        while (m_isSelected)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            yield return null;
        }
    }
    protected abstract void TryDrop();

}
