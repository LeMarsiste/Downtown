using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public class Assassin : Person
{
    List<Person> healers;

    bool caughtByPolice = false;

    protected override void initializations()
    {
        #region Finding all the healers
        healers = new List<Person>(RecordKeeper.Instance.GetHealers());
        #endregion
    }

    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == statusTypes.Undefined)
            status = statusTypes.Active;
        if (status == statusTypes.Dead)
            status = statusTypes.Active;
        SetWaypoints();
        if (isIdleTarget)
            NavigateToWaypoint(idlePointTargets[targetIndex], idlePoints[targetIndex]);
        else
            NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }

    public override void Sleep(bool forever)
    {
        status = forever ? statusTypes.Dead : statusTypes.Asleep;
        StopAllCoroutines();
        agent.SetDestination(gameObject.transform.position);
        animations.Stop();
    }

    public override void WakeUp()
    {
        status = statusTypes.Active;
        if (targetIndex == -1)
            SetWaypoints();
        NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);

    }

    protected override void SetWaypoints()
    {
        wayPoints = new List<Vector3>();
        wayPointTargets = new List<GameObject>();
        

        Person[] NPCs = FindObjectsOfType<Person>();
        foreach (Person npc in NPCs)
        {
            if (npc.gameObject != gameObject && npc.getStatus() == statusTypes.Active)
            {
                wayPointTargets.Add(npc.gameObject);
                wayPoints.Add(npc.gameObject.transform.position);
            }
        }

        #region Checking if character should go IDLE
        if (wayPoints.Count == 0)
        {
            if (isIdleTarget)
                return;

            idlePoints = new List<Vector3>();
            idlePointTargets = new List<GameObject>();

            isIdleTarget = true;

            GameObject[] idleSpots = GameObject.FindGameObjectsWithTag("Environment");
            targetIndex = 0;

            foreach (GameObject spot in idleSpots)
            {
                if (spot.GetComponentInParent<IdlePoint>() && spot.GetComponentInParent<IdlePoint>().occupied == false)
                {
                    idlePointTargets.Add(spot);
                    idlePoints.Add(spot.transform.position);
                    return;

                }
            }

            idlePoints.Clear(); idlePointTargets.Clear();
            idlePoints.Add(startingPosition);
            idlePointTargets.Add(gameObject);

            return;

        }
        #endregion

        #region Finding a Valid waypoint
        targetIndex = Random.Range(0, wayPointTargets.Count - 1);

        if (isIdleTarget)
            currentTarget.GetComponentInParent<IdlePoint>().freeSpot();
        isIdleTarget = false;

        if (wayPointTargets[targetIndex] == null)
        {
            wayPointTargets.RemoveAt(targetIndex);
            wayPoints.RemoveAt(targetIndex);
            SetWaypoints();
        }
        #endregion
    }

    protected override void NavigateToWaypoint(GameObject waypointTarget, Vector3 waypointPos)
    {
        agent.SetDestination(waypointPos);
        animations.Play("Run");
        currentTarget = waypointTarget;
        if (isIdleTarget)
            currentTarget.GetComponentInParent<IdlePoint>().claimSpot();
        StartCoroutine(navigateToPosition());
    }
    IEnumerator navigateToPosition()
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            Incarnate();
        else
        {
            if (isIdleTarget)
            {
                status = statusTypes.Idle;
                animations.Play("Idle");
                yield return new WaitForSeconds(2.5f);
            }
            else
                status = statusTypes.Active;
            Incarnate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Person>() && other.GetComponent<Person>().getStatus() == statusTypes.Active && status == statusTypes.Active && !caughtByPolice)
            UseAbility(other.gameObject);
    }
    IEnumerator killTarget(GameObject target,bool reincarnate)
    {
        if (target.GetComponent<Person>())
        {
            Person npc = target.GetComponent<Person>();
            npc.Sleep(true);
            animations.Play("Mine2/Attack2");
            yield return new WaitForSeconds(0.5f);
            animations.Play("Run");
            foreach (Healer healer in healers)
                healer.AnnounceDead(target);
        }
        yield return new WaitForSeconds(0.1f);
        if (reincarnate)
            Incarnate();
    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        StartCoroutine(killTarget(target.gameObject,reincarnate));
        
    }
    protected override void RecieveMoneyFrom(GameObject target, bool reincarnate = true)
    {
        if (target.GetComponent<Person>())
        {
            Person npc = target.GetComponent<Person>();
            if (npc.Money < Income)
            {
                Money += npc.Money;
                npc.Money = 0;
            }
            else
            {
                Money += Income;
                npc.Money -= Income;
            }

        }
    }
    public void SpottedByPolice()
    {
        gameObject.tag = "Imprisoned";
        status = statusTypes.Immune;
        StopAllCoroutines();
        caughtByPolice = true;
        animations.Play("Talk");
        agent.SetDestination(gameObject.transform.position);
    }
    public void InteractWithPolice(Police policeScript, Vector3 prisonPos)
    {
        animations.Play("WalkFront");
        agent.SetDestination(prisonPos);
        StartCoroutine(goToPrison());
    }
    IEnumerator goToPrison()
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.5f);
        // Other Possible Implementations:
        /// 1- Adding a place to put the imprisoned NPCs there and add a "escape" mechanic to the game
        ///    we can add the "imprisoned" tag to those units and do the rest (hence the reason I have an "Imprisoned" tag)
        Destroy(gameObject);
    }
}
