using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ActionButtonPrefabs : ScriptableObject
{
    public List<GameObject> prefabs;
    public enum ActionButtonPrefab
    {
        Empty,
        RotLeft,
        RotRight,
        PivotLeft,
        PivotRight,
        Extend,
        Retract,
        MovePlus,
        MoveMinus,
        Reset,
        Grab,
        Drop
    }
}
