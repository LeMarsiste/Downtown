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
    protected virtual void Update()
    {
        if (money == 0)
            DestroyBuilding();
    }
    public virtual void DestroyBuilding()
    {
        //TODO: Add Animations for this
        Destroy(gameObject);
    }
    public virtual void GetOccupiedBy(GameObject target)
    {
        interactedNPC = target;
        occupied = true;
    }
    public virtual void RemoveOccupation()
    {
        occupied = false;
    }
}
