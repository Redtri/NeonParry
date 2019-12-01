using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDash : PlayerState
{

    public StateDash(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
        base(player, machine, infos, next, statePriority) {
        stateColor = Color.green;
    }
    
    public override void Enter()
    {
        owner.currentSpotIndex++;
        base.Enter();
    }

    public override void Update() {
        base.Update();
        float mult = (owner.facingLeft) ? 1f : -1f;
        owner.transform.Translate(new Vector3(mult * (GameManager.instance.stepValue / actionInfos.currentActionDuration) * Time.deltaTime, 0, 0));
    }

    public override void Exit(bool reset = false) {
        base.Exit(reset);
    }
}
