using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //CLASSES
    [System.Serializable]
    public class Action {
        public Cooldown cooldown;
        public float actionDuration;

        public bool IsActionPerforming(float time) {
            return (time <= cooldown.lastRefreshTime + actionDuration);
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
    private SpriteRenderer sprRenderer;
    private Rigidbody2D rb;
    [Header("REFERENCES")]
    [SerializeField] private Sword sword;
    [Header("PARAMETERS")]
    [SerializeField] private Action strike;
    [SerializeField] private Action parry;
    //OTHER
    private int playerIndex;
    private bool facingLeft;
    private Vector2 look;

    private void Awake() {
        inputSystem = this.GetComponent<PlayerInput>();
        animator = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody2D>();
        sprRenderer = this.GetComponent<SpriteRenderer>();

        sword.Initialize(this);

        inputSystem.currentActionMap["Move"].performed += context => OnMovement(context);
        inputSystem.currentActionMap["Move"].canceled += context => OnMovement(context);

        inputSystem.currentActionMap["Look"].performed += context => OnLook(context);
        inputSystem.currentActionMap["Look"].canceled += context => OnLook(context);

        inputSystem.currentActionMap["Parry"].started += OnParry;
        inputSystem.currentActionMap["Strike"].started += OnStrike;

        strike.cooldown.BlankRefreshTime(Time.time);
        parry.cooldown.BlankRefreshTime(Time.time);
    }

    private void Start() {
        playerIndex = GameManager.instance.NewPlayer(this);
    }

    // Update is called once per frame
    void Update()
    {
        sprRenderer.flipX = facingLeft;
    }

    //INPUTS

    private void OnMovement(InputAction.CallbackContext value) {
        Vector2 movement = value.ReadValue<Vector2>();

        //Player cannot move on vertical axis
        movement.y = 0;

        rb.velocity = movement;
        animator.SetFloat("speed", movement.magnitude);
        if(movement.x < 0) {
            facingLeft = true;
        } else if(movement.x > 0){
            facingLeft = false;
        }
    }

    private void OnLook(InputAction.CallbackContext value) {
        look = value.ReadValue<Vector2>().normalized;
        print("Look :" + look);
        if(look != Vector2.zero)
            sword.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, (90f+(90*(-look.y))) * ((facingLeft) ? 1f : -1f)));
    }

    private void OnStrike(InputAction.CallbackContext value) {
        if(strike.cooldown.CanRefresh(Time.time)) {
            strike.cooldown.Refresh(Time.time);
            animator.SetTrigger("strike");
            Attack();
        }
    }

    private void OnParry(InputAction.CallbackContext value) {
        if (parry.cooldown.CanRefresh(Time.time)) {
            parry.cooldown.Refresh(Time.time);
            animator.SetTrigger("parry");
        }
    }

    //OTHER

    public bool Attack() {
        //If the opponent sword is being overlapped
        if (sword.overlappedSword) {
            if (sword.overlappedSword.owner.parry.IsActionPerforming(Time.time)) {
                //The opponent successfully parried
                Debug.Log("Opponent parried");
                return false;
            } else {
                //The opponent is being stroke
                Debug.Log("Opponent being stroke");
                return true;
            }
        }
        Debug.Log("No opponent in range");
        return false;
    }
}
