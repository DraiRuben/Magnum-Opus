using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ActionDraggable : Draggable
{
    [SerializeField] private ObjectSlot slot;
    [SerializeField] private Order m_order;

    private bool m_isUsed = false;
    protected override IEnumerator TryDrop()
    {
        //finds if object was dropped out of bounds or not
        if (UIDragHelper.s_isOnUIElement)
        {
            if (UIDragHelper.s_lastUIElementSetter!=null) 
            {
                int cellIndex = ActionLine.GetCellIndex(UIDragHelper.s_lastUIElementSetter.gameObject);
                ActionLine line = ActionLine.GetLine(UIDragHelper.s_lastUIElementSetter.gameObject);
                //add new action in list
                if (UIDragHelper.s_lastUIElementSetter.gameObject.layer == LayerMask.NameToLayer("ActionSlot"))
                {
                    line.m_orders[cellIndex] = m_order;
                }
                //push all actions after this one
                else if (UIDragHelper.s_lastUIElementSetter.gameObject.layer == LayerMask.NameToLayer("Action"))
                {
                    line.PushElementsAtIndex(cellIndex,1);
                    line.m_orders.Insert(cellIndex+1, m_order);
                }

                transform.parent.parent = UIDragHelper.s_lastUIElementSetter.transform;
                transform.parent.localPosition = Vector3.zero;

                if (m_toActivateOnDrop != null) //sometimes we might want to activate things on drop like the arm editor
                {
                    foreach (GameObject _toActivate in m_toActivateOnDrop)
                    {
                        _toActivate.SetActive(true);
                    }
                }
                m_isUsed = true;
            }
            else // we dropped it on the UI but nowhere slottable
            {
                s_IsSomethingSelected = false;
                m_isSelected = false;
                m_isUsed = false;
                slot.TryReturnObject(this);
            }
        }
        else // we dropped it out of the UI
        {
            s_IsSomethingSelected = false;
            m_isSelected = false;
            m_isUsed = false;
            slot.TryReturnObject(this);
        }
        yield return null;
    }
    
}
public enum Order
{
    Empty,
    Reset,
    RotLeft,
    RotRight,
    PivotLeft,
    PivotRight,
    Extend,
    Retract,
    Plus,
    Minus
}