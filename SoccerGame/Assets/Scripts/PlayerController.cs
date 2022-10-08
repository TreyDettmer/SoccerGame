using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public float backswingForce;
    public float downswingForce;
    public float kickDelay = 1.5f;
    private bool canKickLeft = true;
    private bool canKickRight = true;
    private bool isKicking = false;
    private bool isBackswinging_L = false;
    private bool isDownswinging_L = false;
    private bool isResetting_L = false;
    private bool isBackswinging_R = false;
    private bool isDownswinging_R = false;
    private bool isResetting_R = false;

    [SerializeField]
    private Transform ballDetectorOrigin;
    [SerializeField]
    private float ballDetectorRadius;


    [Header("Action")]
    public AnimationCurve backswingAngleFactorFromTime;
    public float maxBackswingAngle;
    public float maxDownswingAngle;
    public float kickSwingSpeed = 60f;

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



    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputAsset = GetComponent<PlayerInput>().actions;
        playerActionMap = playerInputAsset.FindActionMap("Player");
        cam = transform.parent.GetComponentInChildren<Camera>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        
        Vector2 direction = playerActionMap["Movement"].ReadValue<Vector2>();
        verticalInput = direction.y;
        horizontalInput = direction.x;
        if (Input.GetKeyDown(KeyCode.T))
        {
            rb.AddForceAtPosition(new Vector3(0, 0, 5f), transform.position + new Vector3(0f, 60f, 0f));
        }


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
        if (ballDetectorOrigin != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(ballDetectorOrigin.position, ballDetectorRadius);
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
            StartKick(context);
        }
        else if (context.canceled)
        {
            EndKick(context);
        }
    }

    public void StartKick(InputAction.CallbackContext context)
    {
        Debug.Log("Starting kick");
    }

    public void EndKick(InputAction.CallbackContext context)
    {

        Collider[] ballsHit = Physics.OverlapSphere(ballDetectorOrigin.position, ballDetectorRadius);
        if (ballsHit.Length > 0)
        {
            for (int i = 0; i < ballsHit.Length; i++)
            {
                Debug.Log(ballsHit[i].name);
                if (ballsHit[i].name == "Ball")
                {
                    Ball ball = ballsHit[i].GetComponent<Ball>();
                    ball.GetComponent<Rigidbody>().AddForce(transform.forward * 10f, ForceMode.Impulse);
                }
            }

            //Debug.Log(ballsHit);
            //Debug.Log("Kicked Ball");

        }
        else
        {
            Debug.Log("Missed Ball");
        }
    
    }

}