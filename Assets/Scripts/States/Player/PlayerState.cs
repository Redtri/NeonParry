using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eSTATE_PRIORITY { DEFAULT, OVERRIDE}

public abstract class PlayerState
{
    protected PlayerController owner;
    protected FSM_Player stateMachine;
    protected ePLAYER_STATE nextState;
    public eSTATE_PRIORITY priority { get; private set; }
    protected Color stateColor;
    public SwordAction actionInfos { get; private set; }

    public PlayerState(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) {
        owner = player;
        stateMachine = machine;
        actionInfos = infos;
        nextState = next;
        priority = statePriority;
    }

    public virtual void Enter(bool trigger = true) {
        owner.sprRenderer.color = stateColor;
        //Send to the animator, the enum value converted to string of the current state as trigger event
        if (trigger) {
            owner.animator.SetTrigger(stateMachine.currentState.ToString().ToLower());
            owner.cursorAnimator.SetTrigger(stateMachine.currentState.ToString().ToLower());
        }
        if (actionInfos != null) {
            if (actionInfos.samples != null) {
                if (actionInfos.samples.actionSounds.Length > 1) {
                    actionInfos.samples.actionSounds[(int)actionInfos.direction - 1].Post(owner.gameObject);
                } else if (actionInfos.samples.actionSounds.Length > 0) {
                    actionInfos.samples.actionSounds[0].Post(owner.gameObject);
                }
                actionInfos.Refresh(Time.time);
                owner.animator.SetInteger("direction", (int)actionInfos.direction);
                //Refresh animation clips speeds
                owner.animator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
                owner.cursorAnimator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
            }
        }
    }

    public virtual void Update() {
        if(actionInfos != null) {
            if (!actionInfos.IsActionPerforming(Time.time)) {
                stateMachine.ChangeState(nextState);
            }
        }
    }

    public virtual void Exit(bool reset = false) {
        if(actionInfos != null) {
            if (reset) {
                actionInfos.BlankRefreshTime(Time.time);
            }
        }
        owner.animator.ResetTrigger(stateMachine.currentState.ToString().ToLower());
        owner.cursorAnimator.ResetTrigger(stateMachine.currentState.ToString().ToLower());
    }
}
