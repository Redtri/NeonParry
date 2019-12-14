using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateStrike : PlayerState
{
    private bool opponentParried;
    private bool opponentDashed;
    private bool clash;
    public bool dashed;

    public StateStrike(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.magenta;
    }
    
    public override void Enter(bool trigger = true)
    {
        if (trigger)
        {
            owner.animator.SetTrigger(stateMachine.currentState.ToString().ToLower());
            owner.cursorAnimator.SetTrigger(stateMachine.currentState.ToString().ToLower());
        }
        if (actionInfos != null)
        {
            if (actionInfos.currentSamples != null)
            {
                actionInfos.Refresh(Time.time);
                owner.animator.SetInteger("direction", (int)actionInfos.direction);
                //Refresh animation clips speeds
                owner.animator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
                owner.cursorAnimator.SetFloat("duration_" + stateMachine.currentState.ToString().ToLower(), 1 / actionInfos.currentActionDuration);
            }
        }
        opponentParried = false;
        opponentDashed = false;
        dashed = false;
        clash = false;
        owner.onDash += DashEvent;
        nextState = ePLAYER_STATE.NEUTRAL;
    }

    private void DashEvent()
    {
        dashed = true;
    }

    public override void Update() {
        base.Update();
        if (owner.opponent != null) {
            if (owner.opponent.parryAction.IsActionPerforming(Time.time, actionInfos.direction)) {
                Debug.Log("Opponent parried successfully");
                opponentParried = true;
                owner.animator.SetTrigger("knockback");
                //TODO : FIXME
                owner.opponent.parryAction.BlankRefreshTime(Time.time);
                stateMachine.ChangeState(nextState, false);
            } else if (owner.opponent.dashAction.IsActionPerforming(Time.time)) {
                Debug.Log("Opponent dodged, coward !");
                opponentDashed = true;
                nextState = ePLAYER_STATE.REPOS;
            }else if(owner.opponent.strikeAction.IsActionPerforming(Time.time, actionInfos.direction)) {
                Debug.Log("Clash");
                clash = true;
                FX_Manager.instance.ClashFX();
                owner.animator.SetTrigger("knockback");
                actionInfos.currentSamples.additionalSounds[1].Post(owner.gameObject);
                stateMachine.ChangeState(nextState, false);
            }
        }
    }

    public override void Exit(bool reset = false)
    {
        base.Exit(reset);
        owner.chargeAction.Refresh(Time.time, true);
        if (owner.opponent != null)
        {
            if (!opponentParried)
            {
                if (!opponentDashed)
                {
                    if (!owner.isStop)
                    {
                        if (!clash) {
                            if (!dashed)
                            {
                                GameManager.instance.StopPlayers(2f);
                                owner.furyChange(actionInfos.furyModificationOnSuccess); //change the fury of a fixed amount
                                owner.opponent.fxHandler.SpawnFX(ePLAYER_STATE.STRIKE, actionInfos.direction, owner.facingLeft);
                                switch (GameManager.instance.StrikeSuccessful(owner.playerIndex))
                                {
                                    case 0:
                                        owner.opponent.animator.SetTrigger("hit");
                                        break;
                                    case 1:
                                        owner.opponent.animator.SetTrigger("down");
                                        break;
                                    case 2:
                                        owner.animator.SetTrigger("victory");
                                        owner.opponent.animator.SetTrigger("death");
                                        AudioManager.instance.Death();
                                        break;
                                };
                                owner.fury.furyMultiplication(owner.fury.winnerFuryPercentageLeft);
                                actionInfos.currentSamples.additionalSounds[0].Post(owner.gameObject);
                            }
                        }
                    }
                }
            }
        }
        owner.onDash -= DashEvent;
    }
}
