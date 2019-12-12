using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CLASSES
[System.Serializable]
public class SwordAction : Action {
    [HideInInspector] public eDIRECTION direction;
    public AnimationCurve curve;
    public AudioSampler leftSamples;
    public AudioSampler rightSamples;
    [HideInInspector] public AudioSampler currentSamples;

    public SwordAction(SwordAction copy) :
        base(copy)
    {
        direction = copy.direction;
        curve = copy.curve;
        leftSamples = copy.leftSamples;
        rightSamples = copy.rightSamples;
        currentSamples = copy.currentSamples;
    }

    public void SetSampleOwner(bool facingLeft) {
        currentSamples = (facingLeft) ? leftSamples : rightSamples;
    }

    public bool IsActionPerforming(float time, eDIRECTION tDirection) {
        return base.IsActionPerforming(time) && direction == tDirection;
    }
}

[System.Serializable]
public class Action : Cooldown {
    public float baseActionDuration;
    public float currentActionDuration;
    public float percentageWeightFury; //how much, in percentage, the Action is affected by the fury
    public int furyModificationOnSuccess; //how much the fury changed when the Action is perform successfully
    public float perfectPercentTiming;

    public Action (Action copy) :
        base(copy, copy.fury)
    {
        baseActionDuration = copy.baseActionDuration;
        currentActionDuration = copy.currentActionDuration;
        percentageWeightFury = copy.percentageWeightFury;
        furyModificationOnSuccess = copy.furyModificationOnSuccess;
        perfectPercentTiming = copy.perfectPercentTiming;
    }

    public void updateCurrentActionDuration() {
        currentActionDuration = fury.speedMultiplicator(percentageWeightFury) * baseActionDuration;
    }
    public override void Init(float time) {
        base.Init(time);
        currentActionDuration = baseActionDuration;
    }

    public bool IsActionPerforming(float time) {
        return (time <= lastRefreshTime + currentActionDuration);
    }

    //OTHER
    public float GetPercentTime(float time) {
        return ((time - lastRefreshTime) / currentActionDuration);
    }
}

//TODO : We may want to create a separate class file for this one as Cooldowns could be used by other classes.
[System.Serializable]
public class Cooldown {
    public float baseCooldownDuration;
    [HideInInspector] public Fury fury;
    public float currentCooldownDuration;
    public float percentageWeightFuryOnCD; //how much, in percentage, the Action cooldown is affected by the fury
    [HideInInspector] public float lastRefreshTime;

    public Cooldown(Cooldown copy, Fury copyFury) {
        baseCooldownDuration = copy.baseCooldownDuration;
        currentCooldownDuration = copy.currentCooldownDuration;
        percentageWeightFuryOnCD = copy.percentageWeightFuryOnCD;
        lastRefreshTime = copy.lastRefreshTime;

        fury = new Fury(copyFury);
    }

    //INIT
    public virtual void Init(float time) {
        currentCooldownDuration = baseCooldownDuration;
        BlankRefreshTime(time);
    }

    public void BlankRefreshTime(float time) {
        lastRefreshTime = time - currentCooldownDuration;
    }

    //TESTS
    public bool CanRefresh(float time) {
        return (time - lastRefreshTime > currentCooldownDuration || currentCooldownDuration == 0.0f);
    }

    //OPERATIONS
    public void Refresh(float time, bool force = false) {
        if (force || CanRefresh(time)) {
            lastRefreshTime = time;
        }
    }

    public void updateCurrentCooldownDuration(bool current = false) {
        currentCooldownDuration = fury.speedMultiplicator(percentageWeightFuryOnCD) * baseCooldownDuration;
        if (current) {
            currentCooldownDuration = fury.speedMultiplicator(percentageWeightFuryOnCD) * currentCooldownDuration;
        } else {
        }
    }

    public void setFury(Fury f) {
        fury = f;
    }
}

[System.Serializable]
public class Fury {
    public int lowestValueOfFury; //the lowest cap of fury 
    public int highestValueOfFury; //the higher cap of fury ; must be bigger (strictly) than lowestValueOfFury
    public int currentFury; //actual value of the player's Fury // should be [HideInInspector]
    public int startingValueOfFury; //the base value of the fury when a new match start, must be beetween lowestValueOfFury and HighestValueOfFury
    public int minimumMultiplicator; //indicate how many times faster an action is at 0% of fury
    public int maximumMultiplicator; //indicate how many times faster an action is at 100% of fury
    public float winnerFuryPercentageLeft; //wich percentage of his current fury the winner keep
    public bool isFeintAndChargeTheSame; //determine if theire's the need to separate Feinte and Charge
    public float percentagePerfectParyFury; //how much better, in percentage, is a perfect parry

    public Fury (Fury copy) {
        lowestValueOfFury = copy.lowestValueOfFury;
        highestValueOfFury = copy.highestValueOfFury;
        currentFury = copy.currentFury;
        startingValueOfFury = copy.startingValueOfFury;
        minimumMultiplicator = copy.minimumMultiplicator;
        maximumMultiplicator = copy.maximumMultiplicator;
        winnerFuryPercentageLeft = copy.winnerFuryPercentageLeft;
        isFeintAndChargeTheSame = copy.isFeintAndChargeTheSame;
        percentagePerfectParyFury = copy.percentagePerfectParyFury;
    }

    public float speedMultiplicator(float percentageWeight) {
        float proportionOfFury = ((float)currentFury / ((float)highestValueOfFury - (float)lowestValueOfFury)); // can't be <1
        float proportionOfMultiplicator = ((float)maximumMultiplicator - (float)minimumMultiplicator); // if we're max fury we need to return 1/maximumMultiplicator, if we're at 0 fury we need to return 1/minimumMultiplicator
        float weight = percentageWeight;
        float ret = ((float)minimumMultiplicator + (proportionOfFury * proportionOfMultiplicator) * weight);// [1/] because we need a multiplicator that reduce speed ; [minimumMultiplicator+] to be sure that at 0% fury we multipli by the right amount 

        if (ret != 0)
            return (float)1 / ret; //that formula is so much dependent on so much variable we need to be sure
        else
            return (float)1 / (float)10000; //because we don't really need to have that much of a specific case
    }

    public void resetFury() {
        currentFury = startingValueOfFury;
    }
    public void furyModification(float mod) {
        currentFury += (int)mod;
        if (currentFury > highestValueOfFury)
            currentFury = highestValueOfFury;
        else if (currentFury < lowestValueOfFury)
            currentFury = lowestValueOfFury;
    }

    public void furyMultiplication(float mult) {
        float f = currentFury * mult;
        currentFury = (int)f;
        if (currentFury > highestValueOfFury)
            currentFury = highestValueOfFury;
        else if (currentFury < lowestValueOfFury)
            currentFury = lowestValueOfFury;
    }

    public void furyModification(float mod, float perfectParry) {
        furyModification(mod * perfectParry);
    }

    public void Init() {
        currentFury = startingValueOfFury;
    }
}

[CreateAssetMenu(fileName = "New ActionInfos", menuName = "Scriptable/ActionInfos")]
public class ActionInfos : ScriptableObject
{
    public SwordAction parameters;
}
