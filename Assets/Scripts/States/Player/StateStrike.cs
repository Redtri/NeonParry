using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateStrike : PlayerState
{
    private bool opponentParried;
    private bool opponentDashed;
    private bool clash;

    public StateStrike(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.magenta;
    }
    
    public override void Enter(bool trigger = true)
    {
        base.Enter();
        opponentParried = false;
        opponentDashed = false;
        clash = false;
        nextState = ePLAYER_STATE.NEUTRAL;
    }

    public override void Update() {
        base.Update();
        if (owner.opponent != null) {
            if (owner.opponent.parry.IsActionPerforming(Time.time, actionInfos.direction)) {
                Debug.Log("Opponent parried successfully");
                opponentParried = true;
                owner.animator.SetTrigger("knockback");
                //TODO : FIXME
                owner.opponent.parry.BlankRefreshTime(Time.time);
                stateMachine.ChangeState(nextState, false);
            } else if (owner.opponent.dash.IsActionPerforming(Time.time)) {
                Debug.Log("Opponent dodged, coward !");
                opponentDashed = true;
                nextState = ePLAYER_STATE.REPOS;
            }else if(owner.opponent.strike.IsActionPerforming(Time.time, actionInfos.direction)) {
                clash = true;
                owner.animator.SetTrigger("knockback");
                stateMachine.ChangeState(nextState, false);

            }
        }
    }

    public override void Exit(bool reset = false)
    {
        base.Exit(reset);
        owner.charge.Refresh(Time.time, true);
        if (owner.opponent != null) {
            if (!opponentParried) {
                if (!opponentDashed) {
                    if (!clash) {
                        if (!owner.isStop)
                        {
                            owner.opponent.isStop = true;
                            owner.furyChange(actionInfos.furyModificationOnSuccess); //change the fury of a fixed amount
                            owner.opponent.fxHandler.SpawnFX(ePLAYER_STATE.STRIKE, actionInfos.direction);
                            actionInfos.samples.additionalSounds[0].Post(owner.gameObject);
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
                                    break;
                            };
                            owner.fury.furyMultiplication(owner.fury.winnerFuryPercentageLeft);
                            actionInfos.samples.additionalSounds[0].Post(owner.gameObject);
                            //TODO it's really not clean but at least player can't strike after being hit.
                            //owner.onStop();
                            owner.isStop = true;
                            //nextState = ePLAYER_STATE.STOP;
                            owner.opponent.onStop();
                        }
                    } else {
                        actionInfos.samples.additionalSounds[1].Post(owner.gameObject);
                    }
                }
            }
            base.Exit(reset);

        }
    }
}
