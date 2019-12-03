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
    
    public override void Enter()
    {
        base.Enter();
        actionInfos.currentCooldownDuration = owner.strike.currentActionDuration + actionInfos.baseCooldownDuration;
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void Exit(bool reset = false)
    {
        owner.furyChange(actionInfos.furyModificationOnSuccess); //change the fury of a fixed amount
        base.Exit(reset);
    }
}
