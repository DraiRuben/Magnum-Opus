using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    public Tilemap m_placeableMap;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);  
    }

}
