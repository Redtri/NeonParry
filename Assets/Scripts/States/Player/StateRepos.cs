using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateRepos : PlayerState
{
    private Vector3 startPosition;

    public StateRepos(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.grey;
    }
    
    public override void Enter(bool trigger = true) {
        //owner.sprRenderer.color = stateColor;
        //Send to the animator, the enum value converted to string of the current state as trigger event
        if (trigger) {
            owner.animator.SetTrigger(stateMachine.currentState.ToString().ToLower());
            owner.cursorAnimator.SetTrigger(stateMachine.currentState.ToString().ToLower());
        }
        if (actionInfos.samples != null) {
            if (actionInfos.samples.actionSounds.Length > 1) {
                actionInfos.samples.actionSounds[(int)actionInfos.direction - 1].Post(owner.gameObject);
            } else if (actionInfos.samples.actionSounds.Length > 0) {
                actionInfos.samples.actionSounds[0].Post(owner.gameObject);
            }
            actionInfos.Refresh(Time.time, true);
            owner.animator.SetInteger("direction", (int)actionInfos.direction);
            //Refresh animation clips speeds
            owner.animator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
            owner.cursorAnimator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
        }
        --owner.currentSpotIndex;
        startPosition = owner.transform.position;
    }

    public override void Update() {
        base.Update();
        owner.transform.position = Vector3.Lerp(startPosition, GameManager.instance.GetDashPos(owner.playerIndex), actionInfos.curve.Evaluate(actionInfos.GetPercentTime(Time.time)));
        //owner.animator.SetFloat("duration_repos", actionInfos.curve.Evaluate((Time.time - actionInfos.lastRefreshTime) / actionInfos.currentActionDuration));
    }

    public override void Exit(bool reset = false) {
        Debug.Log("Leaving repos");
        base.Exit(reset);
    }
}
