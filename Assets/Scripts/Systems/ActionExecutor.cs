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