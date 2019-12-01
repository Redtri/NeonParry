using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDash : PlayerState
{
    private Vector3 startPosition;

    public StateDash(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.green;
    }
    
    public override void Enter()
    {
        base.Enter();
        ++owner.currentSpotIndex;
        startPosition = owner.transform.position;
    }

    public override void Update() {
        base.Update();
        owner.transform.position = Vector3.Lerp(startPosition, GameManager.instance.GetDashPos(owner.playerIndex), (Time.time- actionInfos.lastRefreshTime)/actionInfos.currentActionDuration);
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
    }
}
