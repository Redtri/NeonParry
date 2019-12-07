using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateNeutral : PlayerState
{

    public StateNeutral(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.white;
    }
    
    public override void Enter(bool trigger = true) {
        //owner.sprRenderer.color = stateColor;
        //Send to the animator, the enum value converted to string of the current state as trigger event
        if (trigger) {
            owner.animator.SetTrigger(stateMachine.currentState.ToString().ToLower());
            owner.cursorAnimator.SetTrigger(stateMachine.currentState.ToString().ToLower());
        }
        owner.animator.SetFloat("duration_neutral", 1f + ((float)owner.fury.currentFury/(float)owner.fury.highestValueOfFury));
    }

    public override void Update() {
        base.Update();
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
    }
}
