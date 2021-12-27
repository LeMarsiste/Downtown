using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public class Theif : Person
{
    public bool IsBriberTheif;
    [Tooltip("Leave Empty if IsBriberTheif is not checked!")]
    public int BribeMoney = 0;

    bool caughtByPolice = false;

    protected override void Initializations()
    {
        #region Finding (Static) Houses
        House[] Houses = RecordKeeper.Instance.GetHouses().ToArray();

        foreach (House houseScript in Houses)
        {
            GameObject house = houseScript.gameObject;
            wayPointTargets.Add(house);
            if (houseScript.WaypointPortal != null)
                wayPoints.Add(houseScript.WaypointPortal.transform.position);
            else
                wayPoints.Add(house.transform.position);
        }
        #endregion

        Income = Random.Range(10, 15) * 10;
        if (IsBriberTheif && BribeMoney == 0)
            BribeMoney = Random.Range(10, 15) * 10;
    }
    

    public override void Sleep(bool forever)
    {
        status = forever ? StatusTypes.Dead : StatusTypes.Asleep;
        //animations.Stop();
        
        agent.SetDestination(gameObject.transform.position);
        if (forever)
        {
            animations.Play("Death");
            if (CurrentTarget.GetComponent<Building>())
                CurrentTarget.GetComponent<Building>().RemoveOccupation();
            else if (CurrentTarget.GetComponent<IdlePoint>())
                CurrentTarget.GetComponent<IdlePoint>().FreeSpot();
        }
        StopAllCoroutines();
    }

    public override void WakeUp()
    {
        status = StatusTypes.Active;
        if (targetIndex == -1)
            SetWaypoints();
        if (isIdleTarget)
            NavigateToWaypoint(idlePointTargets[targetIndex], idlePoints[targetIndex]);
        else
            NavigateToWaypoint(wayPointTargets[targetIndex], wayPoints[targetIndex]);
    }
    protected override void SetWaypoints()
    {
        List<GameObject> possibleTargets = new List<GameObject>();
        List<Vector3> possibleTargetsPos = new List<Vector3>();


        for (int i = 0; i < wayPointTargets.Count; i++)
        {
            GameObject House = wayPointTargets[i];
            if (House != null && (CurrentTarget == null || House.name != CurrentTarget.name) && House.GetComponent<House>().Occupied == false)
            {
                possibleTargets.Add(House);
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
                if (spot.GetComponent<IdlePoint>().occupied == false)
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
        if (status == StatusTypes.Dead || status == StatusTypes.Asleep)
            return;
        agent.SetDestination(waypointPos);
        animations.Play("Run");
        CurrentTarget = waypointTarget;

        if (isIdleTarget && CurrentTarget)
            CurrentTarget.GetComponentInParent<IdlePoint>().ClaimSpot();
        else if (!isIdleTarget && CurrentTarget)
            CurrentTarget.GetComponent<Building>().GetOccupiedBy(gameObject);
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
                animations.Play("Gather");
                UseAbility(CurrentTarget);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Person>() && other.GetComponent<Person>().GetStatus() == StatusTypes.Active && !caughtByPolice)
            UseAbility(other.gameObject, false);
    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        StartCoroutine(StealFrom(target, reincarnate));
    }

    IEnumerator StealFrom(GameObject target, bool incarnate = true)
    {
        if (target.GetComponent<House>())
        {
            yield return new WaitForSeconds(5f);
            RecieveMoneyFrom(target);
        }
        else if (target.GetComponent<Person>())
        {
            yield return new WaitForSeconds(0.1f);
            RecieveMoneyFrom(target, incarnate);
        }
    }


    public void SpottedByPolice()
    {
        gameObject.tag = "Imprisoned";
        status = StatusTypes.Immune;
        StopAllCoroutines();
        animations.Play("Talk");
        agent.SetDestination(gameObject.transform.position);
    }
    public void InteractWithPolice(Police policeScript, Vector3 prisonPos)
    {
        if (IsBriberTheif && Money >= BribeMoney)
        {
            policeScript.GetBribed();
            policeScript.Money += BribeMoney;
            Money -= BribeMoney;
            StartCoroutine(Camoflauge());
            status = StatusTypes.Active;
        }
        else
        {
            animations.Play("Run");
            agent.SetDestination(prisonPos);
            StartCoroutine(GoToPrison());
        }
    }
    IEnumerator Camoflauge()
    {
        gameObject.tag = "Worker";
        yield return new WaitForSeconds(15f);
        gameObject.tag = IsBriberTheif ? "Briber" : "Theif";
        Incarnate();
    }
    IEnumerator GoToPrison()
    {
        while (agent.pathPending) //#nav_mesh_is_dumb
            yield return new WaitForEndOfFrame();

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return new WaitForSeconds(0.5f);
        // Other Possible Implementations:
        /// 1- Adding a place to put the imprisoned NPCs there and add a "escape" mechanic to the game
        ///    we can add the "imprisoned" tag to those units and do the rest (hence the reason I have an "Imprisoned" tag)
        RecordKeeper.Instance.RemoveEntity(gameObject);
        Destroy(gameObject);
    }

}
