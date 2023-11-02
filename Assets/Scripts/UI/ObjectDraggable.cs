using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ObjectDraggable : Draggable
{
    public ArmEditor m_arm;
    [SerializeField] public bool m_canProgram = false;

    [SerializeField] private int m_dropLayerOrder = 3;
    public ObjectSlot m_slot;
    private bool m_placedLine = false;
    protected override IEnumerator Drag()
    {
        SetArmLayer(true);
        SetOtherLayersDrag(true);
        return base.Drag();
    }
    private void SetArmLayer(bool select)
    {
        if (m_arm != null)
        {
            m_arm.GetComponent<SpriteRenderer>().sortingOrder = m_dragLayerOrder + (select ? 1 : -2);
        }
    }
    private void SetOtherLayersDrag(bool drag)
    {
        if (m_toChangeLayer != null && m_toChangeLayer.Count > 0)
        {
            foreach (layerModificationData item in m_toChangeLayer)
            {
                item.spriteRenderer.sortingOrder = m_dragLayerOrder + item.modifier;
            }
        }
    }
    protected override IEnumerator TryDrop()
    {
        InventoryManager.instance.m_scroller.enabled = true;
        m_isInUI = false;
        //finds if object was dropped out of bounds or not
        if (!UIDragHelper.s_isOnUIElement)
        {
            RaycastHit2D Hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), 5000, ~LayerMask.GetMask("Mechanisms", "ArmGrabbable"));
            if (Hit.collider != null && Hit.collider.gameObject.layer == LayerMask.NameToLayer("Map"))
            {
                //finds closest tile and sets position of this object to that tile
                Tilemap map = MapManager.instance.m_placeableMap;
                Vector3Int closestCellPos = map.WorldToCell(transform.parent.position); // parent is pivot point
                Vector3 closestCellWorldPos = map.CellToWorld(closestCellPos);
                transform.parent.position = (Vector2)(closestCellWorldPos); //discards z so that z stays at 0

                yield return new WaitForFixedUpdate(); //waits for collider to update its position

                ContactFilter2D filter = new ContactFilter2D();
                filter.useTriggers = false;
                List<Collider2D> results = new();
                Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);
                RessourceSpawner Ressource = GetComponent<RessourceSpawner>();
                if (Ressource != null)
                {
                    if (Ressource.m_fusedSpawners.Count > 0)
                    {
                        foreach (RessourceSpawner fused in Ressource.m_fusedSpawners)
                        {
                            List<Collider2D> localResults = new();
                            Physics2D.OverlapCollider(fused.GetComponent<Collider2D>(), filter, localResults);
                            results.AddRange(localResults);
                        }
                    }
                }
                List<Collider2D> FilteredResult = results.Where(x => x.CompareTag("Mechanisms")).ToList();

                if (FilteredResult.Count <= 0)// we didn't find anything overlapping so we can place the thing down without resetting anything
                {
                    if (m_dropLayerOrder >= 0)
                    {
                        m_sprite.sortingOrder = m_dropLayerOrder;

                        if (m_arm != null)
                        {
                            m_arm.GetComponent<SpriteRenderer>().sortingOrder = m_dropLayerOrder + 1;
                        }
                        if (m_toChangeLayer != null && m_toChangeLayer.Count > 0)
                        {
                            foreach (layerModificationData item in m_toChangeLayer)
                            {
                                item.spriteRenderer.sortingOrder = m_dropLayerOrder + item.modifier;
                            }
                        }
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
                    m_slot.TryRegenObject();
                }
                else // we dropped it on another object
                {
                    if (m_isInUI)
                    {
                        m_sprite.sortingOrder = m_dragLayerOrder;
                        SetArmLayer(false);
                        SetOtherLayersDrag(true);
                    }
                    else
                    {
                        m_sprite.sortingOrder = m_dropLayerOrder;
                        SetArmLayer(false);
                        SetOtherLayersDrag(true);
                    }

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

