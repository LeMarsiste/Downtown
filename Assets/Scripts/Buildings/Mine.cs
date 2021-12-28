using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Building
{
    private void Awake()
    {
        RecordKeeper.Instance.AddBuilding<Mine>(gameObject);
    }
}
