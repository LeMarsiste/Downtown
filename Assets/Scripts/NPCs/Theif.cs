using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Theif : Person
{
    public bool IsBriberTheif;
    [Tooltip("Leave Empty if IsBriberTheif is not checked!")]
    public int BribeMoney = 0;

    bool caughtByPolice = false;
    private void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        animations = gameObject.GetComponent<Animation>();
        wayPoints = new List<Vector3>();
        wayPointTargets = new List<GameObject>();
        idlePoints = new List<Vector3>();
        idlePointTargets = new List<GameObject>();
        startingPosition = gameObject.transform.position;

        #region Finding (Static) Houses
        GameObject[] Houses = GameObject.FindGameObjectsWithTag("House");
        foreach (GameObject house in Houses)
        {
            wayPointTargets.Add(house);
            if (house.transform.Find("Waypoint Portal"))
                wayPoints.Add(house.transform.Find("Waypoint Portal").position);
            else
                wayPoints.Add(house.transform.position);
        }
        #endregion

        Income = Random.Range(10, 15) *10;
        if (IsBriberTheif && BribeMoney == 0)
            BribeMoney = Random.Range(10, 15) * 10;

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
        agent.SetDestination(gameObject.transform.position);
        if (forever)
        {
            animations.Play("Death");
            if (currentTarget.GetComponent<Building>())
                currentTarget.GetComponent<Building>().RemoveOccupation();
            else if (currentTarget.GetComponent<IdlePoint>())
                currentTarget.GetComponent<IdlePoint>().freeSpot();
        }
        StopAllCoroutines();
    }

    public override void WakeUp()
    {
        status = ACTIVE;
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
            if (House != null && (currentTarget == null || House.name != currentTarget.name) && House.GetComponent<House>().occupied == false)
            {
                possibleTargets.Add(House);
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
        if (status == DEAD || status == ASLEEP)
            return;
        agent.SetDestination(waypointPos);
        animations.Play("Run");
        currentTarget = waypointTarget;

        if (isIdleTarget && currentTarget)
            currentTarget.GetComponentInParent<IdlePoint>().claimSpot();
        else if (!isIdleTarget && currentTarget)
            currentTarget.GetComponent<Building>().GetOccupiedBy(gameObject);
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
                animations.Play("Gather");
                UseAbility(currentTarget);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Person>() && other.GetComponent<Person>().getStatus() == ACTIVE && !caughtByPolice)
            UseAbility(other.gameObject, false);
    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        StartCoroutine(stealFrom(target, reincarnate));
    }

    IEnumerator stealFrom(GameObject target, bool incarnate = true)
    {
        if (target.tag == "House")
        {
            yield return new WaitForSeconds(5f);
            RecieveMoneyFrom(target);
        }else if (target.GetComponent<Person>())
        {
            yield return new WaitForSeconds(0.1f);
            RecieveMoneyFrom(target, incarnate);
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


    public void SpottedByPolice()
    {
        gameObject.tag = "Imprisoned";
        status = IMMUNE;
        StopAllCoroutines();
        animations.Play("Talk");
        agent.SetDestination(gameObject.transform.position);
    }
    public void InteractWithPolice(Police policeScript, Vector3 prisonPos)
    {
        if (IsBriberTheif && Money >= BribeMoney)
        {
            policeScript.getBribed();
            policeScript.Money += BribeMoney;
            Money -= BribeMoney;
            StartCoroutine(camoflauge());
            status = ACTIVE;
        }
        else
        {
            animations.Play("WalkFront");
            agent.SetDestination(prisonPos);
            StartCoroutine(goToPrison());
        }
    }
    IEnumerator camoflauge()
    {
        gameObject.tag = "Worker";
        yield return new WaitForSeconds(15f);
        gameObject.tag = IsBriberTheif ? "Briber" : "Theif";
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
