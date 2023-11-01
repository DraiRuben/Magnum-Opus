using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager instance;
    [HideInInspector] public List<ActionLine> Lines = new List<ActionLine>();
    [HideInInspector] public UnityEvent UpdateLongestActionLine = new();

    [SerializeField] private GameObject LinePrefab;
    [SerializeField] private Button m_playButton;
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
        comp.LineNumber = Lines.Count;
        if (Lines.Count > 0)
        {
            //enable play button since we now have a line
            ExecutionControls.instance.SetPlayButtonsInteractable(true);
        }
    }
    public void RemoveActionnableObject(ObjectDraggable obj)
    {
        ActionLine _toRemove = Lines.Find(x => x.m_actionTarget == obj);
        if (_toRemove != null)
        {
            Lines.Remove(_toRemove);
            // cool thing is that when the line gets destroyed,
            // the scroll rect automatically fills the empty space with lines below it
            Destroy(_toRemove.gameObject);
        }
        UpdateLineNumber();
        if (Lines.Count <= 0)
        {
            //disable play button if we don't have any line to use
            ExecutionControls.instance.SetPlayButtonsInteractable(false);
        }
    }
    private void UpdateLineNumber()
    {
        for (int i = 0; i < Lines.Count; i++)
        {
            Lines[i].LineNumber = i + 1;
        }
    }
}
