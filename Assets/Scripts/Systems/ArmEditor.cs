using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ArmEditor : Draggable
{
    [SerializeField] private Draggable m_armBase;
    [SerializeField] private SpriteRenderer m_smallArm;
    [SerializeField] private GameObject m_mediumArm;
    [SerializeField] private GameObject m_longArm; //of the law 
    private int m_armLength = 1;

    private void SetArmLength(int value)
    {
        m_armLength = Mathf.Clamp(value, 1, 3);
        //lengthen or shorten arm
        m_smallArm.enabled = m_armLength == 1; //doesn't disable gameobject since this the draggable should still be usable
        m_mediumArm.SetActive(m_armLength == 2);
        m_longArm.SetActive(m_armLength == 3);


    }
    private void OnDisable()
    {
        if (MapManager.instance != null && MapManager.instance.m_armZonesMap != null)
            MapManager.instance.m_armZonesMap.transform.position = new(0, 50000);
    }
    protected override IEnumerator TryDrop()
    {
        if (m_toActivateOnDrop != null)
            foreach (GameObject _toActivate in m_toActivateOnDrop)
            {
                _toActivate.SetActive(true);
            }
        MapManager.instance.m_armZonesMap.transform.position = new(0, 50000);
        yield return null;
    }
    protected override void Interact()
    {
        m_isSelected = true; //this puts the check to deselect to false,
                             //and since this editor is only accessible by selection, it just works
        base.Interact();
    }
    protected override IEnumerator Drag()
    {
        m_armBase.m_staySelectedOnDrop = true; // for drop
        //sets center of highlight as arm base
        MapManager.instance.m_armZonesMap.transform.position = transform.parent.parent.position;
        SaveTransform();
        Tilemap validZones = MapManager.instance.m_armZonesMap;
        Quaternion newRotation = transform.parent.parent.rotation;
        Quaternion oldRotation = newRotation;
        Vector3Int newValidTile = new();

        float slerpTimer = 1f;
        while (s_isSomethingDragging)
        {
            Vector3Int tilePos = validZones.WorldToCell((Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));

            if (slerpTimer <= 1f)
            {
                transform.parent.parent.rotation = Quaternion.Slerp(oldRotation, newRotation, slerpTimer);
                slerpTimer += 5.5f * Time.deltaTime;
                if (slerpTimer > 1f) slerpTimer = 1f; //helps to have almost exact angles
            }
            if (validZones.GetTile(tilePos) != null
                && tilePos != newValidTile)
            {
                newValidTile = tilePos;
                oldRotation = transform.parent.parent.rotation;
                slerpTimer = 0f;

                Vector3 validTileWorldPos = validZones.CellToWorld(tilePos);
                Vector3 direction = validTileWorldPos - transform.parent.parent.position;

                newRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                transform.parent.position = transform.parent.parent.position + Vector2.Distance(validTileWorldPos, transform.parent.parent.position) * transform.parent.parent.right;
                transform.parent.position += Vector3.back;  //offset z by 2 so that the raycast on mouse relase may always hit this one first
                SetArmLength(Mathf.RoundToInt(Vector3Int.Distance(tilePos, validZones.WorldToCell((Vector2)transform.parent.parent.position))));

            }

            yield return null;
        }
        //finishes rotation if we stopped dragging midway
        while (Quaternion.Angle(transform.parent.parent.rotation, newRotation) > 1f)
        {
            transform.parent.parent.rotation = Quaternion.Slerp(oldRotation, newRotation, slerpTimer);
            slerpTimer += 5.5f * Time.deltaTime;
            yield return null;
        }
        StartCoroutine(TryDrop());
    }
}
