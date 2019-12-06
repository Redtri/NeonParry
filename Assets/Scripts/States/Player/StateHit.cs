using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateHit : PlayerState
{

    private bool isHit;

    public StateHit(PlayerController player, FSM_Player machine, SwordAction infos, ePLAYER_STATE next, eSTATE_PRIORITY statePriority) :
    base(player, machine, infos, next, statePriority)
    {

    }

    public override void Enter(bool trigger = true)
    {
        Debug.Log("entering StateHit ");
        base.Enter();
        isHit = owner.isHit;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit(bool reset = false)
    {
        base.Exit(reset);
    }

}
