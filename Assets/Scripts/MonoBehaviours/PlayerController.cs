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
    public AK.Wwise.Event[] actionSounds;
    public AK.Wwise.Event[] additionalSounds;

    public bool IsActionPerforming(float time, eDIRECTION tDirection) {
        return base.IsActionPerforming(time) && direction == tDirection;
    }
}
[System.Serializable]
public class Action : Cooldown {
    public float baseActionDuration;
    public float currentActionDuration;

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
    public float currentCooldownDuration;
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
        sprRenderer = this.transform.GetChild(1).GetComponent<SpriteRenderer>();

        //VALUES
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
        inputSystem.currentActionMap["Feint"].started += OnFeint;

        //Charge cooldown is the "attack" skill cooldown, so it needs to consider the strike duration as part of its cooldown

        charge.Init(Time.time);
        strike.Init(Time.time);
        parry.Init(Time.time);
        dash.Init(Time.time);
        sword.Initialize(this);
        //charge.currentCooldownDuration = strike.currentActionDuration + charge.currentCooldownDuration;
    }

    private void Start() {
        playerIndex = GameManager.instance.NewPlayer(this);
        if (facingLeft) {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
    }

    // Update is called once per frame
    void Update() {
        machineState.Update();

        if(machineState.currentState == ePLAYER_STATE.NEUTRAL) {
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
            machineState.ChangeState(ePLAYER_STATE.CHARGE);
        }
    }

    private void OnParry(InputAction.CallbackContext value) {
        if (machineState.StateRequest(ePLAYER_STATE.PARRY)) {
            parry.direction = currentDirection;
            machineState.ChangeState(ePLAYER_STATE.PARRY);
        }
    }

    private void OnDash(InputAction.CallbackContext value) {
        if(currentSpotIndex < GameManager.instance.nbSteps-1) {
            if (machineState.StateRequest(ePLAYER_STATE.DASH)) {
                StartCoroutine(OpponentDashDelay());
                machineState.ChangeState(ePLAYER_STATE.DASH);
            }
        }
    }

    private void OnFeint(InputAction.CallbackContext value) {
        if (machineState.StateRequest(ePLAYER_STATE.FEINT)) {
            print("feint");
            charge.direction = currentDirection;
            machineState.ChangeState(ePLAYER_STATE.FEINT);
        } else {
            print("Cannto feint");
        }
    }

    //Coroutine used to trigger the opponent REPOS state because otherwise, it is done in the same frame and doesn't detect the player dash when striking
    private IEnumerator OpponentDashDelay() {
        yield return new WaitForSeconds(0.0001f);
        if(opponent != null) {
            opponent.OnOpponentDash();
        }
    }

    public void OnOpponentDash() {
        if (machineState.StateRequest(ePLAYER_STATE.REPOS)) {
            machineState.ChangeState(ePLAYER_STATE.REPOS);
        }
    }
}
