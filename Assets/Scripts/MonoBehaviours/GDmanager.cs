using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GDmanager : MonoBehaviour
{
    public int lowestValueOfFury; //the lowest cap of fury
    public int highestValueOfFury; //the higher cap of fury
    public int startingValueOfFury; //the base value of the fury when a new match start, must be beetween lowestValueOfFury and HighestValueOfFury
    public int maximumMultiplicator; //indicate how many times faster an action is at 100% of fury
    public int succesfullParyFuryModification; //can be either + or -, is applied when a player successfully parry must be beetween lowestValueOfFury and HighestValueOfFury
    public int successfullDashFuryModification; //can be either + or -, is applied when a player Dash must be beetween lowestValueOfFury and HighestValueOfFury
    public int winnerFuryPercentageLeft; //wich percentage of his current fury the winner keep
    public bool isFeintAndChargeTheSame; //determine if theire's the need to separate Feinte and Charge
    public int percentagePerfectParyFury; //how much better, in percentage, is a perfect parry

}
