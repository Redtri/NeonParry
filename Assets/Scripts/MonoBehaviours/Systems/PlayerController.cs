using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum eDIRECTION { NONE, UP, MID, DOWN };
public enum eCONTROLLER { KEYBOARD, GAMEPAD };

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
    [SerializeField] public ActionInfos strikeInfos;
    [SerializeField] public ActionInfos chargeInfos;
    [SerializeField] public ActionInfos parryInfos;
    [SerializeField] public ActionInfos dashInfos;
    [SerializeField] public ActionInfos reposInfos;
    [HideInInspector] public SwordAction strikeAction;
    [HideInInspector] public SwordAction chargeAction;
    [HideInInspector] public SwordAction parryAction;
    [HideInInspector] public SwordAction dashAction;
    [HideInInspector] public SwordAction reposAction;
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
        strikeAction = new SwordAction(strikeInfos.parameters);
        chargeAction = new SwordAction(chargeInfos.parameters);
        parryAction = new SwordAction(parryInfos.parameters);
        dashAction = new SwordAction(dashInfos.parameters);
        reposAction = new SwordAction(reposInfos.parameters);

        currentDirection = eDIRECTION.MID;
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
        strikeAction.setFury(fury);
        chargeAction.setFury(fury);
        parryAction.setFury(fury);
        dashAction.setFury(fury);

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
        chargeAction.Init(Time.time);
        strikeAction.Init(Time.time);
        parryAction.Init(Time.time);
        dashAction.Init(Time.time);
        reposAction.Init(Time.time);
        sword.Initialize(this);

        machineState = new FSM_Player(this);
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

        chargeAction.SetSampleOwner(facingLeft);
        strikeAction.SetSampleOwner(facingLeft);
        parryAction.SetSampleOwner(facingLeft);
        dashAction.SetSampleOwner(facingLeft);
        reposAction.SetSampleOwner(facingLeft);
        Debug.Log("Player facing " + ((facingLeft) ? "left" : "right"));
    }

    private void OnEnable() {
        if (machineState != null) {
            machineState.onStateChanged += UpdateLook;
        }
    }

    public void Unsubscribe() {
        machineState.onStateChanged -= UpdateLook;
        inputSystem.currentActionMap["Look"].performed -= OnLook;
        inputSystem.currentActionMap["Look"].canceled -=  OnLook;

        inputSystem.currentActionMap["Strike"].started -= OnStrike;
        inputSystem.currentActionMap["Parry"].started -= OnParry;
        inputSystem.currentActionMap["Dash"].started -= OnDash;
        inputSystem.currentActionMap["Feint"].started -= OnFeint;
    }
    

    // Update is called once per frame
    void Update() {
        machineState.Update();
        Debug.Log(gameObject.name + " " + strikeAction.lastRefreshTime + " " + strikeAction.IsActionPerforming(Time.time));

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
            sword.transform.rotation = Quaternion.Euler(0, 0, deg);
        }
    }

    private void OnStrike(InputAction.CallbackContext value) {
        if (!isStop)
        {
            if (machineState.StateRequest(ePLAYER_STATE.CHARGE))
            {
                onStrike?.Invoke(currentDirection, chargeAction.currentActionDuration + strikeAction.currentActionDuration);
                chargeAction.direction = currentDirection;
                strikeAction.direction = currentDirection;
                machineState.ChangeState(ePLAYER_STATE.CHARGE);
            }
        }
    }

    private void OnParry(InputAction.CallbackContext value) {
        if (!isStop)
        {
            if (machineState.StateRequest(ePLAYER_STATE.PARRY))
            {
                parryAction.direction = currentDirection;
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
                chargeAction.direction = currentDirection;
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
        reposAction.currentSamples.additionalSounds[index].Post(gameObject);
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
        chargeAction.currentCooldownDuration = t;
        chargeAction.Refresh(Time.time, true);
        strikeAction.currentCooldownDuration = t;
        strikeAction.Refresh(Time.time, true);
        parryAction.currentCooldownDuration = t;
        parryAction.Refresh(Time.time, true);
        dashAction.currentCooldownDuration = t;
        dashAction.Refresh(Time.time, true);
    }

    public void updateAllAction()
    {
        chargeAction.updateCurrentActionDuration();
        chargeAction.updateCurrentCooldownDuration();

        strikeAction.updateCurrentActionDuration();
        strikeAction.updateCurrentCooldownDuration();

        parryAction.updateCurrentActionDuration();
        parryAction.updateCurrentCooldownDuration();

        dashAction.updateCurrentActionDuration();
        dashAction.updateCurrentCooldownDuration();
    }
}
