using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Healer : Person
{
    private void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        animations = gameObject.GetComponent<Animation>();
        wayPoints = new List<Vector3>();
        wayPointTargets = new List<GameObject>();
        idlePoints = new List<Vector3>();
        idlePointTargets = new List<GameObject>();
        startingPosition = gameObject.transform.position;

        Income = Random.Range(0, 10);
        Income *= 10;
    }

    public void AnnounceDead(GameObject target)
    {
        if (target == gameObject || status == DEAD)
            return;
        wayPoints.Add(target.transform.position);
        wayPointTargets.Add(target);
        Incarnate();
    }

    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == int.MinValue)
            status = ACTIVE;
        if (status == DEAD)
            status = ACTIVE;
        SetWaypoints();

        if (isIdleTarget && idlePointTargets[targetIndex] != null)
            NavigateToWaypoint(idlePointTargets[targetIndex], idlePoints[targetIndex]);
        else if (!isIdleTarget && wayPointTargets[targetIndex] != null)
            NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
        else
            Incarnate();
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
        status = ACTIVE;
        if (targetIndex < 0 || (isIdleTarget && targetIndex >= wayPointTargets.Count))
            SetWaypoints();
        if (isIdleTarget && idlePointTargets[targetIndex] != null)
            NavigateToWaypoint(idlePointTargets[targetIndex], idlePoints[targetIndex]);
        else if (!isIdleTarget && wayPointTargets[targetIndex] != null)
            NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
        else
            Incarnate();
    }
    protected override void SetWaypoints()
    {
        #region Checking if character should go IDLE
        if (wayPoints.Count == 0)
        {
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
        if (status == DEAD)
            return;
        agent.SetDestination(waypointPos);
        animations.Play("Run");
        currentTarget = waypointTarget;
        if (isIdleTarget && currentTarget)
            currentTarget.GetComponentInParent<IdlePoint>().claimSpot();
        else if (!currentTarget && status != ASLEEP)
        {
            Incarnate();
            return;
        }
        StartCoroutine(navigateToPosition());
    }
    IEnumerator navigateToPosition()
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);



        if (isIdleTarget)
        {
            status = IDLE;
            animations.Play("Idle");
            yield return new WaitForSeconds(2.5f);
        }

        Incarnate();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Person>())
            UseAbility(other.gameObject, false);
    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        if (target.GetComponent<Person>().getStatus() != DEAD)
            return;
        StartCoroutine(reviveTarget(target.gameObject, reincarnate));
        if (reincarnate)
            Incarnate();
    }
    IEnumerator reviveTarget(GameObject target, bool reincarnate)
    {
        if (target.GetComponent<Person>())
        {
            Person npc = target.GetComponent<Person>();
            if (wayPointTargets.Contains(target))
            {
                int index = wayPointTargets.IndexOf(target);
                wayPointTargets.RemoveAt(index);
                wayPoints.RemoveAt(index);
            }
            npc.Incarnate();
            RecieveMoneyFrom(npc.gameObject, false);
            animations.Play("Gather");
            yield return new WaitForSeconds(0.5f);
            animations.Play("Run");

        }
        yield return new WaitForSeconds(0.1f);

        if (reincarnate)
            Incarnate();
    }
    protected override void RecieveMoneyFrom(GameObject target, bool reincarnate = true)
    {
        status = ACTIVE;

        if (target.GetComponent<Building>())
        {
            Building building = target.GetComponent<Building>();
            if (building.money < Income)
            {
                Money += building.money;
                building.money = 0;
            }
            else
            {
                Money += Income;
                building.money -= Income;
            }
        }
        else if (target.tag != "Investor")
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
        else
        {
            Investor investor = target.GetComponent<Investor>();
            if (investor.Money - investor.baseMoney < Income)
            {
                Money += investor.Money - investor.baseMoney;
                investor.Money = investor.baseMoney;
            }
            else
            {
                Money += Income;
                investor.Money -= Income;
            }
        }

    }




}