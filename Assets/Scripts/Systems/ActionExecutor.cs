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
    private Order m_currentOrder;

    private Coroutine currentBehaviour;
    private Transform m_currentlyChangingTransform;
    private void Awake()
    {
        m_draggable = GetComponent<ObjectDraggable>();
    }
    private void FinishOrder()
    {
        if(m_currentOrder == Order.RotLeft ||  m_currentOrder == Order.RotRight
            || m_currentOrder == Order.PivotLeft || m_currentOrder == Order.PivotRight)
        {
            oldRotation = newRotation;
            m_currentlyChangingTransform.rotation = oldRotation;
            Debug.Log(oldRotation.eulerAngles.z);
        }
        else if(m_currentOrder == Order.Extend || m_currentOrder == Order.Retract)
        {

        }
        else if(m_currentOrder == Order.Plus || m_currentOrder == Order.Minus)
        {

        }
        currentBehaviour = null;
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
                    if (currentBehaviour != null) { FinishOrder(); }
                        
                    routineTimer = 0f;
                    m_currentlyChangingTransform = transform.parent;
                    currentBehaviour ??= StartCoroutine(RotateRoutine(false, m_currentlyChangingTransform));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.RotRight:
                if (m_canRotate)
                {
                    if (currentBehaviour != null) { FinishOrder(); }

                    routineTimer = 0f;
                    m_currentlyChangingTransform = transform.parent;
                    currentBehaviour ??= StartCoroutine(RotateRoutine(true, m_currentlyChangingTransform));
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
                    if (currentBehaviour != null) { FinishOrder(); }

                    routineTimer = 0f;
                    m_currentlyChangingTransform = m_draggable.m_arm.m_contentPivot.transform;
                    currentBehaviour??= StartCoroutine(RotateRoutine(true, m_draggable.m_arm.m_contentPivot.transform));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.PivotLeft:
                if (m_hasHand)
                {
                    if (currentBehaviour != null) { FinishOrder(); }

                    routineTimer = 0f;
                    m_currentlyChangingTransform = m_draggable.m_arm.m_contentPivot.transform;
                    currentBehaviour ??= StartCoroutine(RotateRoutine(false, m_draggable.m_arm.m_contentPivot.transform));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Grab:
                if (m_hasHand)
                {
                    StartCoroutine(GrabRoutine());
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Drop:
                if (m_hasHand)
                {
                    StartCoroutine(DropRoutine());
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
        }
        m_currentOrder = order;
    }
    private IEnumerator RotateRoutine(bool right, Transform _toRotate)
    {
        routineTimer = 0f;
        oldRotation = _toRotate.rotation;
        newRotation = Quaternion.Euler(0, 0, _toRotate.rotation.eulerAngles.z + (right ? -60 : 60));
        while (routineTimer < s_cycleDuration && ExecutionControls.instance.m_isPlaying)
        {
            if (!ExecutionControls.instance.m_isPaused)
            {
                _toRotate.rotation = Quaternion.Slerp(oldRotation, newRotation, routineTimer / s_cycleDuration);
                routineTimer += Time.deltaTime;
            }
            if(_toRotate != m_currentlyChangingTransform)
            {
                yield break;
            }
            yield return null;
        }
        currentBehaviour = null;
    }
    private IEnumerator GrabRoutine()
    {
        if (m_draggable.m_arm.m_contentPivot.transform.childCount <= 0)
        {
            Vector3Int tilePosUnderHand = MapManager.instance.m_placeableMap.WorldToCell(m_draggable.m_arm.m_contentPivot.transform.position);
            Vector3 worldTilePosUnderHand = MapManager.instance.m_placeableMap.CellToWorld(tilePosUnderHand);
            RaycastHit2D HitInfo = Physics2D.Raycast(worldTilePosUnderHand + Vector3.back, Vector3.forward, 50f, LayerMask.GetMask("ArmGrabbable"));
            if (HitInfo.collider != null)
            {
                Ressource comp = HitInfo.collider.GetComponent<Ressource>();
                if (comp != null && comp.CanBeGrabbed)
                {
                    comp.RearrangeFusionHierarchy();
                    comp.m_isGrabbed = true;
                    comp.transform.parent = m_draggable.m_arm.m_contentPivot.transform;
                }
            }
        }
        yield return null;
    }
    private IEnumerator DropRoutine()
    {
        if (m_draggable.m_arm.m_contentPivot.transform.childCount > 0)
        {
            Vector3Int tilePosUnderHand = MapManager.instance.m_placeableMap.WorldToCell(m_draggable.m_arm.m_contentPivot.transform.position);
            Vector3 worldTilePosUnderHand = MapManager.instance.m_placeableMap.CellToWorld(tilePosUnderHand);

            Ressource comp = m_draggable.m_arm.m_contentPivot.transform.GetChild(0).GetComponent<Ressource>();
            if (comp != null)
            {
                comp.transform.parent = null;
                comp.m_isGrabbed = false;
            }
        }
        yield return null;
    }
}