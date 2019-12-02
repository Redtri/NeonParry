using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateStrike : PlayerState
{
    private bool opponentParried;
    private bool opponentDashed;

    public StateStrike(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.magenta;
    }
    
    public override void Enter()
    {
        base.Enter();
        opponentParried = false;
        opponentDashed = false;
}

    public override void Update() {
        base.Update();
        if(owner.opponent != null) {
            if (owner.opponent.parry.IsActionPerforming(Time.time, actionInfos.direction)) {
                Debug.Log("Opponent parried successfully");
                opponentParried = true;
                stateMachine.ChangeState(nextState);
            } else if (owner.opponent.dash.IsActionPerforming(Time.time)) {
                Debug.Log("Opponent dodged, coward !");
                opponentDashed = true;
            }
        }
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
        if (owner.opponent != null) {
            if (!opponentParried && !opponentDashed) {
                Debug.Log("Opponent being stroke successfully");
                GameManager.instance.StrikeSuccessful(owner.playerIndex);
            }
        }
    }
}
