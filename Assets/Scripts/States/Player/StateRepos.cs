using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateRepos : PlayerState
{
    private bool opponentParried;

    public StateRepos(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.magenta;
    }
    
    public override void Enter()
    {
        base.Enter();
        opponentParried = false;
    }

    public override void Update() {
        base.Update();
        if(owner.opponent.parry.IsActionPerforming(Time.time, actionInfos.direction)) {
            Debug.Log("Opponent parried successfully");
            opponentParried = true;
            stateMachine.ChangeState(nextState);
        }
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
        if (!opponentParried) {
            Debug.Log("Opponent being stroke successfully");
            GameManager.instance.StrikeSuccessful(owner.playerIndex);
        }
    }
}
