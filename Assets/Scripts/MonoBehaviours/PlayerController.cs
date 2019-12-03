using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum eDIRECTION { NONE, UP, MID, DOWN };
public enum eCONTROLLER { KEYBOARD, GAMEPAD };

//CLASSES
[System.Serializable]
public class SwordAction : Action {
    [HideInInspector] public eDIRECTION direction;
    public AnimationCurve curve;

    public bool IsActionPerforming(float time, eDIRECTION tDirection) {
        return base.IsActionPerforming(time) && direction == tDirection;
    }
}
[System.Serializable]
public class Action : Cooldown {
    public float baseActionDuration;
    public float currentActionDuration;
    public int percentageWeightFury; //how much, in percentage, the Action is affected by the fury
    public int furyModificationOnSuccess; //how much the fury changed when the Action is perform successfully

    public void updateCurrentActionDuration()
    {
        currentActionDuration = fury.speedMultiplicator(percentageWeightFury) * baseActionDuration ;
    }
    public override void Init(float time) {
        base.Init(time);
        currentActionDuration = baseActionDuration;
    }

    public bool IsActionPerforming(float time) {
        return (time <= lastRefreshTime + currentActionDuration);
    }
}
//TODO : We may want to create a separate class file for this one as Cooldowns could be used by other classes.
[System.Serializable]
public class Cooldown {
    public float baseCooldownDuration;
    [HideInInspector] public Fury fury;
    public float currentCooldownDuration;
    public int percentageWeightFuryOnCD; //how much, in percentage, the Action cooldown is affected by the fury
    [HideInInspector] public float lastRefreshTime;

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
        return (time - lastRefreshTime > currentCooldownDuration);
    }

    //OPERATIONS
    public void Refresh(float time) {
        if (CanRefresh(time)) {
            lastRefreshTime = time;
        }
    }

    public void updateCurrentCooldownDuration ()
    {
        currentCooldownDuration = fury.speedMultiplicator(percentageWeightFuryOnCD) * baseCooldownDuration;
    }

    public void setFury(Fury f)
    {
        fury = f;
    }

}

[System.Serializable]
public class Fury
{
    public int lowestValueOfFury; //the lowest cap of fury 
    public int highestValueOfFury; //the higher cap of fury ; must be bigger (strictly) than lowestValueOfFury
    public int currentFury; //actual value of the player's Fury // should be [HideInInspector]
    public int startingValueOfFury; //the base value of the fury when a new match start, must be beetween lowestValueOfFury and HighestValueOfFury
    public int minimumMultiplicator; //indicate how many times faster an action is at 0% of fury
    public int maximumMultiplicator; //indicate how many times faster an action is at 100% of fury
    public int winnerFuryPercentageLeft; //wich percentage of his current fury the winner keep
    public bool isFeintAndChargeTheSame; //determine if theire's the need to separate Feinte and Charge
    public int percentagePerfectParyFury; //how much better, in percentage, is a perfect parry

    public float speedMultiplicator (int percentageWeight)
    {
        float proportionOfFury = ((float)currentFury / ((float)highestValueOfFury - (float)lowestValueOfFury)); // can't be <1
        float proportionOfMultiplicator = ((float)maximumMultiplicator - (float)minimumMultiplicator); // if we're max fury we need to return 1/maximumMultiplicator, if we're at 0 fury we need to return 1/minimumMultiplicator
        float weight = (float)percentageWeight / (float)100;
        float ret = (((float)minimumMultiplicator + (proportionOfFury * proportionOfMultiplicator)) * weight);// [1/] because we need a multiplicator that reduce speed ; [minimumMultiplicator+] to be sure that at 0% fury we multipli by the right amount 
        Debug.Log("pA " + percentageWeight + " | pF " + proportionOfFury + " | pM " + proportionOfMultiplicator + " | weight " + weight + " | ret " + ret);
        if (ret != 0) return (float)1 /ret; //that formula is so much dependent on so much variable we need to be sure
        else return (float)1 / (float)10000; //because we don't really need to have that much of a specific case
    }

    public void furyModification (int mod)
    {
        currentFury += mod;
        if (currentFury > highestValueOfFury) currentFury = highestValueOfFury;
        else if (currentFury < lowestValueOfFury) currentFury = lowestValueOfFury;
    }
    public void Init()
    {
        currentFury = startingValueOfFury;
    }


}
public class PlayerController : MonoBehaviour
{
    
    //REFERENCES
    private PlayerInput inputSystem;
    public Animator animator { get; private set; }
    public Animator cursorAnimator { get; private set; }
    public SpriteRenderer sprRenderer { get; private set; }

    private Rigidbody2D rb;
    [HideInInspector] public PlayerController opponent;
    [Header("REFERENCES")]
    [SerializeField] private Sword sword;
    //PARAMETERS
    [Header("PARAMETERS")]
    [SerializeField] public Fury fury;
    [SerializeField] public SwordAction strike;
    [SerializeField] public SwordAction charge;
    [SerializeField] public SwordAction parry;
    [SerializeField] public SwordAction dash;
    [SerializeField, Range(0, 180)] private float angleFullWindow;
    [SerializeField, Range(2, 10)] private int nbDirections;

    //OTHER
    public int playerIndex { get; private set; }
    [HideInInspector] public bool facingLeft;
    private eCONTROLLER controllerType;

    private FSM_Player machineState;
    private Vector2 look;
    private eDIRECTION currentDirection;
    private bool performingAction;
    private bool strokeOpponent;
    public int currentSpotIndex { get; set; }

    private void Awake() {

        //REFERENCES
        inputSystem = this.GetComponent<PlayerInput>();
        animator = this.GetComponent<Animator>();
        cursorAnimator = sword.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody2D>();
        sprRenderer = this.GetComponent<SpriteRenderer>();


        //VALUES
        strike.setFury(fury);
        charge.setFury(fury);
        parry.setFury(fury);
        dash.setFury(fury);

        machineState = new FSM_Player(this);
        angleFullWindow *= -1;
        currentSpotIndex = 0;
        performingAction = false;
        strokeOpponent = false;
        if (GetComponent<PlayerInput>().currentControlScheme.Contains("Keyboard")) {
            controllerType = eCONTROLLER.KEYBOARD;
        } else {
            controllerType = eCONTROLLER.GAMEPAD;
        }

        //LISTENERS
        inputSystem.currentActionMap["Look"].performed += context => OnLook(context);
        inputSystem.currentActionMap["Look"].canceled += context => OnLook(context);

        inputSystem.currentActionMap["Strike"].started += OnStrike;
        inputSystem.currentActionMap["Parry"].started += OnParry;
        inputSystem.currentActionMap["Dash"].started += OnDash;

        //Charge cooldown is the "attack" skill cooldown, so it needs to consider the strike duration as part of its cooldown

        fury.Init();
        charge.Init(Time.time);
        strike.Init(Time.time);
        parry.Init(Time.time);
        dash.Init(Time.time);
        sword.Initialize(this);
        // charge.currentCooldownDuration = strike.currentActionDuration + charge.currentCooldownDuration;
    }

    private void Start() {
        playerIndex = GameManager.instance.NewPlayer(this);
    }

    // Update is called once per frame
    void Update() {
        sprRenderer.flipX = facingLeft;
        machineState.Update();

        if (machineState.currentState == ePLAYER_STATE.NEUTRAL) {
            performingAction = false;
        } else {
            performingAction = true;
        }
    }

    private void InitController(InputAction.CallbackContext value) {
        controllerType = (value.control.ToString().Contains("Keyboard") ? eCONTROLLER.KEYBOARD : eCONTROLLER.GAMEPAD);
    }

    //INPUTS

    private void OnLook(InputAction.CallbackContext value) {
        if (!performingAction) {
            if(controllerType == eCONTROLLER.KEYBOARD) {
                look = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
            } else {
                look = value.ReadValue<Vector2>().normalized;
            }
            //print("Look :" + look);
            var angleRadians = Mathf.Atan2(look.y, Mathf.Abs(look.x));
            var deg = (angleRadians * Mathf.Rad2Deg) - 90f;//-90 is offset sprite


            if (deg % (angleFullWindow / nbDirections) != 0) {
                deg = (deg - (deg % (angleFullWindow / nbDirections)));
            }
            deg = Mathf.Clamp(deg, angleFullWindow, angleFullWindow / nbDirections);
            if (facingLeft) {
                //print(deg);
                deg *= -1f;
            }
            currentDirection = (eDIRECTION)(nbDirections/Mathf.Abs(angleFullWindow / deg));
            sword.transform.rotation = Quaternion.Euler(0, 0, deg);
        }
    }

    private void OnStrike(InputAction.CallbackContext value) {
        if (machineState.StateRequest(ePLAYER_STATE.CHARGE)) {
            charge.direction = currentDirection;
            strike.direction = currentDirection;
        }
    }

    private void OnParry(InputAction.CallbackContext value) {
        if (machineState.StateRequest(ePLAYER_STATE.PARRY)) {
            parry.direction = currentDirection;
        }
    }

    private void OnDash(InputAction.CallbackContext value) {
        if(currentSpotIndex < GameManager.instance.nbSteps-1) {
            if (machineState.StateRequest(ePLAYER_STATE.DASH)) {
                StartCoroutine(OpponentDashDelay());
            }
        }
    }
    //Coroutine used to trigger the opponent REPOS state because otherwise, it is done in the same frame and doesn't detect the player dash when striking
    private IEnumerator OpponentDashDelay() {
        yield return new WaitForSeconds(0.0001f);
        opponent.OnOpponentDash();
    }

    public void OnOpponentDash() {
        machineState.StateRequest(ePLAYER_STATE.REPOS);
    }


    public void furyChange(int mod)
    {
        fury.furyModification(mod);
        updateAllAction();
    }
    public void updateAllAction()
    {
        charge.updateCurrentActionDuration();
        charge.updateCurrentCooldownDuration();

        strike.updateCurrentActionDuration();
        strike.updateCurrentCooldownDuration();

        parry.updateCurrentActionDuration();
        parry.updateCurrentCooldownDuration();

        dash.updateCurrentActionDuration();
        dash.updateCurrentCooldownDuration();
    }
}
