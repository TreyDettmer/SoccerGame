using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : Player
{



    [SerializeField]
    protected RawImage kickIndicatorImage;

    protected Camera cam;
    protected FollowPlayer cameraFollowPlayer;

    protected InputActionAsset playerInputAsset;
    protected InputActionMap gameplayActionMap;
    
    protected PlayerInput playerInput;

    #region Select Sides Code
    protected InputActionMap selectSidesActionMap;
    [Header("GUI - Select Sides")]
    float previousMovementTime = 0f;
    // guiSection 1 means selecting sides, guiSection 2 means selecting AI count
    public int guiSection = 1;
    public Color color;
    #endregion

    private void Awake()
    {

        Rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerInputAsset = playerInput.actions;
        gameplayActionMap = playerInputAsset.FindActionMap("Player");
        gameplayActionMap.Disable();
        selectSidesActionMap = playerInputAsset.FindActionMap("SelectSides");
        cam = transform.parent.GetComponentInChildren<Camera>();
        cameraFollowPlayer = cam.GetComponent<FollowPlayer>();
        cameraPlayerGui = cam.GetComponent<PlayerGui>();
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(transform.parent.gameObject);
    }

    protected override void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (gameState == GameState.Gameplay)
        {
            base.FixedUpdate();
        }
    }

    protected void GetInput()
    {
        Vector2 direction = gameplayActionMap["Movement"].ReadValue<Vector2>();
        verticalInput = direction.y;
        horizontalInput = direction.x;

        if (isKicking)
        {
            //unitGoalVelocity = GetDirectionOfMovement(0f, verticalInput).normalized;
        }
        else
        {
            //unitGoalVelocity = GetDirectionOfMovement(horizontalInput, verticalInput).normalized;
        }
    }







    private void OnEnable()
    {
        selectSidesActionMap.Enable();
        gameplayActionMap.Enable();
    }


    private void OnDisable()
    {
        gameplayActionMap.Disable();
        selectSidesActionMap.Disable();
    }


    //protected override Vector3 GetDirectionOfMovement(float _horizontalInput, float _verticalInput)
    //{
    //    Vector3 forward = cam.transform.forward;
    //    Vector3 right = cam.transform.right;
    //    forward.y = 0;
    //    right.y = 0;
    //    forward = forward.normalized;
    //    right = right.normalized;
    //    Vector3 forwardRelativeVerticalInput = _verticalInput * forward;
    //    Vector3 rightRelativeHorizontalInput = _horizontalInput * right;
     
    //    return forwardRelativeVerticalInput + rightRelativeHorizontalInput;
    //}


    // Update is called once per frame
    private void Update()
    {
        if (gameState == GameState.Gameplay)
        {


            base.Update();
            // if we do not have the ball
            if (!HasBall)
            {
                // check if the ball is in a dribblable position
                if (CheckIfCanDribble())
                {
                    // check if ball isn't being dribbled by someone else
                    if (ball.owner == null)
                    {
                        // ensure that we are not sliding
                        if (!IsSliding)
                        {
                            // ensure that we are not being penalized for fouling
                            if (playerState != PlayerState.Penalized)
                            {
                                // check if it's been long enough since we last had the ball
                                if (CanDribble)
                                {
                                    HasBall = true;
                                    ball.SetOwner(this);
                                    Debug.Log("New owner");
                                    ball.transform.position = transform.position + transform.forward * 1.5f;
                                }
                            }

                        }

                    }
                }
            }
            if (isKicking)
            {
                kickBackswingElapsedTime += Time.deltaTime;
                float kickPowerCurveValue;
                if (chipModeEnabled)
                {
                    kickPowerCurveValue = chipPowerCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }
                else
                {
                    kickPowerCurveValue = kickPowerCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }

                kickForce = kickPowerFactor * kickPowerCurveValue;
                float kickHeightCurveValue;
                if (chipModeEnabled)
                {
                    kickHeightCurveValue = chipHeightCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }
                else
                {
                    kickHeightCurveValue = kickHeightCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }
                kickHeightForce = kickHeightFactor * kickHeightCurveValue;
                cameraPlayerGui.UpdatePowerMeter(kickBackswingElapsedTime / maxKickBackswingTime);
                if (kickBackswingElapsedTime >= maxKickBackswingTime)
                {
                    EndKick();
                }
            }

        }

    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (IsGrounded && playerState == PlayerState.Playing)
        {
            if (HasBall)
            {
                BallStolen();
            }
            IsGrounded = false;
            IsJumping = true;
            Rb.velocity = new Vector3(Rb.velocity.x, jumpForce, Rb.velocity.z);
        }
    }

    public void Kick(InputAction.CallbackContext context)
    {
        if (playerState == PlayerState.Playing)
        {
            if (context.started)
            {
                StartKick();
            }
            else if (context.canceled)
            {
                EndKick();
            }
        }
    }

    public override void StartKick()
    {
        base.StartKick();


    }



    public override void EndKick()
    {
        base.EndKick();
        cameraPlayerGui.UpdatePowerMeter(0f);

    }

    public void Slide(InputAction.CallbackContext context)
    {  
        if (IsSliding || context.canceled || !IsGrounded || playerState != PlayerState.Playing || !CanSlide || HasBall)
        {
            return;
        }

        IsSliding = true;
        //slideDirection = transform.forward;
        //slideDirection.y = 0;
        //slideDirection = slideDirection.normalized;
        //Vector3.Dot(slideDirection,)
        //slideDirection *= Vector3.Project(rb.velocity, slideDirection).magnitude;
        //animator.SetBool("isSliding", true);
        StartCoroutine(SlideRoutine());
        //transform.DORotate(new Vector3(-90f, 0f, 0f), slideTime/4f,RotateMode.LocalAxisAdd);
    }







   

    

    public void ToggleChipMode(InputAction.CallbackContext context)
    {
        if (isKicking)
        {
            return;
        }
        if (context.canceled)
        {
            chipModeEnabled = false;
        }
        else
        {
            chipModeEnabled = true;
        }
    }

    public void RequestRestart(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            //GameplayManager.instance.PlayerRequestedRestart(this, true);
        }
        else
        {
            //GameplayManager.instance.PlayerRequestedRestart(this);
        }
    }










    public override void ResetValues()
    {
        base.ResetValues();
        cam.transform.position = transform.position + -transform.forward * 3f + Vector3.up * 2f;
    }

    #region Select Sides Code

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            //SelectSidesGui.instance.PlayerMovedLeftOrRight(this, playerInput, true, guiSection);
        }

    }
    public void MoveRight(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            //SelectSidesGui.instance.PlayerMovedLeftOrRight(this, playerInput, false, guiSection);
        }
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            //SelectSidesGui.instance.PlayerMovedUpOrDown(this, playerInput, true, guiSection);
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            //SelectSidesGui.instance.PlayerMovedUpOrDown(this, playerInput, false, guiSection);
        }
    }

    public void ReadyUp(InputAction.CallbackContext context)
    {
        if (!context.canceled)
        {
            if (SelectSidesGui.instance == null)
            {
                return;
            }
            //SelectSidesGui.instance.PlayerReadiedUp(this, playerInput, true);
        }
    }
    #endregion

}
