using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    public int Money;
    public bool Occupied;
    public GameObject InteractedNPC;
    public GameObject WaypointPortal;
    protected virtual void Update()
    {
        if (Money == 0)
            DestroyBuilding();
    }
    public virtual void DestroyBuilding()
    {
        //TODO: Add Animations for this
        Destroy(gameObject);
    }
    public virtual void GetOccupiedBy(GameObject target)
    {
        InteractedNPC = target;
        Occupied = true;
    }
    public virtual void RemoveOccupation()
    {
        Occupied = false;
    }
}
