using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mars.Tools;

public class Investor : Person
{
    int timeSpentInvesting = 0;
    public int BaseMoney;
    public float ProfitPercentage;

    protected override void Initializations()
    {
        Money = Random.Range(0, 200);  //no exact number was specified in the Docs
        ProfitPercentage = Random.Range(0.01f, 0.5f); // no exact number was specified in the Docs
        BaseMoney = Money;
    }

    public override void Incarnate()
    {
        StopAllCoroutines();
        if (status == StatusTypes.Undefined)
            status = StatusTypes.Active;
        if (status == StatusTypes.Dead)
            status = StatusTypes.Active;
        StartCoroutine(Invest());
    }

    public override void Sleep(bool forever)
    {
        status = forever ? StatusTypes.Dead : StatusTypes.Asleep;
        animations.Stop();
        if (forever)
            animations.Play("Death");
        StopAllCoroutines();
        
    }

    public override void WakeUp()
    {
        status = StatusTypes.Active;
        Incarnate();
    }

    IEnumerator Invest()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (status != StatusTypes.Dead)
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
        Money += (int)(Money * ProfitPercentage);
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
