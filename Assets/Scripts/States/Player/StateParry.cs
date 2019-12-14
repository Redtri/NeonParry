using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateParry : PlayerState
{
    private bool parrySuccessful;
    private bool perfectParry;

    public StateParry(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.cyan;
    }
    
    public override void Enter(bool trigger = true)
    {
        base.Enter();
        parrySuccessful = false;
        perfectParry = false;
    }

    public override void Update() {
        base.Update();
        if(owner.opponent != null) {
            if (owner.opponent.strikeAction.IsActionPerforming(Time.time, actionInfos.direction)) {
                parrySuccessful = true;
                if (owner.opponent.strikeAction.GetPercentTime(Time.time) >= owner.opponent.strikeAction.perfectPercentTiming) {
                    Debug.Log("Perfect parry");
                    perfectParry = true;
                    owner.furyChange(actionInfos.furyModificationOnSuccess*owner.fury.percentagePerfectParyFury);
                } else {
                    owner.furyChange(actionInfos.furyModificationOnSuccess); //change the fury of a fixed amount
                }
                stateMachine.ChangeState(nextState, true);
            }
        }
    }

    public override void Exit(bool reset = false) {
        if (parrySuccessful) {
            owner.fxHandler.SpawnFX(ePLAYER_STATE.PARRY, actionInfos.direction, owner.facingLeft);
            if (perfectParry) {
                actionInfos.currentSamples.additionalSounds[0].Post(owner.gameObject);
            } else {
                actionInfos.currentSamples.additionalSounds[1].Post(owner.gameObject);
            }
            if (actionInfos != null) {
                if (reset) {
                    actionInfos.BlankRefreshTime(Time.time);
                }
            }
        }
    }
}
