using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateStop : PlayerState
{
    public StateStop(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority){

    }

    public override void Enter(bool trigger = true)
    {
        //TODO it's really not clean but at least player can't strike after being hit.
        owner.allActionsOnCd(actionInfos.baseActionDuration);
        Debug.Log("entering StateStop ");
        base.Enter(false);
    }

    public override void Update()
    {
        Debug.Log("action : " + actionInfos.IsActionPerforming(Time.time));
        base.Update();
    }

    public override void Exit(bool reset = false)
    {
        Debug.Log("exit StateStop");
        owner.isHit = false;
        base.Exit(reset);
    }
}
