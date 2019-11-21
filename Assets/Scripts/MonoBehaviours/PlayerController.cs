using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum eDIRECTION { UP, MID, DOWN };

public class PlayerController : MonoBehaviour
{
    //CLASSES
    [System.Serializable]
    public class SwordAction : Action {
        [HideInInspector] public eDIRECTION direction;

        public SwordAction(float tDuration, float time) :
            base(tDuration, time)
        {
            actionDuration = tDuration;
            lastRefreshTime = time;
        }

        public bool IsActionPerforming(float time, eDIRECTION tDirection) {
            return base.IsActionPerforming(time) && direction == tDirection;
        }
    }
    [System.Serializable]
    public class Action : Cooldown {
        public float actionDuration;

        public Action(float tDuration, float time) :
            base(tDuration, time)
        {
            cooldownDuration = tDuration;
            lastRefreshTime = time;
        }

        public bool IsActionPerforming(float time) {
            return (time <= lastRefreshTime + actionDuration);
        }
    }
    //TODO : We may want to create a separate class file for this one as Cooldowns could be used by other classes.
    [System.Serializable]
    public class Cooldown {
        public float cooldownDuration;
        [HideInInspector] public float lastRefreshTime;

        //INIT
        public Cooldown(float tDuration, float time) {
            cooldownDuration = tDuration;
            lastRefreshTime = time - cooldownDuration;
        }

        public void BlankRefreshTime(float time) {
            lastRefreshTime = time - cooldownDuration;
        }

        //TESTS
        public bool CanRefresh(float time) {
            return (time - lastRefreshTime > cooldownDuration);
        }

        //OPERATIONS
        public void Refresh(float time) {
            if (CanRefresh(time)) {
                lastRefreshTime = time;
            }
        }
    }
    
    //REFERENCES
    private PlayerInput inputSystem;
    private Animator animator;
    private Animator cursorAnimator;
    private SpriteRenderer sprRenderer;
    private Rigidbody2D rb;
    [HideInInspector] public PlayerController opponent;
    [Header("REFERENCES")]
    [SerializeField] private Sword sword;
    //PARAMETERS
    [Header("PARAMETERS")]
    [SerializeField] private SwordAction strike;
    [SerializeField] private SwordAction charge;
    [SerializeField] private SwordAction parry;
    [SerializeField, Range(0, 180)] private float angleFullWindow;
    [SerializeField, Range(2, 10)] private int nbDirections;

    //OTHER
    private int playerIndex;
    public bool facingLeft;
    private Vector2 look;
    private eDIRECTION currentDirection;
    private bool performingAction;
    private bool strokeOpponent;

    private void Awake() {
        //REFERENCES
        inputSystem = this.GetComponent<PlayerInput>();
        animator = this.GetComponent<Animator>();
        cursorAnimator = sword.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody2D>();
        sprRenderer = this.GetComponent<SpriteRenderer>();

        //VALUES
        angleFullWindow *= -1;
        performingAction = false;
        strokeOpponent = false;
        cursorAnimator.SetFloat("duration_strike", 1 / strike.actionDuration);
        cursorAnimator.SetFloat("duration_charge", 1 / charge.actionDuration);
        cursorAnimator.SetFloat("duration_parry", 1 / parry.actionDuration);

        //LISTENERS
        inputSystem.currentActionMap["Move"].performed += context => OnMovement(context);
        inputSystem.currentActionMap["Move"].canceled += context => OnMovement(context);

        inputSystem.currentActionMap["Look"].performed += context => OnLook(context);
        inputSystem.currentActionMap["Look"].canceled += context => OnLook(context);

        inputSystem.currentActionMap["Parry"].started += OnParry;
        inputSystem.currentActionMap["Strike"].started += OnStrike;

        //Charge cooldown is the "attack" skill cooldown, so it needs to consider the strike duration in its cooldown
        charge.cooldownDuration += strike.actionDuration;
        strike.BlankRefreshTime(Time.time);
        charge.BlankRefreshTime(Time.time);
        parry.BlankRefreshTime(Time.time);
        sword.Initialize(this);
    }

    private void Start() {
        playerIndex = GameManager.instance.NewPlayer(this);
    }

    // Update is called once per frame
    void Update() {
        sprRenderer.flipX = facingLeft;

        //TODO : FIXME
        if (performingAction) {
            if (strike.IsActionPerforming(Time.time)) {
                if (opponent) {
                    if (opponent.parry.IsActionPerforming(Time.time, strike.direction)) {
                        print("HEY : Reset");
                        Debug.Log("Opponent parried successfully");
                        ResetPerforming();
                        strike.BlankRefreshTime(Time.time);
                        strokeOpponent = false;
                        return;
                    } else {
                        strokeOpponent = true;
                    }
                }
            } else {
                print("HEY : Not performing");
                if (strokeOpponent) {
                    Debug.Log("Opponent being stroke successfully");
                    GameManager.instance.StrikeSuccessful(playerIndex);
                    ResetPerforming();
                    strokeOpponent = false;
                }
            }
        }
    }

    //INPUTS

    private void OnMovement(InputAction.CallbackContext value) {
        Vector2 movement = value.ReadValue<Vector2>();

        //Player cannot move on vertical axis
        movement.y = 0;
        /*
        rb.velocity = movement;
        animator.SetFloat("speed", movement.magnitude);
        if(movement.x < 0) {
            facingLeft = true;
        } else if(movement.x > 0){
            facingLeft = false;
        }
        */
    }

    private void OnLook(InputAction.CallbackContext value) {
        if (!performingAction) {
            look = value.ReadValue<Vector2>().normalized;
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
            currentDirection = (eDIRECTION)(nbDirections/Mathf.Abs(angleFullWindow / deg)-1);
            sword.transform.rotation = Quaternion.Euler(0, 0, deg);
        }
    }

    private void OnStrike(InputAction.CallbackContext value) {
        if (!performingAction) {
            if (charge.CanRefresh(Time.time)) {
                charge.Refresh(Time.time);
                performingAction = true;
                charge.direction = currentDirection;

                //animator.SetTrigger("strike");
                cursorAnimator.SetTrigger("strike");
                Invoke("Attack", charge.actionDuration);
                sprRenderer.color = Color.yellow;
            }
        }
    }

    private void OnParry(InputAction.CallbackContext value) {
        if (!performingAction) {
            if (parry.CanRefresh(Time.time)) {
                parry.Refresh(Time.time);
                performingAction = true;
                parry.direction = currentDirection;

                //animator.SetTrigger("parry");
                cursorAnimator.SetTrigger("parry");
                Invoke("ResetPerforming", parry.actionDuration);
                sprRenderer.color = Color.cyan;
            }
        }
    }

    //OTHER

    private void ResetPerforming() {
        performingAction = false;
        sprRenderer.color = Color.white;
    }

    public void Attack() {
        strike.Refresh(Time.time);
        strike.direction = charge.direction;
        sprRenderer.color = Color.black;
    }

    private IEnumerator Striking() {
        yield break;
    }
}
