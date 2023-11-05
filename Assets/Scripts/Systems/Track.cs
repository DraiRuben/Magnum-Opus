using UnityEngine;

public class Track : MonoBehaviour
{
    public Track m_nextTrack;
    public Track m_previousTrack;
    public bool m_isTrackEnd;
    public bool m_isRessourceTrack = true;
    public SpriteRenderer m_renderer;

    public Vector3 GetNextTrackPos()
    {
        if (!m_isTrackEnd)
            return m_nextTrack.transform.parent.position;
        else return transform.position;
    }
    public Vector3 GetPreviousTrackPos()
    {
        if (!m_isTrackEnd)
            return m_previousTrack.transform.parent.position;
        else return transform.position;
    }
}
