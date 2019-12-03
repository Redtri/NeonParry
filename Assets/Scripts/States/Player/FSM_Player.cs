using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ePLAYER_STATE { NEUTRAL, CHARGE, FEINT, STRIKE, PARRY, DASH, REPOS}

public class FSM_Player
{
    public ePLAYER_STATE previousState { get; private set; }
    public ePLAYER_STATE currentState { get; private set; }
    public Dictionary<ePLAYER_STATE, PlayerState> states;

    public FSM_Player(PlayerController player) {
        states = new Dictionary<ePLAYER_STATE, PlayerState> { { ePLAYER_STATE.NEUTRAL, new StateNeutral(player, this, null, ePLAYER_STATE.NEUTRAL, eSTATE_PRIORITY.DEFAULT) },
                                                              { ePLAYER_STATE.CHARGE, new StateCharge(player, this, player.charge, ePLAYER_STATE.STRIKE, eSTATE_PRIORITY.DEFAULT) },
                                                              { ePLAYER_STATE.STRIKE, new StateStrike(player, this, player.strike, ePLAYER_STATE.NEUTRAL, eSTATE_PRIORITY.DEFAULT) },
                                                              { ePLAYER_STATE.PARRY, new StateParry(player, this, player.parry, ePLAYER_STATE.NEUTRAL, eSTATE_PRIORITY.DEFAULT) },
                                                              { ePLAYER_STATE.DASH, new StateDash(player, this, player.dash, ePLAYER_STATE.NEUTRAL, eSTATE_PRIORITY.OVERRIDE) },
                                                              { ePLAYER_STATE.REPOS, new StateRepos(player, this, player.dash, ePLAYER_STATE.NEUTRAL, eSTATE_PRIORITY.OVERRIDE) },
                                                              //TODO : Watch out, we may want to create a new feint swordaction for this
                                                              { ePLAYER_STATE.FEINT, new StateFeint(player, this, player.charge, ePLAYER_STATE.NEUTRAL, eSTATE_PRIORITY.DEFAULT) }
        };
        currentState = ePLAYER_STATE.NEUTRAL;
        previousState = currentState;
    }

    public void Start()
    {
        states[currentState].Enter();
    }
    
    public void Update()
    {
        states[currentState].Update();
    }

    //Should be called by any state that can ensure the state transition
    public void ChangeState(ePLAYER_STATE newState) {
        previousState = currentState;
        states[currentState].Exit();
        currentState = newState;
        states[currentState].Enter();
    }

    //Should be called by the PlayerController when receiving input event
    public bool StateRequest(ePLAYER_STATE newState, eDIRECTION direction = eDIRECTION.NONE) {
        if (states[newState].actionInfos.CanRefresh(Time.time)) {
            if (states[newState].priority == eSTATE_PRIORITY.OVERRIDE && newState != currentState) {

                return true;
            } else if (currentState == ePLAYER_STATE.NEUTRAL) {

                return true;
            }
        }
        return false;
    }
}
