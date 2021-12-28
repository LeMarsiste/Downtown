using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;
using Mars.Tools;

public class Miner : Person
{
    private void Awake()
    {
        RecordKeeper.Instance.AddPerson<Miner>(gameObject);
    }

    protected override void Initializations()
    {
        #region Finding (Static) Mines
        Building[] Mines = RecordKeeper.Instance.GetBuildings<Mine>().ToArray();
        foreach (Building mineScript in Mines)
        {
            GameObject mine = mineScript.gameObject;

            wayPointTargets.Add(mine);
            if (mineScript.WaypointPortal != null)
                wayPoints.Add(mineScript.WaypointPortal.transform.position);
            else
                wayPoints.Add(mine.transform.position);
        }
        #endregion

        Income = Random.Range(10, 15) * 10;
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
            if (mine != null && (CurrentTarget == null || mine.name != CurrentTarget.name) && mine.GetComponent<Mine>().Occupied == false)
            {
                possibleTargets.Add(mine);
                possibleTargetsPos.Add(wayPoints[i]);
            }
        }

        #region Checking if character should go IDLE
        if (possibleTargets.Count == 0)
        {
            isIdleTarget = true;
            IdlePoint[] idleSpots = RecordKeeper.Instance.GetIdlePoints<IdlePoint>().ToArray();
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
    IEnumerator navigateToPosition()
    {
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
}
