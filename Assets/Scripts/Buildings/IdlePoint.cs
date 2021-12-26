using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlePoint : MonoBehaviour
{
    public bool occupied = false;
    protected virtual void Awake()
    {
        RecordKeeper.Instance.AddEntity(gameObject);
    }
    public void ClaimSpot()
    {
        occupied = true;
    }
    public void FreeSpot()
    {
        occupied = false;
    }
}
