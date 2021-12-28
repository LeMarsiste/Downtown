using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public abstract class Person : MonoBehaviour
{

    protected StatusTypes status = StatusTypes.Undefined;

    protected List<Vector3> wayPoints, idlePoints;
    protected List<GameObject> wayPointTargets, idlePointTargets;
    public GameObject CurrentTarget;
    protected int targetIndex = -1;
    protected bool isIdleTarget;

    protected Animator animations;

    protected NavMeshAgent agent;
    protected Vector3 agentsLastVelocity, startingPosition;

    public int Money, Income;
    protected virtual void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        animations = gameObject.GetComponent<Animator>();
        animations.Play("Idle");
        wayPoints = new List<Vector3>();
        wayPointTargets = new List<GameObject>();
        idlePoints = new List<Vector3>();
        idlePointTargets = new List<GameObject>();
        startingPosition = gameObject.transform.position;

        Income = Random.Range(0, 10);
        Income *= 10;

        Initializations();
    }
    protected abstract void Initializations();
    // (Re)Incarnates the character and initiates the thinking process
    public virtual void Incarnate()
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
    // Stops the activities of the character (and restarts them if needed AKA Kills the character) till it is Incarnated again
    public abstract void Sleep(bool forever);
    // Resumes the activies of the character (provided it has been put to sleep)
    public abstract void WakeUp();
    // Finds and Randomizes the possible waypoints for the character
    protected abstract void SetWaypoints();
    // Navigates to a waypoint using the agent and declares its movement (Changes status to IDLE if needed)
    protected abstract void NavigateToWaypoint(GameObject waypointTarget, Vector3 waypointPos);
    // Upon reaching the target uses the ability (Changes status to IMMUNE if needed)
    protected abstract void UseAbility(GameObject target, bool reincarnate = true);
    // Upon Completing the Ability Reduces the targets money (Changes status to ACTIVE if needed)
    protected virtual void RecieveMoneyFrom(GameObject target, bool reincarnate = true)
    {
        status = StatusTypes.Active;

        if (target.GetComponent<Building>())
        {

            Building building = target.GetComponent<Building>();
            if (building.Money < Income)
            {
                Money += building.Money;
                building.Money = 0;
            }
            else
            {
                Money += Income;
                building.Money -= Income;
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
    // Returns the Status of this Character
    public StatusTypes GetStatus()
    {
        return status;
    }


}
