using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateFeint : PlayerState
{

    public StateFeint(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority)
    {
        stateColor = Color.yellow;
    }
    
    public override void Enter(bool trigger = true) {
        owner.sprRenderer.color = stateColor;
        //Send to the animator, the enum value converted to string of the current state as trigger event
        owner.animator.SetTrigger("feint");
        owner.cursorAnimator.SetTrigger("feint");
        if (actionInfos != null) {
            actionInfos.Refresh(Time.time);
            owner.animator.SetInteger("direction", (int)actionInfos.direction);
            //Refresh animation clips speeds
            owner.animator.SetFloat("duration_" + "charge", 1 / actionInfos.currentActionDuration);
            owner.cursorAnimator.SetFloat("duration_" + "charge", 1 / actionInfos.currentActionDuration);
        }
        owner.animator.SetBool("feinting", true);
        //actionInfos.currentCooldownDuration = owner.strike.currentActionDuration + actionInfos.baseCooldownDuration;
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void Exit(bool reset = false)
    {
        actionInfos.currentCooldownDuration = 0f;
        //owner.animator.SetBool("feinting", false);
        base.Exit(reset);
    }
}
