using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed;
    public float acceleration;
    public float maxAccelForce;
    private float speedFactor = 1f;
    private float maxAccelForceFactor = 1f;
    public Vector3 forceScale;
    public float rotationSpeed = 1f;
    Vector3 goalVelocity = Vector3.zero;
    private Vector3 unitGoalVelocity;
    public AnimationCurve AccelerationFactorFromDot;
    public AnimationCurve MaxAccelerationFactorFromDot;
    public Transform graphicsTransform;
    private float horizontalInput;
    private float verticalInput;
    private float fixedHorizontalInput;
    private float fixedVerticalInput;
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
    public float kickHeightFactor = 10f; 
    public float kickPowerFactor = 30f;
    public float kickSpinFactor = 20f;
    public float maxKickBackswingTime = 1.5f;
    private float kickForce = 0f;
    private float kickHeightForce = 0f;
    private bool isKicking = false;
    private bool canKick = true;
    private float kickBackswingElapsedTime = 0f;
    

    [SerializeField]
    private LayerMask ballLayerMask;
    private bool isWithinKickingRange = false;
    private Ball ball;
    [SerializeField]
    private RawImage kickIndicatorImage;

    
    public AnimationCurve backswingAngleFactorFromTime;
    public float maxBackswingAngle;
    public float maxDownswingAngle;
    public float kickSwingSpeed = 60f;

    
    private Animator animator;
    [Header("Action")]
    public bool isSliding = false;
    public float slideSpeed = 5f;
    public float slideTime = 1f;
    private Vector3 slideDirection = Vector3.zero;
    private bool slidIntoBall = false;

    private bool isGrounded = false;
    public LayerMask groundLayerMask;
    public Transform groundChecker;
    private Rigidbody rb;
    private Vector3 inputVelocity;
    private Vector3 movementDirection;
    private Quaternion goalRotation;
    private bool isJumping = false;

    private InputActionAsset playerInputAsset;
    private InputActionMap playerActionMap;
    private PlayerInput playerInput;
    private Camera cam;
    public int teamIndex = -1;



    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputAsset = GetComponent<PlayerInput>().actions;
        playerActionMap = playerInputAsset.FindActionMap("Player");
        cam = transform.parent.GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isSliding)
        {
            if (rb.velocity.sqrMagnitude < 5f)
            {
                rb.AddForce(slideDirection * slideSpeed);
                
            }
            return;
        }
        Vector2 direction = playerActionMap["Movement"].ReadValue<Vector2>();
        verticalInput = direction.y;
        horizontalInput = direction.x;


        //unitGoalVelocity = new Vector3(horizontalInput, 0f, verticalInput);
        unitGoalVelocity = GetDirectionOfMovement(horizontalInput,verticalInput).normalized;
        Debug.DrawRay(transform.position, unitGoalVelocity * 5f, Color.green);
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.black);
        //unitGoalVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        //Debug.Log("Input: (" + horizontalInput + "," + verticalInput + ")");

        if (horizontalInput != 0 || verticalInput != 0)
        {
            goalRotation = Quaternion.LookRotation(unitGoalVelocity, Vector3.up);
            Vector3 motionDirection = GetDirectionOfMovement(horizontalInput, verticalInput).normalized;
            Quaternion toRotation = Quaternion.LookRotation(motionDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            //Vector3 straight = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            //goalRotation = Quaternion.LookRotation(straight, Vector3.up);
            //Vector3 motionDirection = new Vector3(rb.velocity.x, 0f, rb.velocity.z).normalized;
            ////transform.rotation = Quaternion.LookRotation(straight, Vector3.up);
        }
        Debug.DrawRay(groundChecker.position, -transform.up * defaultHeight, Color.red);
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


            
        Vector3 unitVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float velDot = Vector3.Dot(unitGoalVelocity, unitVelocity);
        float accel = acceleration * AccelerationFactorFromDot.Evaluate(velDot);
        Vector3 _goalVelocity = unitGoalVelocity * maxSpeed * speedFactor;

        goalVelocity = Vector3.MoveTowards(goalVelocity, _goalVelocity, accel * Time.fixedDeltaTime);
        Vector3 neededAccel = (goalVelocity - rb.velocity) / Time.fixedDeltaTime;

        float maxAccel = maxAccelForce * MaxAccelerationFactorFromDot.Evaluate(velDot) * maxAccelForceFactor;

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
        //Debug.Log("Ground force: " + Vector3.Scale(neededAccel * rb.mass, forceScale));
        rb.AddForce(Vector3.Scale(neededAccel * rb.mass, forceScale));


    }

    

    private void OnDrawGizmosSelected()
    {
        
    }

    private void OnEnable()
    {
        playerActionMap.Enable();
    }


    private void OnDisable()
    {
        playerActionMap.Disable();
    }


    private Vector3 GetDirectionOfMovement(float _horizontalInput, float _verticalInput)
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
    void Update()
    {
        if (isKicking)
        {
            kickBackswingElapsedTime += Time.deltaTime;
            float kickPowerCurveValue = kickPowerCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
            kickForce = kickPowerFactor * kickPowerCurveValue;
            float kickHeightCurveValue = kickHeightCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
            kickHeightForce = kickHeightFactor * kickHeightCurveValue;
            GameplayGui.instance.UpdatePowerMeter(teamIndex, kickBackswingElapsedTime / maxKickBackswingTime);
            if (kickBackswingElapsedTime >= maxKickBackswingTime)
            {
                EndKick();
            }
        }


    }

    public void UpdateUprightForce()
    {
        Quaternion characterCurrent = transform.rotation;
        Quaternion toGoal = ShortestRotation(goalRotation, characterCurrent);
        Vector3 rotAxis;
        float rotDegrees;
        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();
        float rotRadians = rotDegrees * Mathf.Deg2Rad;
        rb.AddTorque((rotAxis * (rotRadians * uprightSprintStrength)) - (rb.angularVelocity * uprightSpringDampening));
    }

    public static Quaternion ShortestRotation(Quaternion a, Quaternion b)

    {

        if (Quaternion.Dot(a, b) < 0)

        {

            return a * Quaternion.Inverse(Multiply(b, -1));

        }

        else return a * Quaternion.Inverse(b);

    }



    public static Quaternion Multiply(Quaternion input, float scalar)

    {

        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);

    }




    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            isGrounded = false;
            isJumping = true;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    public void Kick(InputAction.CallbackContext context)
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

    public void StartKick()
    {
        
        if (!isKicking && !isSliding)
        {
            Debug.Log("Starting kick");
            isKicking = true;
            canKick = false;
        }

    }



    public void EndKick()
    {
        if (!isKicking)
        {
            return;
        }

        if (ball != null)
        {
            Vector3 directionToBall = (ball.transform.position - transform.position).normalized;
            directionToBall.y = 0;
            directionToBall = directionToBall.normalized;
            Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            float dot = Vector3.SignedAngle(directionToBall, forward,Vector3.up);
            Debug.Log("Dot: " + dot);
            Vector3 torque = new Vector3(0f,-dot * kickSpinFactor,0f);

            ball.GetComponent<Rigidbody>().AddForce(directionToBall * kickForce + new Vector3(0f,kickHeightForce,0f), ForceMode.Impulse);
            ball.GetComponent<Rigidbody>().AddTorque(torque, ForceMode.VelocityChange);
        }



        GameplayGui.instance.UpdatePowerMeter(teamIndex, 0f);
        isKicking = false;
        kickBackswingElapsedTime = 0f;
    }

    public void Slide(InputAction.CallbackContext context)
    {  
        if (isSliding || context.canceled || !isGrounded)
        {
            return;
        }
        Debug.Log("Slide");
        isSliding = true;
        slideDirection = transform.forward;
        slideDirection.y = 0;
        slideDirection = slideDirection.normalized;
        //Vector3.Dot(slideDirection,)
        //slideDirection *= Vector3.Project(rb.velocity, slideDirection).magnitude;
        //animator.SetBool("isSliding", true);
        StartCoroutine(SlideRoutine());
        transform.DORotate(new Vector3(-90f, 0f, 0f), slideTime/4f,RotateMode.LocalAxisAdd);
    }

    IEnumerator SlideRoutine()
    {
        yield return new WaitForSeconds(slideTime * .75f);
        transform.DORotate(new Vector3(90f, 0f, 0f), slideTime / 4f, RotateMode.LocalAxisAdd);
        yield return new WaitForSeconds(slideTime * .25f);
        EndSlide();
    }

    public void EndSlide()
    {
        //animator.SetBool("isSliding", false);

        slidIntoBall = false;
        isSliding = false;
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if (isSliding)
            {
                slidIntoBall = true;
            }
            
        }
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.collider.GetComponent<PlayerController>())
            {
                if (collision.collider.GetComponent<PlayerController>().teamIndex != teamIndex)
                {
                    if (isSliding && slidIntoBall == false)
                    {
                        Debug.Log("Foul on team " + teamIndex);
                    }
                }
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if (!isWithinKickingRange)
            {
                isWithinKickingRange = true;
                kickIndicatorImage.color = new Color(0f, 1f, 0f, .5f);
                ball = other.gameObject.GetComponent<Ball>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if (isWithinKickingRange)
            {
                isWithinKickingRange = false;
                kickIndicatorImage.color = new Color(1f, 1f, 1f, .5f);
                ball = null;
            }
        }
    }
}
