using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum eDIRECTION { NONE, UP, MID, DOWN };
public enum eCONTROLLER { KEYBOARD, GAMEPAD };

//CLASSES
[System.Serializable]
public class SwordAction : Action {
    [HideInInspector] public eDIRECTION direction;
    public AnimationCurve curve;
    public AudioSampler samples;

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

    public void updateCurrentActionDuration()
    {
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

    public void updateCurrentCooldownDuration (bool current = false) {
        currentCooldownDuration = fury.speedMultiplicator(percentageWeightFuryOnCD) * baseCooldownDuration;
        if (current) {
            currentCooldownDuration = fury.speedMultiplicator(percentageWeightFuryOnCD) * currentCooldownDuration;
        } else {
        }
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
    public float winnerFuryPercentageLeft; //wich percentage of his current fury the winner keep
    public bool isFeintAndChargeTheSame; //determine if theire's the need to separate Feinte and Charge
    public float percentagePerfectParyFury; //how much better, in percentage, is a perfect parry

    public float speedMultiplicator (float percentageWeight)
    {
        float proportionOfFury = ((float)currentFury / ((float)highestValueOfFury - (float)lowestValueOfFury)); // can't be <1
        float proportionOfMultiplicator = ((float)maximumMultiplicator - (float)minimumMultiplicator); // if we're max fury we need to return 1/maximumMultiplicator, if we're at 0 fury we need to return 1/minimumMultiplicator
        float weight = percentageWeight;
        float ret = ((float)minimumMultiplicator + (proportionOfFury * proportionOfMultiplicator) * weight);// [1/] because we need a multiplicator that reduce speed ; [minimumMultiplicator+] to be sure that at 0% fury we multipli by the right amount 

        if (ret != 0) return (float)1 /ret; //that formula is so much dependent on so much variable we need to be sure
        else return (float)1 / (float)10000; //because we don't really need to have that much of a specific case
    }

    public void resetFury()
    {
        currentFury = startingValueOfFury;
    }
    public void furyModification (float mod)
    {
        currentFury += (int)mod;
        if (currentFury > highestValueOfFury) currentFury = highestValueOfFury;
        else if (currentFury < lowestValueOfFury) currentFury = lowestValueOfFury;
    }

    public void furyMultiplication(float mult) {
        float f = currentFury * mult;
        currentFury = (int)f;
        if (currentFury > highestValueOfFury)
            currentFury = highestValueOfFury;
        else if (currentFury < lowestValueOfFury)
            currentFury = lowestValueOfFury;
    }

    public void furyModification (float mod, float perfectParry)
    {
        furyModification(mod*perfectParry);
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
    [HideInInspector] public FX_Handler fxHandler;

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
    [SerializeField] public SwordAction stop;
    [SerializeField] public SwordAction repos;
    [SerializeField, Range(0, 180)] private float angleFullWindow;
    [SerializeField, Range(2, 10)] private int nbDirections;

    //OTHER
    public int playerIndex { get; private set; }
    [HideInInspector] public bool facingLeft;
    private eCONTROLLER controllerType;

    private FSM_Player machineState;
    private Vector2 look;
    private Vector2 temporaryLastLook;
    private eDIRECTION currentDirection;
    private bool performingAction;
    private bool strokeOpponent;
    [HideInInspector] public bool isStop;
    public int currentSpotIndex { get; set; }
    [HideInInspector] public int skin;

    public delegate void StrikeEvent(eDIRECTION direction, float delay);
    public StrikeEvent onStrike;

    private void Awake() {
        

        if (SceneManager.GetActiveScene().name == "IntroScene") {
            gameObject.AddComponent<SpriteController>();
        }

        //REFERENCES
        inputSystem = this.GetComponent<PlayerInput>();
        animator = this.GetComponent<Animator>();
        cursorAnimator = sword.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody2D>();
        fxHandler = this.GetComponent<FX_Handler>();


        //VALUES
        strike.setFury(fury);
        charge.setFury(fury);
        parry.setFury(fury);
        dash.setFury(fury);
        stop.setFury(fury);

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

        //Charge cooldown is the "attack" skill cooldown, so it needs to consider the strike duration as part of its cooldown

        fury.Init();
        charge.Init(Time.time);
        strike.Init(Time.time);
        parry.Init(Time.time);
        dash.Init(Time.time);
        repos.Init(Time.time);
        stop.Init(Time.time);
        sword.Initialize(this);
        // charge.currentCooldownDuration = strike.currentActionDuration + charge.currentCooldownDuration;
    }


    private void Start() {
        //TODO Mettre condition scene
        if (SceneManager.GetActiveScene().name != "IntroScene") {
            playerIndex = GameManager.instance.NewPlayer(this);
        } else {
            playerIndex = MenuManager.instance.NewPlayer(this);
        }

        if (facingLeft) {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
        inputSystem.currentActionMap["Look"].performed += context => OnLook(context);
        inputSystem.currentActionMap["Look"].canceled += context => OnLook(context);

        inputSystem.currentActionMap["Strike"].started += OnStrike;
        inputSystem.currentActionMap["Parry"].started += OnParry;
        inputSystem.currentActionMap["Dash"].started += OnDash;
        inputSystem.currentActionMap["Feint"].started += OnFeint;
        machineState.onStateChanged += UpdateLook;
    }

    private void OnEnable() {
        if (machineState != null) {
            machineState.onStateChanged += UpdateLook;
        }
    }

    public void Unsubscribe() {
        inputSystem.currentActionMap["Look"].performed -= context => OnLook(context);
        inputSystem.currentActionMap["Look"].canceled -= context => OnLook(context);

        inputSystem.currentActionMap["Strike"].started -= OnStrike;
        inputSystem.currentActionMap["Parry"].started -= OnParry;
        inputSystem.currentActionMap["Dash"].started -= OnDash;
        inputSystem.currentActionMap["Feint"].started -= OnFeint;
    }
    

    // Update is called once per frame
    void Update() {
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

        if (controllerType == eCONTROLLER.KEYBOARD) {
            temporaryLastLook = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position - new Vector3(0f, 9f, 0f)).normalized;
        } else {
            temporaryLastLook = value.ReadValue<Vector2>().normalized;
        }
        if (!performingAction) {
            if(controllerType == eCONTROLLER.KEYBOARD) {
                look = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position - new Vector3(0f, 9f, 0f)).normalized;
            } else {
                look = value.ReadValue<Vector2>().normalized;
            }
            var angleRadians = Mathf.Atan2(look.y, Mathf.Abs(look.x));
            var deg = (angleRadians * Mathf.Rad2Deg) - 90f;//-90 is offset sprite

            if (deg % (angleFullWindow / nbDirections) != 0) {
                deg = (deg - (deg % (angleFullWindow / nbDirections)));
            }
            deg = Mathf.Clamp(deg, angleFullWindow, angleFullWindow / nbDirections);
            if (facingLeft) {
                deg *= -1f;
            }
            currentDirection = (eDIRECTION)(nbDirections / Mathf.Abs(angleFullWindow / deg));
        }
    }

    private void OnStrike(InputAction.CallbackContext value) {
        if (!isStop)
        {
            if (machineState.StateRequest(ePLAYER_STATE.CHARGE))
            {
            onStrike?.Invoke(currentDirection, charge.currentActionDuration + strike.currentActionDuration);
            charge.direction = currentDirection;
            strike.direction = currentDirection;
            machineState.ChangeState(ePLAYER_STATE.CHARGE);
            }
        }
    }

    private void OnParry(InputAction.CallbackContext value) {
        if (!isStop)
        {
            if (machineState.StateRequest(ePLAYER_STATE.PARRY))
            {
                parry.direction = currentDirection;
                machineState.ChangeState(ePLAYER_STATE.PARRY);
            }
        }
    }

    private void OnDash(InputAction.CallbackContext value) {
        if (!isStop) {
            if (SceneManager.GetActiveScene().name != "IntroScene") {
                if (currentSpotIndex < GameManager.instance.nbSteps - 1) {
                    if (machineState.currentState != ePLAYER_STATE.REPOS && machineState.StateRequest(ePLAYER_STATE.DASH)) {
                        machineState.ChangeState(ePLAYER_STATE.DASH);
                        StartCoroutine(OpponentReposDelay());
                    }
                }
            } else {

                if (currentSpotIndex < MenuManager.instance.nbSteps - 1) {
                    if (machineState.StateRequest(ePLAYER_STATE.DASH)) {
                        StartCoroutine(OpponentReposDelay());
                        machineState.ChangeState(ePLAYER_STATE.DASH);
                        MenuManager.instance.PlayerReady();
                    }
                }
            }
        }
    }

    private void OnFeint(InputAction.CallbackContext value) {
        if (!isStop)
        {
            if (machineState.StateRequest(ePLAYER_STATE.FEINT))
            {
                charge.direction = currentDirection;
                machineState.ChangeState(ePLAYER_STATE.FEINT);
            }
        }
    }

    //Coroutine used to trigger the opponent REPOS state because otherwise, it is done in the same frame and doesn't detect the player dash when striking
    private IEnumerator OpponentReposDelay() {
        yield return new WaitForSeconds(0.0001f);
        if(opponent != null) {
            opponent.OnOpponentDash();
        }
    }

    public void OnOpponentDash() {
            machineState.ChangeState(ePLAYER_STATE.REPOS);
    }

    public void EventPlay(int index) {
        repos.samples.additionalSounds[index].Post(gameObject);
    }

    private void UpdateLook() {
        if (look != temporaryLastLook && machineState.currentState != ePLAYER_STATE.STRIKE) {
            look = temporaryLastLook;

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
            currentDirection = (eDIRECTION)(nbDirections / Mathf.Abs(angleFullWindow / deg));
            sword.transform.rotation = Quaternion.Euler(0, 0, deg);
        }
    }

    public void onNeutral()
    {
        isStop = true;
        machineState.ChangeState(ePLAYER_STATE.NEUTRAL);
    }

    public void onStop()
    {
        isStop = true;
        machineState.ChangeState(ePLAYER_STATE.STOP);
    }

    public void furyChange(float mod)
    {
        fury.furyModification(mod);
        fxHandler.UpdateFuryFX(((float)fury.currentFury)/100);
        if(machineState.currentState != ePLAYER_STATE.DASH) {
            updateAllAction();
        }
        if (facingLeft) {
            AkSoundEngine.SetRTPCValue("FuryLeft_RTPC", (float)fury.currentFury / (float)fury.highestValueOfFury);
        } else {
            AkSoundEngine.SetRTPCValue("FuryRight_RTPC", (float)fury.currentFury/(float)fury.highestValueOfFury);
        }
    }

    public void allActionsOnCd(float t) {
        charge.currentCooldownDuration = t;
        charge.Refresh(Time.time, true);
        strike.currentCooldownDuration = t;
        strike.Refresh(Time.time, true);
        parry.currentCooldownDuration = t;
        parry.Refresh(Time.time, true);
        dash.currentCooldownDuration = t;
        dash.Refresh(Time.time, true);
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
