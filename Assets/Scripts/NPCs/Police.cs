using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public class Police : Person
{
    GameObject prison, prisonWaypoint;
    Prison prisonScript;

    protected override void Initializations()
    {
        #region Finding House Objects (why? its funny)
        House[] Houses = RecordKeeper.Instance.GetHouses().ToArray(); 

        foreach (House houseScript in Houses)
        {
            GameObject house = houseScript.gameObject;
            wayPointTargets.Add(house);
            wayPoints.Add(house.transform.position);
        }
        #endregion

        prison = RecordKeeper.Instance.GetPrisons()[0].gameObject;
        prisonWaypoint = prison.transform.Find("Waypoint Portal").gameObject;
        prisonScript = prison.GetComponent<Prison>();

        wayPoints.Add(prisonWaypoint.transform.position);
        wayPointTargets.Add(prison);
    }

    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == StatusTypes.Undefined)
            status = StatusTypes.Immune;
        SetWaypoints();
        NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }

    public override void Sleep(bool forever)
    {
        status = forever ? StatusTypes.Dead : StatusTypes.Asleep;
        animations.Stop();
        if (forever)
        {
            animations.Play("Death");
            gameObject.GetComponent<Healer>().enabled = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        agent.SetDestination(gameObject.transform.position);
        StopAllCoroutines();
    }

    public override void WakeUp()
    {
        status = StatusTypes.Immune;
        if (targetIndex == -1)
            SetWaypoints();
        NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }
    protected override void SetWaypoints()
    {
        int prevIndex = targetIndex;
        while (targetIndex == prevIndex)
            targetIndex = Random.Range(0, wayPoints.Count - 1);
    }
    protected override void NavigateToWaypoint(GameObject waypointTarget, Vector3 waypointPos)
    {
        if (status == StatusTypes.Dead || status == StatusTypes.Asleep)
            return;
        agent.SetDestination(waypointPos);
        animations.Play("Run");
        CurrentTarget = waypointTarget;
        StartCoroutine(navigateToPosition());
    }

    IEnumerator navigateToPosition()
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);

        Incarnate();

    }

    private void OnTriggerEnter(Collider other)
    {
        // Alternative Solution : using mesh and raycast to simulate a 170 degree arc to simulate the vision range... but that one got buggy 
        if (other.tag == "Theif" || other.tag == "Briber" || other.tag == "Assassin")
            UseAbility(other.gameObject);
    }
    public void GetBribed()
    {
        StopAllCoroutines();
        Incarnate();
    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        if (target.GetComponent<Person>().GetStatus() != StatusTypes.Active && target.GetComponent<Person>().GetStatus() != StatusTypes.Idle)
            return;

        StopAllCoroutines();

        agent.SetDestination(target.transform.position);
        animations.Play("Run");
        CurrentTarget = target;

        if (target.GetComponent<Assassin>() && target.GetComponent<Assassin>().GetStatus() != StatusTypes.Dead)
            target.GetComponent<Assassin>().SpottedByPolice();
        else if (target.GetComponent<Theif>() && target.GetComponent<Theif>().GetStatus() != StatusTypes.Dead)
            target.GetComponent<Theif>().SpottedByPolice();

        StartCoroutine(ArrestTheCriminal(target, reincarnate));
    }
    IEnumerator ArrestTheCriminal(GameObject target, bool incarnate = true)
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);

        agent.SetDestination(prisonWaypoint.transform.position);
        animations.Play("Run");
        CurrentTarget = target;

        if (target.GetComponent<Assassin>())
            target.GetComponent<Assassin>().InteractWithPolice(this, prisonWaypoint.transform.position);
        else if (target.GetComponent<Theif>())
            target.GetComponent<Theif>().InteractWithPolice(this, prisonWaypoint.transform.position);

        if (target.GetComponent<Assassin>())
            prisonScript.QueueAssassinSpawn();
        else if (target.GetComponent<Theif>())
            prisonScript.QueueTheifSpawn();

        StartCoroutine(MoveTowardsPrison(target,incarnate));
    }
    IEnumerator MoveTowardsPrison(GameObject target, bool incarnate = true)
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);

        

        if (incarnate)
            Incarnate();
    }
    protected override void RecieveMoneyFrom(GameObject target, bool reincarnate = true)
    {
        //doesnt ask for money from anyone ... Violation of SOLID perhaps
    }
}
