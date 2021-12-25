using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlePoint : MonoBehaviour
{
    public bool occupied = false;
    protected virtual void Awake()
    {
        if (RecordKeeper.Instance == null)
            RecordKeeper.Instance = GameObject.Find("RecordKeeper").GetComponent<RecordKeeper>(); //This was necessary
        RecordKeeper.Instance.AddEntity(gameObject);
    }
    public void claimSpot()
    {
        occupied = true;
    }
    public void freeSpot()
    {
        occupied = false;
    }
}
