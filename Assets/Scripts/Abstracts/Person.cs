using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Person : MonoBehaviour
{
    protected static int IDLE = -1, ACTIVE = 0, IMMUNE = 1,DEAD = 2,ASLEEP = 3;
    protected int status = int.MinValue;

    protected List<Vector3> wayPoints,idlePoints;
    protected List<GameObject> wayPointTargets,idlePointTargets;
    public GameObject currentTarget;
    protected int targetIndex = -1;
    protected bool isIdleTarget;

    protected Animation animations;

    protected NavMeshAgent agent;
    protected Vector3 agentsLastVelocity,startingPosition;

    public int Money, Income;
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
    public int getStatus() {
        return status;
    }
    
    
}