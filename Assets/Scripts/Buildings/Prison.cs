using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prison : Building
{
    public GameObject TheifPrefab, AssassinPrefab;
    public GameObject NPCParent;
    protected override void Update()
    {
        //overriden to prevent this object from being removed if gold reached 0
    }
    public void QueueTheifSpawn()
    {
        StartCoroutine(SpawnTheif());
    }
    public void QueueAssassinSpawn()
    {
        StartCoroutine(SpawnAssassin());
    }
    IEnumerator SpawnTheif()
    {
        yield return new WaitForSeconds(60f);
        GameObject newTheif = Instantiate(TheifPrefab, NPCParent.transform) as GameObject;
        yield return new WaitForSeconds(0.2f); //This could have been WaitForEndOfFrame()
        newTheif.GetComponent<Theif>().Incarnate();
    }
    IEnumerator SpawnAssassin()
    {
        yield return new WaitForSeconds(60f);
        GameObject newTheif = Instantiate(AssassinPrefab, NPCParent.transform) as GameObject;
        yield return new WaitForSeconds(0.2f); //This could have been WaitForEndOfFrame()
        newTheif.GetComponent<Assassin>().Incarnate();
    }
}
