using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexScript : MonoBehaviour
{
    //Stores the grid coordinates
    [System.NonSerialized]
    public Vector2Int Coordinates = Vector2Int.zero;
    
    [System.NonSerialized]
    public bool isBomb = false;

}
