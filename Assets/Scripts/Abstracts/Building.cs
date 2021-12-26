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
        RecordKeeper.Instance.AddEntity(gameObject);
    }

    public abstract void DestroyBuilding();
    public abstract void RemoveOccupation();
    public abstract void GetOccupiedBy(GameObject target);
    
}
