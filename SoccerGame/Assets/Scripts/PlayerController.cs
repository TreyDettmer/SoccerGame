using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : Player
{
    public enum ControllerType
    {
        Keyboard,
        Xbox,
        Switch
    }

    [SerializeField]
    protected RawImage kickIndicatorImage;
    [SerializeField]
    public ControllerType controllerType = ControllerType.Xbox;
    protected Camera cam;
    protected FollowPlayer cameraFollowPlayer;
    protected PlayerGui cameraPlayerGui;

    protected InputActionAsset playerInputAsset;
    protected InputActionMap playerActionMap;

    private void Awake()
    {
        playerInputAsset = GetComponent<PlayerInput>().actions;
        playerActionMap = playerInputAsset.FindActionMap("Player");
        cam = transform.parent.GetComponentInChildren<Camera>();
        cameraFollowPlayer = cam.GetComponent<FollowPlayer>();
        cameraPlayerGui = cam.GetComponent<PlayerGui>();
        animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();   
    }

    protected override void GetInput()
    {
        Vector2 direction = playerActionMap["Movement"].ReadValue<Vector2>();
        verticalInput = direction.y;
        horizontalInput = direction.x;

        if (isKicking)
        {
            unitGoalVelocity = GetDirectionOfMovement(0f, verticalInput).normalized;
        }
        else
        {
            unitGoalVelocity = GetDirectionOfMovement(horizontalInput, verticalInput).normalized;
        }
        
        

        
        
    }




    public void ToggleSprint(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            isSprinting = false;
        }
        else
        {
            isSprinting = true;
        }
    }


    private void OnEnable()
    {
        playerActionMap.Enable();
    }


    private void OnDisable()
    {
        playerActionMap.Disable();
    }


    protected override Vector3 GetDirectionOfMovement(float _horizontalInput, float _verticalInput)
    {
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;
        Vector3 forwardRelativeVerticalInput = _verticalInput * forward;
        Vector3 rightRelativeHorizontalInput = _horizontalInput * right;

        
        return forwardRelativeVerticalInput + rightRelativeHorizontalInput;
        
        
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // if we do not have the ball
        if (!hasBall)
        {
            // check if the ball is in a dribblable position
            if (CheckIfCanDribble())
            {
                // check if ball isn't being dribbled by someone else
                if (ball.owner == null)
                {
                    // ensure that we are not sliding
                    if (!isSliding)
                    {
                        // ensure that we are not being penalized for fouling
                        if (playerState != PlayerState.Penalized)
                        {
                            // check if it's been long enough since we last had the ball
                            if (canDribble)
                            {
                                hasBall = true;
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


    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && playerState == PlayerState.Playing)
        {
            if (hasBall)
            {
                BallStolen();
            }
            isGrounded = false;
            isJumping = true;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
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
        if (!isKicking && !isSliding)
        {
            Debug.Log("Starting kick");
            isKicking = true;
            canKick = false;
        }

    }



    public override void EndKick()
    {
        base.EndKick();
        if (!isKicking)
        {
            return;
        }

        if (ball != null)
        {
            if (CheckIfCanDribble())
            {
                if (hasBall)
                {
                    Vector3 directionToBall = (ball.transform.position - transform.position).normalized;
                    Vector3 curveDirection = (transform.right * horizontalInput).normalized;
                    curveDirection.y = 0;
                    curveDirection = curveDirection.normalized;
                    directionToBall.y = 0;
                    directionToBall = directionToBall.normalized;
                    Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                    float dot = Vector3.SignedAngle(curveDirection, forward, Vector3.up);
                    Debug.Log("Dot: " + dot);
                    Vector3 torque = new Vector3(0f, -dot * kickSpinFactor, 0f);

                    ball.lastKickedBy = this;
                    ball.SetOwner(null);
                    hasBall = false;
                    StartCoroutine(BallPickupCooldownRoutine());
                    ballRb.AddForce(directionToBall * kickForce + new Vector3(0f, kickHeightForce, 0f), ForceMode.Impulse);

                    ballRb.AddForce(directionToBall * kickForce + new Vector3(0f, kickHeightForce, 0f), ForceMode.Impulse);
                    ballRb.AddTorque(torque, ForceMode.VelocityChange);
                }
            }

        }



        cameraPlayerGui.UpdatePowerMeter(0f);
        isKicking = false;
        kickBackswingElapsedTime = 0f;
    }

    public void Slide(InputAction.CallbackContext context)
    {  
        if (isSliding || context.canceled || !isGrounded || playerState != PlayerState.Playing || !canSlide || hasBall)
        {
            return;
        }

        isSliding = true;
        slideDirection = transform.forward;
        slideDirection.y = 0;
        slideDirection = slideDirection.normalized;
        //Vector3.Dot(slideDirection,)
        //slideDirection *= Vector3.Project(rb.velocity, slideDirection).magnitude;
        //animator.SetBool("isSliding", true);
        StartCoroutine(SlideRoutine());
        //transform.DORotate(new Vector3(-90f, 0f, 0f), slideTime/4f,RotateMode.LocalAxisAdd);
    }

    IEnumerator SlideRoutine()
    {
        yield return new WaitForSeconds(slideTime * .75f);
        //transform.DORotate(new Vector3(90f, 0f, 0f), slideTime / 4f, RotateMode.LocalAxisAdd);
        yield return new WaitForSeconds(slideTime * .25f);
        EndSlide();
    }

    IEnumerator SlideCooldownRoutine()
    {
        yield return new WaitForSeconds(slideCooldownTime);
        canSlide = true;
    }

    public void EndSlide()
    {
        //animator.SetBool("isSliding", false);

        slidIntoBall = false;
        isSliding = false;
        if (shouldBePenalized)
        {
            shouldBePenalized = false;
            UpdatePlayerState(PlayerState.Penalized);
        }
        canSlide = false;
        StartCoroutine(SlideCooldownRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if (isSliding)
            {
                slidIntoBall = true;
            }
            if (!hasBall)
            {
                if (ball.owner == null)
                {

                }
                else if (ball.owner != this)
                {
                    ball.owner.BallStolen();
                    StartCoroutine(BallPickupCooldownRoutine());
                    //hasBall = true;
                    //ball.SetOwner(this);
                    //Debug.Log("New owner");
                }


            }

        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.collider.GetComponent<PlayerController>())
            {
                if (collision.collider.GetComponent<PlayerController>().teamIndex != teamIndex)
                {
                    if (isSliding && slidIntoBall == false)
                    {
                        GameplayManager.instance.PlayerFoul(this, collision.collider.GetComponent<PlayerController>());
                        Debug.Log("Foul on team " + teamIndex);
                    }
                }
            }

        }
    }

    public void UpdatePlayerState(PlayerState newState)
    {
        if (newState == PlayerState.Penalized)
        {
            if (playerState != PlayerState.Penalized)
            {
                if (isSliding)
                {
                    shouldBePenalized = true;
                    return;
                }
                playerState = PlayerState.Penalized;
                meshRenderer.material = penalizedMaterial;
                gameObject.layer = LayerMask.NameToLayer("PlayerPenalized");
                StartCoroutine(PenaltyTimeoutRoutine());
                

            }
        }
        else if (newState == PlayerState.Playing)
        {
            if (playerState != PlayerState.Playing)
            {
                gameObject.layer = LayerMask.NameToLayer("Player");
                meshRenderer.material = playingMaterial;
                playerState = PlayerState.Playing;
            }
        }
        else if (newState == PlayerState.Waiting)
        {
            playerState = PlayerState.Waiting;
            gameObject.layer = LayerMask.NameToLayer("Player");
            meshRenderer.material = playingMaterial;
        }
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
            GameplayManager.instance.PlayerRequestedRestart(this, true);
        }
        else
        {
            GameplayManager.instance.PlayerRequestedRestart(this);
        }
    }

    public void UpdateMaterial()
    {

        meshRenderer.material = playingMaterial;    
    }

    public bool CheckIfCanDribble()
    {
        if ((ball.transform.position - transform.position).sqrMagnitude <= ballcanDribbleDistance)
        {
            Vector3 dir = (ball.transform.position - transform.position).normalized;
            dir.y = 0;
            dir = dir.normalized * 3f;
            Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            float angle = Vector3.SignedAngle(forward, dir.normalized, Vector3.up);
            if (angle >= -ballcanDribbleAngle && angle <= ballcanDribbleAngle)
            {
                if (Mathf.Abs(ball.transform.position.y - transform.position.y) < 1f)
                {
                    return true;
                }
                

            }

        }
        return false;
    }
    IEnumerator PenaltyTimeoutRoutine()
    {
        yield return new WaitForSeconds(penaltyTime);
        UpdatePlayerState(PlayerState.Playing);
    }

    public void BallStolen()
    {

        ball.SetOwner(null);
        hasBall = false;
        StartCoroutine(BallPickupCooldownRoutine());
    }

    IEnumerator BallPickupCooldownRoutine()
    {
        canDribble = false;
        yield return new WaitForSeconds(ballcanDribbleCooldown);
        canDribble = true;

    }

    public void ResetValues()
    {
        hasBall = false;
        canDribble = true;
        cam.transform.position = transform.position + -transform.forward * 3f + Vector3.up * 2f;
    }
}
