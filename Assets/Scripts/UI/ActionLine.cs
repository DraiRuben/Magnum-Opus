using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ActionLine : MonoBehaviour
{
    [HideInInspector] public ObjectDraggable m_actionTarget;
    [HideInInspector] public List<ActionDraggable> m_orders = new();

    [SerializeField] private TextMeshProUGUI m_lineIndexText;
    [SerializeField] private ActionButtonPrefabs m_actionsPrefabs;
    public static int s_longestActionLine = 0; //for cycle looping
    private int m_longestPersonalActionLine = 0;
    private static ActionLine s_longestLine;
    private int m_lineNumber;
    private void Start()
    {
        for (int i = 0; i < transform.GetChild(0).childCount - 1; i++)
        {
            m_orders.Add(null);
        }
        ExecutionControls.instance.UpdateLongestActionLine.AddListener(UpdateLength);
    }

    private void UpdateLength()
    {
        int highest = 0;
        for(int i = 0; i < m_orders.Count; i++)
        {
            if (m_orders[i] != null && i > highest)
                highest = i;
        }
        //if we found a higher length or if we changed the length of the higher one,
        //override with the new value even if it's lower
        if (highest > s_longestActionLine || s_longestLine == this) 
        {
            s_longestActionLine = highest;
            s_longestLine = this;
        }
        m_longestPersonalActionLine = highest;
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
    public void PushElementsAtIndex(int index, int amount)
    {
        int i = index;
        bool tryBackwards = false;
        //gets the index of the first element to overwrite (empty cell or the last cell if there's none)
        while (amount > 0)
        {
            if (m_orders[i] == null || m_orders[i].m_order == Order.Empty)
            {
                amount--;
                m_orders.RemoveAt(i);
                if(tryBackwards)
                {
                    for (int u = index + 1; u > i + 1; u--)
                    {
                        Transform actionDraggable = transform.GetChild(0).GetChild(u).GetChild(0);
                        actionDraggable.parent = transform.GetChild(0).GetChild(u-1);
                        actionDraggable.localPosition = Vector3.zero;
                    }
                }
                else
                {
                    for (int u = index + 1; u < i + 1; u++)
                    {
                        Transform actionDraggable = transform.GetChild(0).GetChild(u).GetChild(0);
                        actionDraggable.parent = transform.GetChild(0).GetChild(u + 1);
                        actionDraggable.localPosition = Vector3.zero;
                    }
                }
                
            }
            else
            {
                i += tryBackwards ? -1 : 1; //only increment if we don't remove an element since that would offset the actual position in the list by 2
            }
            if (!tryBackwards && i >= m_orders.Count)
            {//reverse pushing direction if we still need to push elements but don't have anything in front
                tryBackwards = true;
                i--;
            }
            else if (tryBackwards && i == 0)
            {
                //this is done only in the case of the line being entirely full and the player dropping a new order on it
                //what happens is the last(s) element(s) of the list get destroyed, one for each dropped order
                while (amount > 0)
                {
                    //no -1 since we need to ignore number displayer which counts in the childcount but not in the ordercount 
                    Destroy(transform.GetChild(0).GetChild(m_orders.Count).GetChild(0).gameObject);
                    m_orders.RemoveAt(m_orders.Count - 1);
                    amount--;
                    for (int u = index + 1; u < m_orders.Count+1; u++)
                    {
                        Transform actionDraggable = transform.GetChild(0).GetChild(u).GetChild(0);
                        actionDraggable.parent = transform.GetChild(0).GetChild(u + 1);
                        actionDraggable.localPosition = Vector3.zero;
                    }
                }

            }
        }
    }
    public static int GetCellIndex(GameObject cell)
    {// gets all children of the line and gets index of the cell we're looking for
        List<Transform> allChildren = cell.transform.parent.Cast<Transform>().Select(x => x.transform).ToList();
        return allChildren.IndexOf(cell.transform);
    }
    public static ActionLine GetLine(GameObject cell)
    {
        return cell.transform.parent.parent.GetComponent<ActionLine>();
    }
}
