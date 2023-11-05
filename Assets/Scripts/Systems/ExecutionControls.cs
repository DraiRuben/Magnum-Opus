using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExecutionControls : MonoBehaviour
{
    public static ExecutionControls instance;

    [SerializeField] private GameObject m_cycleHighlight;
    [SerializeField] private ButtonSpriteChanger m_playButtonSpriteManager;
    [SerializeField] private ButtonSpriteChanger m_stepButtonSpriteManager;
    [SerializeField] private ButtonSpriteChanger m_stopButtonSpriteManager;
    [SerializeField] private Button m_stopButton;
    [SerializeField] private Button m_playButton;
    [SerializeField] private Button m_stepButton;

    [HideInInspector] public bool m_isPaused = true;
    [HideInInspector] public bool m_stepByStep = false;
    [HideInInspector] public bool m_nextStep = false;
    [HideInInspector] public bool m_isPlaying = false;


    [HideInInspector] public UnityEvent SkipToNextTrackEvent = new();
    private int m_currentCycle = 0;
    private Coroutine m_playRoutine;



    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        m_stopButton.onClick.AddListener(Stop);
        m_playButton.onClick.AddListener(Play);
        m_stepButton.onClick.AddListener(NextStep);
    }
    public void SetPlayButtonsInteractable(bool interactable)
    {
        m_playButton.interactable = interactable;
        m_stepButton.interactable = interactable;
    }
    public void InterruptPlay()
    {
        m_isPlaying = false;
        m_isPaused = true;
        m_playButton.interactable = false;
        m_playButtonSpriteManager.ClearHoverState();
        m_stepButton.interactable = false;
        m_stepButtonSpriteManager.ClearHoverState();
        m_stopButton.interactable = true;
    }
    public void NextStep()
    {
        if (!m_stepByStep)
        {
            //only do it once when clicking on the button
            MapManager.instance.m_unselectAll = true;
            Selectable.s_CanPlayerSelect = false;
            TimelineManager.instance.UpdateLongestActionLine.Invoke();
        }
        m_stopButton.interactable = true;
        m_isPaused = false;
        m_stepByStep = true;
        m_nextStep = true;
        SkipToNextTrackEvent.Invoke();
        m_playRoutine ??= StartCoroutine(PlayRoutine());
        m_playButtonSpriteManager.SetState(false);

    }
    public void Play()
    {
        //TODO: call function to fill in empty spaces between instructions
        
        if (m_stepByStep)
        {
            m_isPaused = false;
        }
        else
        {
            m_isPaused = !m_isPaused;
        }
        m_stopButton.interactable = true;
        m_stopButtonSpriteManager.ClearHoverState();
        m_stepByStep = false;
        m_nextStep = true;
        MapManager.instance.m_unselectAll = true;
        Selectable.s_CanPlayerSelect = false;
        TimelineManager.instance.UpdateLongestActionLine.Invoke();
        m_playRoutine ??= StartCoroutine(PlayRoutine());
    }

    public void Stop()
    {
        //destroy all cloned ressources & filler empty actions, need to make a class to allow ressources to be picked up & an automatic function in actionline that fills up the lines
        m_currentCycle = 0;
        m_stepByStep = false;
        m_cycleHighlight.SetActive(false);
        m_isPlaying = false;
        Selectable.s_CanPlayerSelect = true;
        m_playButtonSpriteManager.SetState(false);
        m_playRoutine = null;
        m_playButton.interactable = true;
        m_stepButton.interactable = true;

        m_stopButton.interactable = false;
        m_stopButtonSpriteManager.ClearHoverState();
    }
    private IEnumerator PlayRoutine()
    {
        m_isPlaying = true;
        m_cycleHighlight.SetActive(true);
        while (m_isPlaying)
        {
            m_cycleHighlight.transform.position = TimelineManager.instance.Lines[0].transform.GetChild(0).GetChild(m_currentCycle + 1).position;
            m_cycleHighlight.transform.localPosition = new(m_cycleHighlight.transform.localPosition.x, 0, m_cycleHighlight.transform.localPosition.z);
            foreach (ActionLine line in TimelineManager.instance.Lines)
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
                m_nextStep = false;
                yield return new WaitUntil(gotoNextStep);
            }
            else
            {
                if (m_isPaused)
                {
                    yield return new WaitUntil(isUnpaused);
                }
                else
                {
                    yield return new WaitForSeconds(ActionExecutor.s_cycleDuration*1.01f);

                }
            }
        }
    }
    private bool gotoNextStep()
    {
        return m_nextStep;
    }
    private bool isUnpaused()
    {
        return !m_isPaused;
    }
}
