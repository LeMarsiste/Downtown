using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public class Healer : Person
{
    

    protected override void Initializations()
    {
        //Do Nothing?
    }

    public void AnnounceDead(GameObject target)
    {
        if (target == gameObject || status == StatusTypes.Dead)
            return;
        wayPoints.Add(target.transform.position);
        wayPointTargets.Add(target);
        Incarnate();
    }

    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == StatusTypes.Undefined)
            status = StatusTypes.Active;
        if (status == StatusTypes.Dead)
            status = StatusTypes.Active;
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
        status = StatusTypes.Active;
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
            IdlePoint[] idleSpots = RecordKeeper.Instance.GetIdlePoints().ToArray();
            targetIndex = 0;

            foreach (IdlePoint spot in idleSpots)
            {
                if (spot.GetComponentInParent<IdlePoint>().occupied == false)
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
        targetIndex = Random.Range(0, wayPointTargets.Count - 1);

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
        if (isIdleTarget && CurrentTarget)
            CurrentTarget.GetComponentInParent<IdlePoint>().ClaimSpot();
        else if (!CurrentTarget && status != StatusTypes.Asleep)
        {
            Incarnate();
            return;
        }
        StartCoroutine(NavigateToPosition());
    }
    IEnumerator NavigateToPosition()
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.1f);



        if (isIdleTarget)
        {
            status = StatusTypes.Idle;
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
        if (target.GetComponent<Person>().GetStatus() != StatusTypes.Dead)
            return;
        StartCoroutine(ReviveTarget(target.gameObject, reincarnate));
        if (reincarnate)
            Incarnate();
    }
    IEnumerator ReviveTarget(GameObject target, bool reincarnate)
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

    }




}
