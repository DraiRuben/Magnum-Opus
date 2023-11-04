using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    [HideInInspector] public UnityEvent UnselectAllEvent = new();
    public Vector3Int mouseTilePosDebug;
    // litteraly the same as a method call, this is only for the sake of clearer code
    public bool m_unselectAll { set { if (value) UnselectAllEvent.Invoke(); } }

    public Tilemap m_placeableMap;
    public Tilemap m_armZonesMap;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    private void Update()
    {
        mouseTilePosDebug = m_placeableMap.WorldToCell(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
    }
}
