using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{

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



    protected PlayerState playerState;

    public int teamIndex = -1;

    [Header("Appearance")]
    [SerializeField]
    protected MeshRenderer meshRenderer;
    public Material playingMaterial;
    public Material penalizedMaterial;




    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerState = PlayerState.Waiting;
        currentMaxSpeed = maxSpeed;
        ball = FindObjectOfType<Ball>();
        ballRb = ball.GetComponent<Rigidbody>();
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
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
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
}
