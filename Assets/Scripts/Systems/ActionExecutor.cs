using System.Collections;
using UnityEngine;

public class ActionExecutor : MonoBehaviour
{
    [SerializeField] private bool m_isExtendable = false;
    [SerializeField] private bool m_hasHand = false;
    [SerializeField] private bool m_canRotate = false;
    [SerializeField] private bool m_isOnTracks = false;

    public static float s_cycleDuration = 0.7f;
    private ObjectDraggable m_draggable;
    
    private Quaternion oldRotation;
    private Quaternion newRotation;

    private float routineTimer;

    private Coroutine currentBehaviour;
    private void Awake()
    {
        m_draggable = GetComponent<ObjectDraggable>();
    }

    public void ExecuteOrder(Order order)
    {
        switch (order)
        {
            case Order.Empty:
                break;
            case Order.RotLeft:
                if (m_canRotate)
                {
                    //this means we tried starting a new rotation before the previous behaviour was finished,
                    //we need to instantly finish (not skip) the current one, then do the next one,
                    //this is used for when the user spams the next step button
                    if (currentBehaviour != null)
                    {
                        oldRotation = newRotation;
                        transform.parent.rotation = oldRotation;
                        newRotation = Quaternion.Euler(0, 0, transform.parent.rotation.eulerAngles.z + 60f);
                        routineTimer = 0f;
                    } 
                    currentBehaviour ??= StartCoroutine(RotateRoutine(false, transform.parent));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.RotRight:
                if (m_canRotate)
                {
                    if (currentBehaviour != null)
                    {
                        oldRotation = newRotation;
                        transform.parent.rotation = oldRotation;
                        newRotation = Quaternion.Euler(0, 0, transform.parent.rotation.eulerAngles.z - 60f);
                        routineTimer = 0f;

                    }
                    currentBehaviour ??= StartCoroutine(RotateRoutine(true, transform.parent));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Extend:
                if (m_isExtendable)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Retract:
                if (m_isExtendable)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Plus:
                if (m_isOnTracks)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Minus:
                if (m_isOnTracks)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);

                }
                break;
            case Order.PivotRight:
                if (m_hasHand)
                {
                    StartCoroutine(RotateRoutine(true, m_draggable.m_arm.m_contentPivot.transform));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.PivotLeft:
                if (m_hasHand)
                {
                    StartCoroutine(RotateRoutine(false, m_draggable.m_arm.m_contentPivot.transform));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
        }
    }
    private IEnumerator RotateRoutine(bool right, Transform _toRotate)
    {
        routineTimer = 0f;
        oldRotation = _toRotate.rotation;
        newRotation = Quaternion.Euler(0, 0, _toRotate.rotation.eulerAngles.z + (right ? -60f : 60f));
        while (routineTimer < s_cycleDuration)
        {
            if (!ExecutionControls.instance.m_isPaused)
            {
                transform.parent.rotation = Quaternion.Slerp(oldRotation, newRotation, routineTimer / s_cycleDuration);
                routineTimer += Time.deltaTime;
            }
            yield return null;
        }
        currentBehaviour = null;
    }
}