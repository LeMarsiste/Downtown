using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prison : Building
{
    public GameObject TheifPrefab, AssassinPrefab;
    public GameObject NPCParent;
    private void Update()
    {
    }
    public override void destroyBuilding()
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
    public void QueueTheifSpawn()
    {
        StartCoroutine(spawnTheif());
    }
    public void QueueAssassinSpawn()
    {
        StartCoroutine(spawnAssassin());
    }
    IEnumerator spawnTheif()
    {
        yield return new WaitForSeconds(60f);
        GameObject newTheif = Instantiate(TheifPrefab, NPCParent.transform) as GameObject;
        yield return new WaitForSeconds(0.2f); //This could have been WaitForEndOfFrame()
        newTheif.GetComponent<Theif>().Incarnate();
    }
    IEnumerator spawnAssassin()
    {
        yield return new WaitForSeconds(60f);
        GameObject newTheif = Instantiate(AssassinPrefab, NPCParent.transform) as GameObject;
        yield return new WaitForSeconds(0.2f); //This could have been WaitForEndOfFrame()
        newTheif.GetComponent<Assassin>().Incarnate();
    }
}
