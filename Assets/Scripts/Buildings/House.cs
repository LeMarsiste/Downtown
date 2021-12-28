using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Building
{
    private void Awake()
    {
        RecordKeeper.Instance.AddBuilding<House>(gameObject);
    }
}
