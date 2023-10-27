using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager instance;
    private List<ActionLine> Lines = new List<ActionLine>();

    [SerializeField] private GameObject LinePrefab;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public void RegisterNewActionnableObject(ObjectDraggable obj)
    {
        ActionLine comp = Instantiate(LinePrefab, transform).GetComponent<ActionLine>();
        Lines.Add(comp);
        comp.m_actionTarget = obj;
    }
    public void RemoveActionnableObject(ObjectDraggable obj)
    {
        ActionLine _toRemove = Lines.Find(x => x.m_actionTarget == obj);
        if (_toRemove != null)
        {
            Lines.Remove(_toRemove);
            // cool thing is that when the line gets destroyed,
            // the scroll rect automatically fills the empty space with lines below it
            Destroy(_toRemove);
        }
        UpdateLineNumber();
    }
    private void UpdateLineNumber()
    {
        for (int i = 0; i < Lines.Count; i++)
        {
            Lines[i].LineNumber = i + 1;
        }
    }
}
