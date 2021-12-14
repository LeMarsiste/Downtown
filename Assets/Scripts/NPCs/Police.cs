using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Police : Person
{
    GameObject prison, prisonWaypoint;
    Prison prisonScript;
    private void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        animations = gameObject.GetComponent<Animation>();
        wayPoints = new List<Vector3>();
        wayPointTargets = new List<GameObject>();
        idlePoints = new List<Vector3>();
        idlePointTargets = new List<GameObject>();
        startingPosition = gameObject.transform.position;

        #region Finding House Objects (why? its funny)
        GameObject[] Houses = GameObject.FindGameObjectsWithTag("House");

        foreach (GameObject house in Houses)
        {
            wayPointTargets.Add(house);
            wayPoints.Add(house.transform.position);
        }
        #endregion

        prison = GameObject.FindGameObjectWithTag("Prison");
        prisonWaypoint = prison.transform.Find("Waypoint Portal").gameObject;
        prisonScript = prison.GetComponent<Prison>();

        wayPoints.Add(prisonWaypoint.transform.position);
        wayPointTargets.Add(prison);

        Income = Random.Range(0, 10);
        Income *= 10;
    }
    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == int.MinValue)
            status = IMMUNE;
        SetWaypoints();
        NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }

    public override void Sleep(bool forever)
    {
        status = forever ? DEAD : ASLEEP;
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
        status = IMMUNE;
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
        if (status == DEAD || status == ASLEEP)
            return;
        agent.SetDestination(waypointPos);
        animations.Play("Run");
        currentTarget = waypointTarget;
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
    public void getBribed()
    {
        StopAllCoroutines();
        Incarnate();
    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        if (target.GetComponent<Person>().getStatus() != ACTIVE && target.GetComponent<Person>().getStatus() != IDLE)
            return;

        StopAllCoroutines();

        agent.SetDestination(target.transform.position);
        animations.Play("Run");
        currentTarget = target;

        if (target.GetComponent<Assassin>() && target.GetComponent<Assassin>().getStatus() != DEAD)
            target.GetComponent<Assassin>().SpottedByPolice();
        else if (target.GetComponent<Theif>() && target.GetComponent<Theif>().getStatus() != DEAD)
            target.GetComponent<Theif>().SpottedByPolice();

        StartCoroutine(arrestTheCriminal(target, reincarnate));
    }
    IEnumerator arrestTheCriminal(GameObject target, bool incarnate = true)
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);

        agent.SetDestination(prisonWaypoint.transform.position);
        animations.Play("Run");
        currentTarget = target;

        if (target.GetComponent<Assassin>())
            target.GetComponent<Assassin>().InteractWithPolice(this, prisonWaypoint.transform.position);
        else if (target.GetComponent<Theif>())
            target.GetComponent<Theif>().InteractWithPolice(this, prisonWaypoint.transform.position);

        if (target.GetComponent<Assassin>())
            prisonScript.QueueAssassinSpawn();
        else if (target.GetComponent<Theif>())
            prisonScript.QueueTheifSpawn();

        StartCoroutine(moveTowardsPrison(target,incarnate));
    }
    IEnumerator moveTowardsPrison(GameObject target, bool incarnate = true)
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
