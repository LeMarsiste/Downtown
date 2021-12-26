using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    public int money;
    public bool occupied;
    public GameObject interactedNPC;

    protected virtual void Awake()
    {
        if (RecordKeeper.Instance == null)
            RecordKeeper.Instance = GameObject.Find("RecordKeeper").GetComponent<RecordKeeper>(); //This was necessary
        RecordKeeper.Instance.AddEntity(gameObject);
    }

    public abstract void DestroyBuilding();
    public abstract void RemoveOccupation();
    public abstract void GetOccupiedBy(GameObject target);
    
}
