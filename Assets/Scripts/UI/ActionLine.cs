using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ActionLine : MonoBehaviour
{
    [HideInInspector] public ObjectDraggable m_actionTarget;
    [HideInInspector] public List<Order> m_orders;

    [SerializeField] private TextMeshProUGUI m_lineIndexText;
    private int m_lineNumber;
    private void Start()
    {
        m_orders = new List<Order>(transform.childCount - 1); //-1 since we don't count the number displayer
    }
    public int LineNumber
    {
        get
        {
            return m_lineNumber;
        }
        set
        {
            m_lineNumber = value;
            m_lineIndexText.text = m_lineNumber.ToString();
        }
    }
    public void PushElementsAtIndex(int index,int amount)
    {
        int i = index;
        bool tryBackwards = false;
        //gets the index of the first element to overwrite (empty cell or the last cell if there's none)
        while (amount>0) 
        {
            if (m_orders[i] == Order.Empty)
            {
                amount--;
                m_orders.RemoveAt(i);
            }
            else
            {
                i+= tryBackwards?-1:1; //only increment if we don't remove an element since that would offset the actual position in the list by 2
            }
            if(!tryBackwards && i == m_orders.Count - 1)
            {//reverse pushing direction if we still need to push elements but don't have anything in front
                tryBackwards = true; 
            }
            else if (tryBackwards && i == 0)
            {
                //this is done only in the case of the line being entirely full and the player dropping a new order on it
                //what happens is the last(s) element(s) of the list get destroyed, one for each dropped order
                while (amount > 0)
                {
                    //no -1 since we need to ignore number displayer which counts in the childcount but not in the ordercount 
                    Destroy(transform.GetChild(m_orders.Count).gameObject);
                    m_orders.RemoveAt(m_orders.Count-1);

                }

            }
        }
    }
    public static int GetCellIndex(GameObject cell)
    {// gets all children of the line and gets index of the cell we're looking for
        var allChildren = cell.transform.parent.Cast<Transform>().Select(x => x.transform).ToList();
        return allChildren.IndexOf(cell.transform);
    }
    public static ActionLine GetLine(GameObject cell)
    {
        return cell.transform.parent.GetComponent<ActionLine>();
    }
}