using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateCharge : PlayerState
{

    public StateCharge(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority)
    {
        stateColor = Color.yellow;
    }
    
    public override void Enter(bool trigger = true) {
        owner.animator.SetBool("feinting", false);
        base.Enter();
        if (actionInfos.direction == eDIRECTION.UP) {
            owner.animator.speed = 0.5f;
        }
        //actionInfos.currentCooldownDuration = owner.strike.currentActionDuration + actionInfos.baseCooldownDuration;
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void Exit(bool reset = false) {
        owner.animator.speed = 1f;
        owner.furyChange(actionInfos.furyModificationOnSuccess); //change the fury of a fixed amount
        base.Exit(reset);
    }
}
