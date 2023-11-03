using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
        Vector3Int oldTilePos;
        Vector3Int currentTilePos = MapManager.instance.m_placeableMap.WorldToCell(transform.position);
        //set the old tile pos as the first tile of the track if the track isn't built yet
        while (s_isSomethingDragging)
        {
            Vector3Int newTilePos = MapManager.instance.m_placeableMap.WorldToCell((Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
            if (currentTilePos != newTilePos)
            {
                var HitInfo = Physics2D.Raycast(MapManager.instance.m_placeableMap.CellToWorld(newTilePos), Vector3.forward, 50f, LayerMask.GetMask("Tracks"));
                //if we dragged on a new tile but this tile is also part of the track
                //remove the previous one
                if (HitInfo.collider != null)
                {
                    var comp = HitInfo.collider.GetComponent<Track>();

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
                        Destroy(m_tracks[trackIndex +1].transform.parent.gameObject);
                    }
                }//else new track to place
                else
                {
                    Vector3 currentWorldPos = MapManager.instance.m_placeableMap.CellToWorld(currentTilePos);
                    Vector3 newWorldPos = MapManager.instance.m_placeableMap.CellToWorld(newTilePos);
                    Vector3Int NewToCurrentRelativePos = newTilePos - currentTilePos;

                    //create new track
                    float angle = Mathf.Atan2(currentWorldPos.y - newWorldPos.y,currentWorldPos.x - newWorldPos.x) * Mathf.Rad2Deg;
                    m_tracks.Add(Instantiate(m_trackPrefab, newWorldPos,Quaternion.identity,transform.parent).GetComponentInChildren<Track>());
                    m_tracks[m_tracks.Count - 1].transform.rotation = Quaternion.Euler(0, 0, angle);
                    m_previousTrackSprite = m_currentTrackSprite;

                    //change sprite of previous track & unsets it as track end
                    SetTrackSprite(m_previousTrackSprite, SurroundingTilesCorrespondingSprite[offsetTilePos(NewToCurrentRelativePos)]);
                    m_tracks[m_tracks.Count - 2].m_nextTrack = m_tracks[m_tracks.Count - 1];
                    m_tracks[m_tracks.Count - 2].m_isTrackEnd = false;


                    //change sprite and rotation of new track & sets its previous track
                    m_currentTrackSprite = m_tracks[m_tracks.Count-1].GetComponent<SpriteRenderer>();
                    m_currentTrackSprite.sprite = m_180;
                    m_tracks[m_tracks.Count-1].transform.rotation = Quaternion.Euler(0, 0, angle); 
                    m_tracks[m_tracks.Count - 1].m_isTrackEnd = true;
                    m_tracks[m_tracks.Count - 1].m_previousTrack = m_tracks[m_tracks.Count - 2];


                }
                transform.position = MapManager.instance.m_placeableMap.CellToWorld(newTilePos);
                currentTilePos = newTilePos;
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
    //to get offset we do previousTileRelativePos - 3, so if we have the base one, offset is 0, if we are South West (-1,-1,0) offset is 4 - 3 = 1
    private Vector3Int offsetTilePos(Vector3Int previousTileRelativePos)
    {
        int index = SurroundingTilesCoord.IndexOf(previousTileRelativePos);
        int wrappedIndex = (index - 3 + 6) % 6 ;
        return SurroundingTilesCoord[wrappedIndex];
    }
    //gives the ID of the tile to place depending on the position of the new tile relative to the current one,
    //this is assuming the previous tile pos relative to the current one is (-1,0,0), which is why we need the previous 2 lists to get an offset
    private Dictionary<Vector3Int, int> SurroundingTilesCorrespondingSprite = new Dictionary<Vector3Int, int>(6)
    {
        { new(1,0,0),    1 },
        { new(0,1,0),    2 },
        { new(-1,1,0),   3 },
        { new(-1,0,0),  -1 },
        { new(-1,-1,0), -3 },
        { new(0,-1,0),  -2 },
    };
    private void SetTrackSprite(SpriteRenderer track, int spriteID)
    {
        if(spriteID == 1 || spriteID == -1)
        {
            track.sprite = m_180;
            if (spriteID < 0) track.flipY = true;
            else track.flipY = false;
        }
        else if(spriteID == 2 || spriteID ==-2)
        {
            track.sprite = m_120;
            if (spriteID < 0) track.flipY = true;
            else track.flipY = false;
        }
        else if(spriteID == 3 || spriteID ==-3)
        {
            track.sprite = m_60;
            if (spriteID < 0) track.flipY = true;
            else track.flipY = false;
        }
    }
}
