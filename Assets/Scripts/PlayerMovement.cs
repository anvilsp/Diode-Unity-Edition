using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState { Idle, Walk, Jump, HandstandIdle, HandstandWalk, HoldIdle, HoldWalk, HoldJump, Climb };

    private float horizontal;
    private static float defaultSpeed = 4.5f;
    private static float defaultJumpingPower = 8f;
    private static float gravityScale = 0.8f;
    private static float apexGravity = 0.65f;
    private float speed = defaultSpeed;
    private float jumpingPower = defaultJumpingPower;
    private float airTime = 0f; // cooldown
    private bool isFacingRight = true;
    private bool holdingItem = false;

    PlayerState state = PlayerState.Idle; // current player state


    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask hazardLayer;
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private LayerMask doorLayer;


    [SerializeField] private Animator animator;
    [SerializeField] private LevelController level;
    [SerializeField] private GameObject levelObj;
    [SerializeField] private GameObject heldItem;
    [SerializeField] private GameObject keyPrefab;
    private Scene currentScene;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        animator.Play("Idle");
    }

    private void Update()
    {

        if(IsTouchingHazard() || (IsTouchingDoor() && state == PlayerState.HoldIdle))
        {
            print(currentScene.name);
            SceneManager.LoadScene(currentScene.name);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if(holdingItem)
            {
                // throw the item
                GameObject newKey;
                newKey = Instantiate(keyPrefab, new Vector3(heldItem.transform.position.x, heldItem.transform.position.y, heldItem.transform.position.z), Quaternion.identity) as GameObject;
                Key keyController = newKey.GetComponent<Key>();
                keyController.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                keyController.rb.velocity = new Vector2(5 * Mathf.Sign(transform.localScale.x), 5);
                keyController.level = levelObj;
                keyController.StartTimer();
                state = PlayerState.Idle;
                holdingItem = false;
                animator.Play("Idle");
                level.keyObj = newKey;
            }
            else if(IsOnItem() && (state == PlayerState.Idle || state == PlayerState.Walk || state == PlayerState.Jump))
            {
                // pick up the item
                state = PlayerState.HoldIdle;
                holdingItem = true;
                animator.Play("HoldIdle");
                Destroy(level.keyObj);
            }
            heldItem.SetActive(holdingItem);
        }

        if(IsTouchingLadder() && Input.GetAxis("Vertical") != 0 && state == PlayerState.Idle)
        {
            animator.Play("Climb");
            transform.position = new Vector3(transform.position.x, transform.position.y + (0.1f * Input.GetAxis("Vertical")), transform.position.z);
            rb.gravityScale = 0f;
            state = PlayerState.Climb;
        }

        if (Input.GetButtonDown("Jump") && Input.GetAxis("Vertical") < 0 && IsGrounded() && (state == PlayerState.Idle || state == PlayerState.Walk))
        {
            rb.velocity = new Vector2(0, 5);
            state = PlayerState.HandstandIdle;
            animator.Play("HandstandIdle");
            speed = 2.5f;
            jumpingPower = 10f;
        }


        else if (Input.GetButtonDown("Jump"))
        {
            print(IsGrounded());
            if(IsGrounded())
            {
                bool wasHandstand = false;
                if (state == PlayerState.HandstandIdle || state == PlayerState.HandstandWalk)
                {
                    wasHandstand = true;
                }
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                if (state ==  PlayerState.HoldIdle || state == PlayerState.HoldWalk)
                {
                    state = PlayerState.HoldJump;
                    animator.Play("HoldJump");
                }
                else
                {
                    state = PlayerState.Jump;
                    animator.Play("Jump");
                }
                if (wasHandstand)
                {
                    speed = defaultSpeed;
                    jumpingPower = defaultJumpingPower;
                }
                print(state);
            }
        }

        if (Input.GetButtonUp("Jump") && (state == PlayerState.Jump || state == PlayerState.HoldJump))
        {
            if (rb.velocity.y > 0f)
            {
                if (state == PlayerState.HoldJump)
                {
                    animator.Play("HoldFall");
                    state = PlayerState.HoldIdle;
                }
                else
                {
                    animator.Play("Falling");
                    state = PlayerState.Idle;
                }
                rb.gravityScale = gravityScale;
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }

        // pseudo state machine - still have to use a lot of if statements :(
        switch (state)
        {
            case PlayerState.Idle:
                // switch to walking state if the player is moving
                if (IsGrounded() && (int) rb.velocity.x != 0) // rigidbody is annoying
                {
                    animator.Play("Walk");
                    state = PlayerState.Walk;
                    print(state);
                }
                // play the idle animation if the wrong anim plays for some reason
                if (IsGrounded() && rb.velocity.y == 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    animator.Play("Idle");
                break;
            case PlayerState.Walk:
                // switch back to idle if the player is grounded and not walking
                if (IsGrounded() && (int) rb.velocity.x == 0)
                {
                    animator.Play("Idle");
                    state = PlayerState.Idle;
                    print(state);
                }

                // play the walk anim if it's not playing
                if (IsGrounded() && rb.velocity.y != 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                    animator.Play("Walk");
                break;
            case PlayerState.Jump:
                airTime += Time.deltaTime;
                // switch back to idle if the player is grounded
                if (IsGrounded() && airTime > 0.3) // added a cooldown because this is too trigger happy
                {
                    animator.Play("Idle");
                    airTime = 0;
                    state = PlayerState.Idle;
                    print(state);
                }

                // switch to falling anim if the player is descending
                if (rb.velocity.y < 0)
                {
                    animator.Play("Falling");
                }

                // floatiness at the apex of the jump
                if (rb.velocity.y < 10 && rb.velocity.y > 0 && rb.gravityScale != apexGravity)
                {
                    rb.gravityScale = apexGravity;
                    print("set apex gravity");
                }
                // KILL floatiness >:)
                else if (rb.gravityScale == apexGravity && rb.velocity.y < 0)
                {
                    rb.gravityScale = gravityScale;
                    print("set default gravity");
                }
                break;
            case PlayerState.HandstandIdle:
                // change to walking state if needed
                if (IsGrounded() && (int) rb.velocity.x != 0)
                {
                    animator.Play("HandstandWalk");
                    state = PlayerState.HandstandWalk;
                }
                break;
            case PlayerState.HandstandWalk:
                // change to idle state if needed
                if (IsGrounded() && (int) rb.velocity.x == 0)
                {
                    animator.Play("HandstandIdle");
                    state = PlayerState.HandstandIdle;
                }
                break;
            case PlayerState.HoldWalk:
                // switch back to idle if the player is grounded and not walking
                if (IsGrounded() && (int)rb.velocity.x == 0)
                {
                    animator.Play("HoldIdle");
                    state = PlayerState.HoldIdle;
                    print(state);
                }

                // play the walk anim if it's not playing
                if (IsGrounded() && rb.velocity.y != 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("HoldWalk"))
                    animator.Play("HoldWalk");
                break;
            case PlayerState.HoldIdle:
                // switch to walking state if the player is moving
                if (IsGrounded() && (int)rb.velocity.x != 0) // rigidbody is annoying
                {
                    animator.Play("HoldWalk");
                    state = PlayerState.HoldWalk;
                    print(state);
                }
                // play the idle animation if the wrong anim plays for some reason
                if (IsGrounded() && rb.velocity.y == 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("HoldIdle"))
                    animator.Play("HoldIdle");
                break;
            case PlayerState.HoldJump:
                airTime += Time.deltaTime;
                if (IsGrounded() && airTime > 0.3) // added a cooldown because this is too trigger happy
                {
                    animator.Play("HoldIdle");
                    airTime = 0;
                    state = PlayerState.HoldIdle;
                    print(state);
                }

                // switch to falling anim if the player is descending
                if (rb.velocity.y < 0)
                {
                    animator.Play("HoldFall");
                }

                // floatiness at the apex of the jump
                if (rb.velocity.y < 10 && rb.velocity.y > 0 && rb.gravityScale != apexGravity)
                {
                    rb.gravityScale = apexGravity;
                    print("set apex gravity");
                }
                // KILL floatiness >:)
                else if (rb.gravityScale == apexGravity && rb.velocity.y < 0)
                {
                    rb.gravityScale = gravityScale;
                    print("set default gravity");
                }
                break;
            case PlayerState.Climb:
                animator.Play("Climb");
                if(IsGrounded() || !IsTouchingLadder() || Input.GetButtonDown("Fire1"))
                {
                    animator.Play("Idle");
                    state = PlayerState.Idle;
                    rb.gravityScale = gravityScale;
                }
                break;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        GetInput();
        //ApplyMovement();
    }

    private void GetInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // player can only change direction midair if they've passed the apex of their jump - this mirrors the behavior of the mario vs donkey kong remake
        if((state != PlayerState.Jump || (state == PlayerState.Jump && (rb.velocity.y < -2 || (int) rb.velocity.x == 0))) && state != PlayerState.Climb)
        {
            // implementation is half-baked - could maybe make this work with discrete state system
            //float useSpeed;
            //if (state == PlayerState.Jump && rb.velocity.x != 0)
            //    useSpeed = speed / 2;
            //else
            //    useSpeed = speed;
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
            Flip();
        }
        else if(state == PlayerState.Climb)
        {
            rb.velocity = new Vector2(0, vertical * (speed / 2));
        }
    }   


    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public bool IsOnItem()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, itemLayer);
    }

    public bool IsTouchingHazard()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, hazardLayer);
    }

    public bool IsTouchingLadder()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, ladderLayer);
    }

    public bool IsTouchingDoor()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, doorLayer);
    }    

    public PlayerState GetState()
    {
        return state;
    }

}
