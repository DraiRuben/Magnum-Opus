using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectDraggable : Draggable
{
    [SerializeField] private ObjectSlot slot;
    protected override void TryDrop()
    {
        //tries to find anything that would overlap with the thing
        RaycastHit2D Hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 5000,~LayerMask.GetMask("Mechanisms"));
        if (Hit.collider != null && Hit.collider.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            List<Collider2D> results = new();
            Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);
            var FilteredResult = results.Where(x => x.CompareTag("Mechanisms")).ToList();
            if (FilteredResult.Count <= 0)// we didn't find anything overlapping so we can place the thing down
            {
                //finds closest tile and sets position of this object to that tile
                Tilemap map = MapManager.instance.m_placeableMap;
                Vector3Int closestCellPos = map.WorldToCell(transform.parent.position); // parent is pivot point
                transform.parent.position = map.CellToWorld(closestCellPos);
            }
        }
        else // we dropped it out of the buildable area
        {
            slot.TryReturnObject(this);
        }
        s_IsSomethingSelected = false;
        m_isSelected = false;
    }
}