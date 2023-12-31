using System.Collections;
using UnityEngine;

public class ActionDraggable : Draggable
{
    public ObjectSlot m_slot;
    public Order m_order;

    private ActionLine m_line;
    protected override IEnumerator TryDrop()
    {

        //finds if object was dropped out of bounds or not
        if (UIDragHelper.s_isOnUIElement)
        {
            if (UIDragHelper.s_lastUIElementSetter != null
                && UIDragHelper.s_lastUIElementSetter.transform.parent.parent != null
                && UIDragHelper.s_lastUIElementSetter.transform.parent.parent.GetComponent<ActionLine>() != null)
            {
                int cellIndex = ActionLine.GetCellIndex(UIDragHelper.s_lastUIElementSetter.gameObject);
                m_line = ActionLine.GetLine(UIDragHelper.s_lastUIElementSetter.gameObject);
                //add new action in list
                if (UIDragHelper.s_lastUIElementSetter.gameObject.layer == LayerMask.NameToLayer("ActionSlot"))
                {
                    gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    RaycastHit2D Hit = Physics2D.GetRayIntersection
                        (
                        Camera.main.ScreenPointToRay
                        (
                            Camera.main.WorldToScreenPoint(UIDragHelper.s_lastUIElementSetter.GetComponent<RectTransform>().transform.position)),
                            float.PositiveInfinity,
                            LayerMask.GetMask("Mechanisms", "Action")
                        );
                    if (Hit.collider != null && Hit.collider.gameObject.layer == LayerMask.NameToLayer("Action"))
                    {
                        m_line.PushElementsAtIndex(cellIndex - 1, 1);
                        m_line.m_orders.Insert(cellIndex - 1, this);
                    }
                    else
                    {
                        m_line.m_orders[cellIndex - 1] = this;
                    }
                    gameObject.layer = LayerMask.NameToLayer("Action");
                }
                transform.parent.parent = UIDragHelper.s_lastUIElementSetter.transform;
                transform.parent.localPosition = Vector3.zero;

                if (m_toActivateOnDrop != null) //sometimes we might want to activate things on drop like a highlight
                {
                    foreach (GameObject _toActivate in m_toActivateOnDrop)
                    {
                        _toActivate.SetActive(true);
                    }
                }
                m_slot.TryRegenObject();
            }
            else // we dropped it on the UI but nowhere slottable
            {
                s_IsSomethingSelected = false;
                m_isSelected = false;
                m_slot.TryReturnObject(this);
            }
        }
        else // we dropped it out of the UI
        {
            s_IsSomethingSelected = false;
            m_isSelected = false;
            m_slot.TryReturnObject(this);
        }
        yield return null;
    }
    protected override IEnumerator Drag()
    {

        if (m_line != null)
        {
            m_line.m_orders[m_line.m_orders.IndexOf(this)] = null;
        }

        return base.Drag();

    }
    protected override void Unselect()
    {
        if (!m_staySelectedOnDrop)
        {
            base.Unselect();

            s_isSomethingDragging = false;

            if (m_toActivateOnSelect != null)
            {
                foreach (GameObject _toDisable in m_toActivateOnSelect)
                {
                    _toDisable.SetActive(false);
                }
            }

        }
        else
        {
            m_staySelectedOnDrop = false;
        }

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
    Minus,
    Grab,
    Drop
}