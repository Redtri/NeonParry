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
        base.Enter();
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
