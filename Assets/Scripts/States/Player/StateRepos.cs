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
    
    public override void Enter(bool trigger = true)
    {
        base.Enter();
        --owner.currentSpotIndex;
        startPosition = owner.transform.position;
    }

    public override void Update() {
        base.Update();
        owner.animator.SetFloat("duration_repos", actionInfos.curve.Evaluate((Time.time - actionInfos.lastRefreshTime) / actionInfos.currentActionDuration));
        owner.transform.position = Vector3.Lerp(startPosition, GameManager.instance.GetDashPos(owner.playerIndex), actionInfos.curve.Evaluate((Time.time - actionInfos.lastRefreshTime) / actionInfos.currentActionDuration));
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
    }
}
