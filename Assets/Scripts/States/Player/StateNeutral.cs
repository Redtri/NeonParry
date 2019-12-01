using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateNeutral : PlayerState
{

    public StateNeutral(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.white;
    }
    
    public override void Enter()
    {
        base.Enter();
    }

    public override void Update() {
        base.Update();
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
    }
}
