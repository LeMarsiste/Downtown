using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;
using Mars.Tools;

public class Miner : Person
{

    
    protected override void Initializations()
    {
        #region Finding (Static) Mines
        Mine[] Mines = RecordKeeper.Instance.GetMines().ToArray();
        foreach (Mine mineScript in Mines)
        {
            GameObject mine = mineScript.gameObject;

            wayPointTargets.Add(mine);
            if (mine.transform.Find("Waypoint Portal"))
                wayPoints.Add(mine.transform.Find("Waypoint Portal").position);
            else
                wayPoints.Add(mine.transform.position);
        }
        #endregion

        Income = Random.Range(10, 15) * 10;
    }
    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == StatusTypes.Undefined)
            status = StatusTypes.Active;
        if (status == StatusTypes.Dead)
            status = StatusTypes.Active;
        SetWaypoints();
        if (isIdleTarget)
            NavigateToWaypoint(idlePointTargets[targetIndex], idlePoints[targetIndex]);
        else
            NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }
    public override void Sleep(bool forever)
    {
        status = forever ? StatusTypes.Dead : StatusTypes.Asleep;
        //animations.Stop();
        
        if (forever)
        {
            animations.Play("Death");
            if (CurrentTarget.GetComponent<Building>())
                CurrentTarget.GetComponent<Building>().RemoveOccupation();
            else if (CurrentTarget.GetComponent<IdlePoint>())
                CurrentTarget.GetComponent<IdlePoint>().FreeSpot();
        }
        StopAllCoroutines();
        agent.SetDestination(gameObject.transform.position);

    }

    public override void WakeUp()
    {
        status = StatusTypes.Active;
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
            if (mine != null && (CurrentTarget == null || mine.name != CurrentTarget.name) && mine.GetComponent<Mine>().occupied == false)
            {
                possibleTargets.Add(mine);
                possibleTargetsPos.Add(wayPoints[i]);
            }
        }

        #region Checking if character should go IDLE
        if (possibleTargets.Count == 0)
        {
            isIdleTarget = true;
            IdlePoint[] idleSpots = RecordKeeper.Instance.GetIdlePoints().ToArray();
            targetIndex = 0;

            foreach (IdlePoint spot in idleSpots)
            {
                if ( spot.GetComponent<IdlePoint>().occupied == false)
                {
                    idlePointTargets.Add(spot.gameObject);
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
            CurrentTarget.GetComponentInParent<IdlePoint>().FreeSpot();
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
        if (status == StatusTypes.Dead)
            return;
        agent.SetDestination(waypointPos);
        animations.Play("Run");
        CurrentTarget = waypointTarget;
        if (isIdleTarget)
            CurrentTarget.GetComponentInParent<IdlePoint>().ClaimSpot();
        else
            CurrentTarget.GetComponent<Building>().GetOccupiedBy(gameObject);
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
            if (CurrentTarget.GetComponent<Building>())
                CurrentTarget.GetComponent<Building>().RemoveOccupation();
        }
        else if (status != StatusTypes.Dead)
        {
            if (isIdleTarget)
            {
                status = StatusTypes.Idle;
                animations.Play("Idle");
                yield return new WaitForSeconds(2.5f);
                Incarnate();
            }
            else
            {
                status = StatusTypes.Immune;
                animations.Play("Mine1/Attack1");
                UseAbility(CurrentTarget);
            }
        }

    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        StartCoroutine(startMining(target, reincarnate));
    }

    IEnumerator startMining(GameObject target, bool incarnate = true)
    {
        if (target.GetComponent<Mine>())
        {
            yield return new WaitForSeconds(5f);
            RecieveMoneyFrom(target);
        }
    }

    protected override void RecieveMoneyFrom(GameObject target, bool reincarnate = true)
    {
        status = StatusTypes.Active;

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
        else if (!target.GetComponent<Investor>())
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
            if (investor.Money - investor.BaseMoney < Income)
            {
                Money += investor.Money - investor.BaseMoney;
                investor.Money = investor.BaseMoney;
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
