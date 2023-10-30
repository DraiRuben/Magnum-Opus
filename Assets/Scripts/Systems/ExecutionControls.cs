using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ExecutionControls : MonoBehaviour
{
    public static ExecutionControls instance;
    [HideInInspector] public UnityEvent UpdateLongestActionLine = new();
    [SerializeField] private GameObject m_cycleHighlight;
    private bool m_isPlaying = false;
    private bool m_stepByStep = false;
    private bool m_nextStep = false;

    private int m_currentCycle = 0;
    private Coroutine m_playRoutine;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public void NextStep()
    {
        if (!m_stepByStep)
        {
            //only do it once when clicking on the button
            MapManager.instance.m_unselectAll = true;
        }
        m_stepByStep = true;
        m_nextStep = true;
        m_playRoutine ??= StartCoroutine(PlayRoutine());
    }
    public void Play()
    {
        //TODO: call function to fill in empty spaces between instructions
        m_stepByStep = false;
        MapManager.instance.m_unselectAll = true;
        m_playRoutine??= StartCoroutine(PlayRoutine());
    }
    public void Stop()
    {
        //destroy all cloned ressources & filler empty actions, need to make a class to allow ressources to be picked up & an automatic function in actionline that fills up the lines
        m_currentCycle = 0;
        m_cycleHighlight.SetActive(false);
        m_isPlaying = false;

    }
    private IEnumerator PlayRoutine()
    {
        m_isPlaying = true;
        m_cycleHighlight.SetActive(true);
        while (m_isPlaying)
        {
            m_cycleHighlight.transform.position = TimelineManager.instance.Lines[0].transform.GetChild(0).GetChild(m_currentCycle).position;
            m_cycleHighlight.transform.localPosition = new(m_cycleHighlight.transform.localPosition.x,0,m_cycleHighlight.transform.localPosition.z);
            foreach(var line in TimelineManager.instance.Lines)
            {
                if (line.m_orders[m_currentCycle] != null)
                {
                    line.m_actionTarget.GetComponent<ActionExecutor>().ExecuteOrder(line.m_orders[m_currentCycle].m_order);
                }
            }
            m_currentCycle++;
            if (m_currentCycle > ActionLine.s_longestActionLine) //loop back to the beginning of each line
            {
                m_currentCycle = 0;
            }
            if (m_stepByStep)
            {
                yield return new WaitUntil(gotoNextStep);
            }
            else
            {
                yield return new WaitForSeconds(ActionExecutor.s_cycleDuration);
            }
        }
        m_playRoutine = null;
    }
    private bool gotoNextStep()
    {
        return m_nextStep;
    }
}
