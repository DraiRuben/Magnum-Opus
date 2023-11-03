using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public Track m_nextTrack;
    public Track m_previousTrack;
    public bool m_isTrackEnd;

    public Vector3 GetNextTrackPos()
    {
        if(!m_isTrackEnd)
            return m_nextTrack.transform.position;
        else return transform.position;
    }
    public Vector3 GetPreviousTrackPos()
    {
        if (!m_isTrackEnd)
            return m_previousTrack.transform.position;
        else return transform.position;
    }
}
