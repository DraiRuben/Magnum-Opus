using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ObjectDraggable : Draggable
{
    [HideInInspector] public bool m_canProgram = false;
    [HideInInspector] public ArmEditor m_arm;
    [SerializeField] private int m_dropLayerOrder = 3; // if it's set to -1 that means we don't change layer on drop
    public ObjectSlot m_slot;
    private bool m_placedLine = false;
    protected override IEnumerator TryDrop()
    {
        InventoryManager.instance.m_scroller.enabled = true;
        m_isInUI = false;
        //finds if object was dropped out of bounds or not
        if (!UIDragHelper.s_isOnUIElement)
        {
            RaycastHit2D Hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), 5000, ~LayerMask.GetMask("Mechanisms"));
            if (Hit.collider != null && Hit.collider.gameObject.layer == LayerMask.NameToLayer("Map"))
            {
                //finds closest tile and sets position of this object to that tile
                Tilemap map = MapManager.instance.m_placeableMap;
                Vector3Int closestCellPos = map.WorldToCell(transform.parent.position); // parent is pivot point
                Vector3 closestCellWorldPos = map.CellToWorld(closestCellPos);
                transform.parent.position = (Vector2)closestCellWorldPos; //discards z so that z stays at 0

                yield return new WaitForFixedUpdate(); //waits for collider to update its position

                ContactFilter2D filter = new ContactFilter2D();
                filter.useTriggers = false;
                List<Collider2D> results = new();
                Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);
                List<Collider2D> FilteredResult = results.Where(x => x.CompareTag("Mechanisms")).ToList();

                if (FilteredResult.Count <= 0)// we didn't find anything overlapping so we can place the thing down without resetting anything
                {
                    if (m_dropLayerOrder >= 0)
                    {
                        m_sprite.sortingOrder = m_dropLayerOrder;
                        if (m_toActivateOnDrop != null) //sometimes we might want to activate things on drop like the arm editor
                        {
                            foreach (GameObject _toActivate in m_toActivateOnDrop)
                            {
                                _toActivate.SetActive(true);
                            }
                        }
                    }
                    if (!m_placedLine && m_canProgram)
                    {
                        m_placedLine = true;
                        TimelineManager.instance.RegisterNewActionnableObject(this);
                    }
                }
                else // we dropped it on another object
                {
                    if (m_isInUI)
                        m_sprite.sortingOrder = m_dragLayerOrder;
                    else
                        m_sprite.sortingOrder = m_dropLayerOrder;

                    m_initialDrag = true;
                    InvalidateDrop();
                }
            }
            else // we dropped it out of the buildable area
            {
                if (m_placedLine)
                {
                    m_placedLine = false;
                    TimelineManager.instance.RemoveActionnableObject(this);
                }
                m_initialDrag = false;
                InvalidateDrop();
            }
        }
        else // we dropped it on a UI element
        {
            if (m_placedLine)
            {
                m_placedLine = false;
                TimelineManager.instance.RemoveActionnableObject(this);
            }
            m_initialDrag = false;
            s_IsSomethingSelected = false;
            m_isSelected = false;
            InvalidateDrop();
        }
    }
    protected override IEnumerator Drag()
    {
        if (m_isInUI)
        {
            m_slot.TryRegenObject();

        }
        return base.Drag();

    }
    private void InvalidateDrop()
    {
        if (m_initialDrag)
        {
            m_initialDrag = false;
            transform.parent.position = m_posBeforeDrag;
            transform.parent.rotation = m_rotBeforeDrag;
            transform.parent.localScale = m_scaleBeforeDrag;

        }
        else
        {
            m_initialDrag = true;
            m_slot.TryReturnObject(this);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjectDraggable))] // to show the arm field only if the object is programmable
public class ObjectDraggable_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields

        ObjectDraggable script = (ObjectDraggable)target;

        // draw checkbox for the bool
        script.m_canProgram = EditorGUILayout.Toggle("CanProgram", script.m_canProgram);
        if (script.m_canProgram) // if bool is true, show other fields
        {
            script.m_arm = EditorGUILayout.ObjectField("Arm", script.m_arm, typeof(ArmEditor), true) as ArmEditor;
        }
    }
}
#endif