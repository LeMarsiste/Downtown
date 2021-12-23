using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public class Investor : Person
{
    int timeSpentInvesting = 0;
    public int baseMoney;
    public float profitPercentage;
    // Start is called before the first frame update
    void Start()
    {
        #region Initializations
        agent = gameObject.GetComponent<NavMeshAgent>();
        animations = gameObject.GetComponent<Animation>();
        Money = Random.Range(0, 200);  //no exact number was specified in the Docs
        profitPercentage = Random.Range(0.01f, 0.5f); // no exact number was specified in the Docs
        baseMoney = Money;
        #endregion

        
        

    }
    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == statusTypes.Undefined)
            status = statusTypes.Active;
        if (status == statusTypes.Dead)
            status = statusTypes.Active;
        StartCoroutine(invest());
    }

    public override void Sleep(bool forever)
    {
        status = forever ? statusTypes.Dead : statusTypes.Asleep;
        animations.Stop();
        if (forever)
            animations.Play("Death");
        StopAllCoroutines();
        
    }

    public override void WakeUp()
    {
        status = statusTypes.Active;
        Incarnate();
    }

    IEnumerator invest()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (status != statusTypes.Dead)
            {
                if (++timeSpentInvesting >= 5)
                {
                    UseAbility(null);
                    timeSpentInvesting -= 5;
                }
            }
        }
    }
    protected override void UseAbility(GameObject target, bool reincarnate = true)
    {
        Money += (int)(Money * profitPercentage);
        animations.Play("Jump");
    }
    #region Violating SOLID(tm) 
    protected override void SetWaypoints()
    {
        //Does Nothing, Violation of SOLID (maybe we should change the definition of Person in docs?)
    }
    protected override void NavigateToWaypoint(GameObject waypointTarget, Vector3 waypointPos)
    {
        //Does Nothing, Violation of SOLID (maybe we should change the definition of Person in docs?)
    }
    protected override void RecieveMoneyFrom(GameObject target, bool reincarnate = true)
    {
        //Does Nothing, Violation of SOLID (maybe we should change the definition of Person in docs?)
    }
    #endregion
}
