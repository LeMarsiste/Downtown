using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Building
{

    private void Update()
    {
        if (money == 0)
            destroyBuilding();
    }
    public override void destroyBuilding()
    {
        //TODO: Add Animations for this
        RecordKeeper.Instance.RemoveEntity(gameObject);
        Destroy(gameObject);
    }

    public override void GetOccupiedBy(GameObject target)
    {
        interactedNPC = target;
        occupied = true;
    }

    public override void RemoveOccupation()
    {
        occupied = false;
    }
}
