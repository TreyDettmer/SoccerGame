using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{

    public enum ControllerType
    {
        Keyboard,
        Xbox,
        Switch,
        AI
    }

    public enum PlayerState
    {
        Penalized,
        Waiting,
        Playing
    }



    [Header("Movement")]
    public float maxSpeed;
    public float acceleration;
    public float maxAccelForce;
    protected float speedFactor = 1f;
    protected float maxAccelForceFactor = 1f;
    public Vector3 forceScale;
    public float rotationSpeed = 1f;
    public float kickingRotationSpeed = 0.8f;
    protected Vector3 goalVelocity = Vector3.zero;
    protected Vector3 unitGoalVelocity;
    public AnimationCurve AccelerationFactorFromDot;
    public AnimationCurve MaxAccelerationFactorFromDot;
    public Transform graphicsTransform;
    protected float horizontalInput;
    protected float verticalInput;
    protected float currentMaxSpeed;

    [Header("Physics")]
    public float defaultHeight;
    public float springStrength;
    public float springDampening;
    public float jumpForce;
    public float uprightSprintStrength;
    public float uprightSpringDampening;

    public float legSpringStrength;
    public float legSpringDampening;

    [Header("Kicking")]
    public AnimationCurve kickPowerCurve;
    public AnimationCurve kickHeightCurve;
    public AnimationCurve chipPowerCurve;
    public AnimationCurve chipHeightCurve;
    public float kickHeightFactor = 10f;
    public float kickPowerFactor = 30f;
    public float kickSpinFactor = 20f;
    public float maxKickBackswingTime = 1.5f;
    protected float kickForce = 0f;
    protected float kickHeightForce = 0f;
    protected bool isKicking = false;
    protected bool canKick = true;
    protected bool chipModeEnabled = false;
    protected float kickBackswingElapsedTime = 0f;


    [SerializeField]
    protected LayerMask ballLayerMask;
    protected bool isWithinKickingRange = false;
    protected Ball ball;
    protected Rigidbody ballRb;


    protected Animator animator;
    [Header("Action")]
    public bool isSliding = false;
    public float slideForce = 5f;
    public float maxSlideSpeed = 10f;
    public float slideTime = 1f;
    public float slideCooldownTime = 0.5f;
    public float penaltyTime = 2f;
    protected Vector3 slideDirection = Vector3.zero;
    protected bool slidIntoBall = false;
    protected bool shouldBePenalized = false;
    protected bool canSlide = true;
    public float ballDribbleHelpForce = 5f;
    public float ballDribbleDirectionFactor = .3f;
    public float sprintingMaxSpeed = 4f;
    protected bool isSprinting = false;
    protected bool canDribble = true;
    [HideInInspector]
    public bool hasBall = false;
    public float ballcanDribbleAngle = 45f;
    public float ballcanDribbleDistance = 4f;
    public float ballcanDribbleCooldown = 1f;

    protected bool isGrounded = false;
    public LayerMask groundLayerMask;
    public Transform groundChecker;
    protected Rigidbody rb;
    protected Quaternion goalRotation;
    protected bool isJumping = false;

    [SerializeField]
    public ControllerType controllerType = ControllerType.Xbox;
    protected Goalie myGoalie;



    protected PlayerState playerState;

    public int teamIndex = -1;

    [Header("Appearance")]
    [SerializeField]
    protected MeshRenderer meshRenderer;
    public Material playingMaterial;
    public Material penalizedMaterial;

    protected AudioSource audioSource;




    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerState = PlayerState.Waiting;
        currentMaxSpeed = maxSpeed;
        ball = FindObjectOfType<Ball>();
        ballRb = ball.GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        Goalie[] goalieObjects = FindObjectsOfType<Goalie>();
        for (int i = 0; i < goalieObjects.Length; i++)
        {
            if (goalieObjects[i].teamIndex == teamIndex)
            {
                myGoalie = goalieObjects[i];
            }
        }
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        if (playerState == PlayerState.Playing)
        {
            GetInput();
            if (isSliding)
            {
                if (rb.velocity.sqrMagnitude < maxSlideSpeed * maxSlideSpeed)
                {
                    rb.AddForce(slideDirection * slideForce);

                }
                return;
            }
            if ((horizontalInput != 0 || verticalInput != 0))
            {
                goalRotation = Quaternion.LookRotation(unitGoalVelocity, Vector3.up);
                Vector3 motionDirection = GetDirectionOfMovement(horizontalInput, verticalInput).normalized;
                Quaternion toRotation = Quaternion.LookRotation(motionDirection, Vector3.up);
                if (isKicking)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, kickingRotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
                }
                
            }


            Vector3 unitVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            float velDot = Vector3.Dot(unitGoalVelocity, unitVelocity);
            float accel = acceleration * AccelerationFactorFromDot.Evaluate(velDot);
            Vector3 _goalVelocity = unitGoalVelocity * currentMaxSpeed * speedFactor;

            goalVelocity = Vector3.MoveTowards(goalVelocity, _goalVelocity, accel * Time.fixedDeltaTime);
            Vector3 neededAccel = (goalVelocity - rb.velocity) / Time.fixedDeltaTime;

            float maxAccel = maxAccelForce * MaxAccelerationFactorFromDot.Evaluate(velDot) * maxAccelForceFactor;

            neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
            rb.AddForce(Vector3.Scale(neededAccel * rb.mass, forceScale));



        }

        RaycastHit hit;
        if (Physics.Raycast(groundChecker.position, -transform.up, out hit, defaultHeight, groundLayerMask))
        {
            if (!isJumping)
            {
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
            if (isJumping)
            {
                isJumping = false;
            }

        }
        if (isSprinting)
        {
            currentMaxSpeed = sprintingMaxSpeed;
        }
        else
        {
            currentMaxSpeed = maxSpeed;
        }
    }

    protected virtual void GetInput()
    {

    }

    protected virtual Vector3 GetDirectionOfMovement(float _horizontalInput, float _verticalInput)
    {
        return Vector3.zero;
    }

    public virtual void StartKick()
    {

    }

    public virtual void EndKick()
    {

    }

    public virtual void UpdateMaterial()
    {

        meshRenderer.material = playingMaterial;
    }

    public virtual void ResetValues()
    {
        hasBall = false;
        canDribble = true;
    }

    public virtual void UpdatePlayerState(PlayerState newState)
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

    protected IEnumerator PenaltyTimeoutRoutine()
    {
        yield return new WaitForSeconds(penaltyTime);
        UpdatePlayerState(PlayerState.Playing);
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

    public void BallStolen()
    {

        ball.SetOwner(null);
        hasBall = false;
        StartCoroutine(BallPickupCooldownRoutine());
    }

    protected IEnumerator BallPickupCooldownRoutine()
    {
        canDribble = false;
        yield return new WaitForSeconds(ballcanDribbleCooldown);
        canDribble = true;

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

    protected IEnumerator SlideCooldownRoutine()
    {
        yield return new WaitForSeconds(slideCooldownTime);
        canSlide = true;
    }

    protected IEnumerator SlideRoutine()
    {
        yield return new WaitForSeconds(slideTime * .75f);
        //transform.DORotate(new Vector3(90f, 0f, 0f), slideTime / 4f, RotateMode.LocalAxisAdd);
        yield return new WaitForSeconds(slideTime * .25f);
        EndSlide();
    }
}
