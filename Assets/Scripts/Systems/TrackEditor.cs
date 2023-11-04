using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class TrackEditor : Draggable
{
    [SerializeField] private Draggable m_draggable;
    [SerializeField] private GameObject m_trackPrefab;
    [SerializeField] private Sprite m_startingTracks;
    [SerializeField] private Sprite m_end;
    [SerializeField] private Sprite m_60;
    [SerializeField] private Sprite m_120;
    [SerializeField] private Sprite m_180;
    [SerializeField] private List<Track> m_tracks;

    private SpriteRenderer m_currentTrackSprite;

    private SpriteRenderer m_previousTrackSprite;
    protected override IEnumerator TryDrop()
    {
        if (m_toActivateOnDrop != null)
            foreach (GameObject _toActivate in m_toActivateOnDrop)
            {
                _toActivate.SetActive(true);
            }
        yield return null;
    }
    private void Start()
    {
        m_tracks.Add(Instantiate(m_trackPrefab, transform.parent).GetComponentInChildren<Track>());
        m_currentTrackSprite = m_tracks[0].GetComponent<SpriteRenderer>();
        m_currentTrackSprite.sprite = m_startingTracks;
    }
    protected override void Interact()
    {
        m_isSelected = true; //this puts the check to deselect to false,
                             //and since this editor is only accessible by selection, it just works
        base.Interact();
    }
    protected override IEnumerator Drag()
    {
        Vector3Int oldTilePos = MapManager.instance.m_placeableMap.WorldToCell(m_tracks[0].transform.position);
        if (m_tracks.Count > 2)
        {
            oldTilePos = MapManager.instance.m_placeableMap.WorldToCell(m_tracks[m_tracks.Count-2].transform.parent.position);
        }
        
        Vector3Int currentTilePos = MapManager.instance.m_placeableMap.WorldToCell(transform.position);
        currentTilePos.z = 0;
        //set the old tile pos as the first tile of the track if the track isn't built yet
        while (s_isSomethingDragging)
        {
            Vector3Int newTilePos = MapManager.instance.m_placeableMap.WorldToCell((Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
            if (currentTilePos != newTilePos)
            {
                Vector3Int NewToCurrentRelativePos = currentTilePos - newTilePos;
                bool canPlace = NewToCurrentRelativePos.y%2==0? SurroundingTilesCoord.Contains(NewToCurrentRelativePos): SurroundingTilesCoordAlt.Contains(NewToCurrentRelativePos);
                if (canPlace)
                {
                    var HitInfo = Physics2D.Raycast(MapManager.instance.m_placeableMap.CellToWorld(newTilePos), Vector3.forward, 50f, LayerMask.GetMask("Tracks"));
                    //if we dragged on a new tile but this tile is also part of the track
                    //remove the previous one
                    if (HitInfo.collider != null)
                    {
                        /*var comp = HitInfo.collider.GetComponent<Track>();

                        int trackIndex = m_tracks.IndexOf(comp);
                        if (trackIndex > 0)
                        {
                            //change previous tile's sprite 
                            m_previousTrackSprite = m_tracks[trackIndex - 1].GetComponent<SpriteRenderer>();
                            oldTilePos = MapManager.instance.m_placeableMap.WorldToCell(m_tracks[trackIndex - 1].transform.position);
                            Vector3Int OldToCurrentRelativePos = oldTilePos - currentTilePos;
                            if (trackIndex > 2) //if when we remove the current track, we don't end up making the track's length 1
                            {
                                SetTrackSprite(m_previousTrackSprite, SurroundingTilesCorrespondingSprite[offsetTilePos(OldToCurrentRelativePos)]);
                                m_tracks[trackIndex].m_isTrackEnd = true;
                            }
                            else
                            {
                                m_previousTrackSprite.sprite = m_startingTracks;
                            }
                            //sets changed track as track end
                            m_tracks[trackIndex].m_nextTrack = null;
                            m_tracks[trackIndex].m_isTrackEnd = true;
                            //destroy previous one, meaning index of new track +1
                            Destroy(m_tracks[trackIndex + 1].transform.parent.gameObject);
                        }*/
                    }//else new track to place
                    else
                    {
                        Vector3 currentWorldPos = MapManager.instance.m_placeableMap.CellToWorld(currentTilePos);
                        Vector3 newWorldPos = MapManager.instance.m_placeableMap.CellToWorld(newTilePos);
                        Vector3 oldWorldPos = MapManager.instance.m_placeableMap.CellToWorld(oldTilePos);

                        //create new track
                        float angle = Mathf.Atan2(newWorldPos.y - currentWorldPos.y, newWorldPos.x - currentWorldPos.x) * Mathf.Rad2Deg;
                        m_tracks.Add(Instantiate(m_trackPrefab, newWorldPos, Quaternion.identity, transform.parent).GetComponentInChildren<Track>());
                        m_tracks[m_tracks.Count - 1].transform.rotation = Quaternion.Euler(0, 0, angle);
                        m_previousTrackSprite = m_currentTrackSprite;

                        //change sprite of previous track & unsets it as track end
                        Vector3 newToCurrent = newWorldPos - currentWorldPos;
                        Vector3 oldToCurrent = oldWorldPos - currentWorldPos;
                        float angle2 = Mathf.Acos(Vector3.Dot(newToCurrent,oldToCurrent)/(newToCurrent.magnitude * oldToCurrent.magnitude))*Mathf.Rad2Deg;
                        SetTrackSprite(m_previousTrackSprite, angle2, oldTilePos, newTilePos, currentTilePos);
                        m_tracks[m_tracks.Count - 2].m_nextTrack = m_tracks[m_tracks.Count - 1];
                        m_tracks[m_tracks.Count - 2].m_isTrackEnd = false;


                        //change sprite and rotation of new track & sets its previous track
                        m_currentTrackSprite = m_tracks[m_tracks.Count - 1].GetComponent<SpriteRenderer>();
                        m_currentTrackSprite.sprite = m_end;
                        m_tracks[m_tracks.Count - 1].transform.rotation = Quaternion.Euler(0, 0, angle);
                        m_tracks[m_tracks.Count - 1].m_isTrackEnd = true;
                        m_tracks[m_tracks.Count - 1].m_previousTrack = m_tracks[m_tracks.Count - 2];


                    }
                    transform.position = MapManager.instance.m_placeableMap.CellToWorld(newTilePos);
                    oldTilePos = currentTilePos;
                    currentTilePos = newTilePos;
                }
                
            }
            yield return null;
        }

    }
    //this list is used to get an offset to use in the surroundingtilescorrespondingsprite dict
    private List<Vector3Int> SurroundingTilesCoord = new List<Vector3Int>(6) 
    {
        new(1,0,0),
        new(0,1,0),
        new(-1,1,0),
        new(-1,0,0), // this is the base one
        new(-1,-1,0),
        new(0,-1,0),
    };
    private List<Vector3Int> SurroundingTilesCoordAlt = new List<Vector3Int>(6) 
    {
        new(1,0,0),
        new(1,1,0),
        new(0,1,0),
        new(-1,0,0), // this is the base one
        new(0,-1,0),
        new(1,-1,0),
    };
    private void SetTrackSprite(SpriteRenderer track, float angle, Vector3Int previousTilePos, Vector3Int newTilePos, Vector3Int currentTilePos)
    {
        if(Mathf.Abs(0 - Mathf.Repeat(angle, 360.0f)) <= 2f|| Mathf.Abs(180 - Mathf.Repeat(angle, 360.0f)) <= 2f)
        {
            track.sprite = m_180;
        }
        else if(Mathf.Abs(120 - Mathf.Repeat(angle, 360.0f)) <= 2f)
        {
            track.sprite = m_120;
            track.flipY = DetermineFlipYFor120Degree(previousTilePos, newTilePos, currentTilePos);
        }
        else if(Mathf.Abs(60-Mathf.Repeat(angle, 360.0f))<=2f)
        {
            track.sprite = m_60;
            track.flipY = DetermineFlipYFor60Degree(previousTilePos, newTilePos, currentTilePos);
        }
    }
    // A FUCKING NIGHTMARE THAT WAS
    bool DetermineFlipYFor60Degree(Vector3Int previousTilePos, Vector3Int newTilePos, Vector3Int currentTilePos)
    {
        bool isAlternate = previousTilePos.y % 2 != 0; // see https://docs.unity3d.com/Manual/Tilemap-Hexagonal.html for more info,
                                                       // since my hexagonal tilemap has its odd lines offset, we need to do things differently depending on the line
        Vector3Int relativePos = newTilePos - previousTilePos;
        Vector3Int relativeToCurrentPos = newTilePos - currentTilePos;
        // Check if the relative position indicates a flip along the Y-axis
        bool flipY = false;
        if (!isAlternate)
        {
            if ((relativePos == new Vector3Int(0, -1, 0) && relativeToCurrentPos == new Vector3Int(-1, -1, 0))
                || (relativePos == new Vector3Int(1, 0, 0) && relativeToCurrentPos == new Vector3Int(1, -1, 0))
                || (relativePos == new Vector3Int(-1, -1, 0) && relativeToCurrentPos == new Vector3Int(-1, 0, 0))
                || (relativePos == new Vector3Int(-1, -1, 0) && relativeToCurrentPos == new Vector3Int(-1, 0, 0))
                || (relativePos == new Vector3Int(0, 1, 0) && relativeToCurrentPos == new Vector3Int(1, 0, 0))
                || (relativePos == new Vector3Int(-1, 1, 0) && relativeToCurrentPos == new Vector3Int(0, 1, 0))
                || (relativePos == new Vector3Int(-1, 0, 0) && relativeToCurrentPos == new Vector3Int(0, 1, 0)))
            {
                flipY = true;
            }
        }
        else
        {
            if ((relativePos == new Vector3Int(-1, 0, 0) && relativeToCurrentPos == new Vector3Int(-1, 1, 0))
                ||(relativePos == new Vector3Int(-1, 0, 0) && relativeToCurrentPos == new Vector3Int(0, 1, 0))
                ||(relativePos == new Vector3Int(1, -1, 0) && relativeToCurrentPos == new Vector3Int(0, -1, 0))
                ||(relativePos == new Vector3Int(0, -1, 0) && relativeToCurrentPos == new Vector3Int(-1, 0, 0))
                ||(relativePos == new Vector3Int(0, 1, 0) && relativeToCurrentPos == new Vector3Int(1, 1, 0))
                ||(relativePos == new Vector3Int(1, 1, 0) && relativeToCurrentPos == new Vector3Int(1, 0, 0))
                ||(relativePos == new Vector3Int(1, 0, 0) && relativeToCurrentPos == new Vector3Int(0, -1, 0)))
            {
                flipY = true;
            }
        }

        return flipY;
    }
    bool DetermineFlipYFor120Degree(Vector3Int previousTilePos, Vector3Int newTilePos, Vector3Int currentTilePos)
    {
        bool isAlternate = previousTilePos.y % 2 != 0; // see https://docs.unity3d.com/Manual/Tilemap-Hexagonal.html for more info,
                                                       // since my hexagonal tilemap has its odd lines offset, we need to do things differently depending on the line
        Vector3Int relativePos = newTilePos - previousTilePos;
        Vector3Int relativeToCurrentPos = newTilePos - currentTilePos;
        // Check if the relative position indicates a flip along the Y-axis
        bool flipY = false;
        if (!isAlternate)
        {
            if (   (relativePos == new Vector3Int(-1, 1, 0) && relativeToCurrentPos == new Vector3Int(0, 1, 0))
                || (relativePos == new Vector3Int(1, -1, 0) && relativeToCurrentPos == new Vector3Int(0,-1,0))
                || (relativePos == new Vector3Int(1, 1, 0) && relativeToCurrentPos == new Vector3Int(1, 0, 0))
                || (relativePos == new Vector3Int(-2, -1, 0) && relativeToCurrentPos == new Vector3Int(-1, 0, 0))
                || (relativePos == new Vector3Int(-2, 1, 0)&& relativeToCurrentPos == new Vector3Int(-1,1,0))
                || (relativePos == new Vector3Int(0, -2, 0)&& relativeToCurrentPos == new Vector3Int(0,-1,0))
                || (relativePos == new Vector3Int(0, 2, 0)&& relativeToCurrentPos == new Vector3Int(1,1,0)))
            {
                flipY = true;
            }
        }
        else
        {
            if (( relativePos == new Vector3Int(-1, 1, 0) && relativeToCurrentPos == new Vector3Int(0, 1, 0))
                || relativePos == new Vector3Int(-1,-1,0) && relativeToCurrentPos == new Vector3Int(-1, 0, 0)
                || (relativePos == new Vector3Int(0, 2, 0) && relativeToCurrentPos == new Vector3Int(0, 1, 0))
                || (relativePos == new Vector3Int(2, 1, 0) && relativeToCurrentPos == new Vector3Int(1, 0, 0))
                || (relativePos == new Vector3Int(0, -2, 0) && relativeToCurrentPos == new Vector3Int(-1, -1, 0))
                || (relativePos == new Vector3Int(2, -1, 0) && relativeToCurrentPos == new Vector3Int(1, -1, 0)))
            {
                flipY = true;
            }
        }

        return flipY;
    }
}
