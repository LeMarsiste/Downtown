using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlePoint : MonoBehaviour
{
    public bool occupied = false;
    
    public void claimSpot()
    {
        occupied = true;
    }
    public void freeSpot()
    {
        occupied = false;
    }
}
