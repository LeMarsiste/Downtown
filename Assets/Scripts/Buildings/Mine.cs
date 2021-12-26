using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Building
{
    private void Update()
    {
        if (money == 0)
            DestroyBuilding();
    }
    public override void DestroyBuilding()
    {
        //TODO: Add Animations for this
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
