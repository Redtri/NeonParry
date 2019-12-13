using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StateDash : PlayerState
{
    private Vector3 startPosition;
    private Vector3 destPos;

    public StateDash(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.green;
    }
    
    public override void Enter(bool trigger = true)
    {
        if (trigger) {
            owner.animator.SetTrigger(stateMachine.currentState.ToString().ToLower());
            owner.cursorAnimator.SetTrigger(stateMachine.currentState.ToString().ToLower());
        }
        if (actionInfos != null) {
            uint state;
            AkSoundEngine.GetState("inGame", out state);

            Debug.Log("InGame Wwise state : " + state);
            if(state == 2) {
                actionInfos.currentSamples.actionSounds[0].Post(owner.gameObject);
            } else {
                actionInfos.currentSamples.actionSounds[0].Post(owner.gameObject);
                actionInfos.currentSamples.additionalSounds[0].Post(owner.gameObject);
            }
            actionInfos.Refresh(Time.time);
            owner.animator.SetInteger("direction", (int)actionInfos.direction);
            //Refresh animation clips speeds
            owner.animator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
            owner.cursorAnimator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
        }
        //owner.allActionsOnCd(actionInfos.currentActionDuration);
        ++owner.currentSpotIndex;
        //AudioManager.instance.UpdateMusic(2);
        startPosition = owner.transform.position;
        destPos = (SceneManager.GetActiveScene().name != "IntroScene") ? GameManager.instance.GetDashPos(owner.playerIndex) : MenuManager.instance.GetDashPos(owner.playerIndex);
    }

    public override void Update() {
        base.Update();

        owner.transform.position = Vector3.Lerp(startPosition, destPos, (Time.time- actionInfos.lastRefreshTime)/actionInfos.currentActionDuration);
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
        owner.furyChange(actionInfos.furyModificationOnSuccess); //change the fury of a fixed amount
    }
}
