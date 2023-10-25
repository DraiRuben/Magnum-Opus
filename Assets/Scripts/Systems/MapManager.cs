using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    public UnityEvent UnselectAllEvent = new();
    // litteraly the same as a method call, this is only for the sake of clearer code
    public bool m_unselectAll { set { if (value) UnselectAllEvent.Invoke(); } } 

    public Tilemap m_placeableMap;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);  
    }

}
