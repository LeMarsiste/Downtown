using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public abstract class Person : MonoBehaviour
{
    
    protected statusTypes status = statusTypes.Undefined;

    protected List<Vector3> wayPoints,idlePoints;
    protected List<GameObject> wayPointTargets,idlePointTargets;
    public GameObject currentTarget;
    protected int targetIndex = -1;
    protected bool isIdleTarget;

    protected Animation animations;

    protected NavMeshAgent agent;
    protected Vector3 agentsLastVelocity,startingPosition;

    public int Money, Income;
    
    protected virtual void Awake()
    {
        if (RecordKeeper.Instance == null)
            RecordKeeper.Instance = GameObject.Find("RecordKeeper").GetComponent<RecordKeeper>(); //This was necessary
        RecordKeeper.Instance.AddEntity(gameObject);
    }

    protected virtual void Start()
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

        initializations();
    }
    protected abstract void initializations();
    // (Re)Incarnates the character and initiates the thinking process
    public abstract void Incarnate();
    // Stops the activities of the character (and restarts them if needed AKA Kills the character) till it is Incarnated again
    public abstract void Sleep(bool forever);
    // Resumes the activies of the character (provided it has been put to sleep)
    public abstract void WakeUp();
    // Finds and Randomizes the possible waypoints for the character
    protected abstract void SetWaypoints(); 
    // Navigates to a waypoint using the agent and declares its movement (Changes status to IDLE if needed)
    protected abstract void NavigateToWaypoint(GameObject waypointTarget, Vector3 waypointPos);
    // Upon reaching the target uses the ability (Changes status to IMMUNE if needed)
    protected abstract void UseAbility(GameObject target,bool reincarnate = true);
    // Upon Completing the Ability Reduces the targets money (Changes status to ACTIVE if needed)
    protected abstract void RecieveMoneyFrom(GameObject target, bool reincarnate = true);
    // Returns the Status of this Character
    public statusTypes getStatus() {
        return status;
    }
    
    
}
