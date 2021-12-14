using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Miner : Person
{

    public void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        animations = gameObject.GetComponent<Animation>();
        wayPoints = new List<Vector3>();
        wayPointTargets = new List<GameObject>();
        idlePoints = new List<Vector3>();
        idlePointTargets = new List<GameObject>();
        startingPosition = gameObject.transform.position;

        #region Finding (Static) Mines
        GameObject[] Mines = GameObject.FindGameObjectsWithTag("Mine");
        foreach (GameObject mine in Mines)
        {
            wayPointTargets.Add(mine);
            if (mine.transform.Find("Waypoint Portal"))
                wayPoints.Add(mine.transform.Find("Waypoint Portal").position);
            else
                wayPoints.Add(mine.transform.position);
        }
        #endregion

        Income = Random.Range(10, 15);
        Income *= 10;
    }
    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == int.MinValue)
            status = ACTIVE;
        if (status == DEAD)
            status = ACTIVE;
        SetWaypoints();
        if (isIdleTarget)
            NavigateToWaypoint(idlePointTargets[targetIndex], idlePoints[targetIndex]);
        else
            NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }
    public override void Sleep(bool forever)
    {
        status = forever ? DEAD : ASLEEP;
        animations.Stop();
        if (forever)
        {
            animations.Play("Death");
            if (currentTarget.GetComponent<Building>())
                currentTarget.GetComponent<Building>().RemoveOccupation();
            else if (currentTarget.GetComponent<IdlePoint>())
                currentTarget.GetComponent<IdlePoint>().freeSpot();
        }
        StopAllCoroutines();
        agent.SetDestination(gameObject.transform.position);

    }

    public override void WakeUp()
    {
        status = ACTIVE;
        if (targetIndex == -1)
            SetWaypoints();
        NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }

    protected override void SetWaypoints()
    {
        List<GameObject> possibleTargets = new List<GameObject>();
        List<Vector3> possibleTargetsPos = new List<Vector3>();


        for (int i = 0; i < wayPointTargets.Count; i++)
        {
            GameObject mine = wayPointTargets[i];
            if (mine != null && (currentTarget == null || mine.name != currentTarget.name) && mine.GetComponent<Mine>().occupied == false)
            {
                possibleTargets.Add(mine);
                possibleTargetsPos.Add(wayPoints[i]);
            }
        }

        #region Checking if character should go IDLE
        if (possibleTargets.Count == 0)
        {
            isIdleTarget = true;
            GameObject[] idleSpots = GameObject.FindGameObjectsWithTag("Environment");
            targetIndex = 0;

            foreach (GameObject spot in idleSpots)
            {
                if (spot.GetComponent<IdlePoint>() && spot.GetComponent<IdlePoint>().occupied == false)
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
        targetIndex = Random.Range(0, possibleTargets.Count - 1);
        targetIndex = wayPointTargets.IndexOf(possibleTargets[targetIndex]);

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
        if (isIdleTarget)
            currentTarget.GetComponentInParent<IdlePoint>().claimSpot();
        else
            currentTarget.GetComponent<Building>().GetOccupiedBy(gameObject);
        StartCoroutine(navigateToPosition());

    }
    int wtf = 0;
    IEnumerator navigateToPosition()
    {
        wtf++;
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Incarnate();
            if (currentTarget.GetComponent<Building>())
                currentTarget.GetComponent<Building>().RemoveOccupation();
        }
        else if (status != DEAD)
        {
            if (isIdleTarget)
            {
                status = IDLE;
                animations.Play("Idle");
                yield return new WaitForSeconds(2.5f);
                Incarnate();
            }
            else
            {
                status = IMMUNE;
                animations.Play("Mine1/Attack1");
                UseAbility(currentTarget);
            }
        }

    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        StartCoroutine(startMining(target, reincarnate));
    }

    IEnumerator startMining(GameObject target, bool incarnate = true)
    {
        if (target.tag == "Mine")
        {
            yield return new WaitForSeconds(5f);
            RecieveMoneyFrom(target);
        }
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
            building.RemoveOccupation();
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

        if (reincarnate)
            Incarnate();
    }

}
