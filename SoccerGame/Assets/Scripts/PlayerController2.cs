using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour
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
    [Header("Physics")]
    public float defaultHeight;
    public float springStrength;
    public float springDampening;
    public float jumpForce;
    public float uprightSprintStrength;
    public float uprightSpringDampening;

    public float legSpringStrength;
    public float legSpringDampening;
    private JointLimits restrictedLegLimits;
    private JointLimits unrestrictedLegLimits;

    [Header("Kicking")]
    public Rigidbody legRigidbody_L;
    public Rigidbody legRigidbody_R;
    public HingeJoint legJoint_L;
    public HingeJoint legJoint_R;
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


    [Header("Action")]
    public Transform leg_L;
    public Transform leg_R;
    public Transform legPivot_L;
    public Transform legPivot_R;
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






    private void Awake()
    {

    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        unrestrictedLegLimits = legJoint_L.limits;
        restrictedLegLimits = legJoint_L.limits;
        restrictedLegLimits.min = -2f;
        restrictedLegLimits.max = 2f;
        legJoint_L.limits = restrictedLegLimits;
        legJoint_R.limits = restrictedLegLimits;
    }

    private void FixedUpdate()
    {

        Debug.DrawRay(groundChecker.position, -transform.up * defaultHeight, Color.red);
        RaycastHit hit;
        
        if (Physics.Raycast(groundChecker.position, -transform.up, out hit, defaultHeight, groundLayerMask))
        {
            if(!isJumping)
            {
                isGrounded = true;
            }
            
            Vector3 vel = rb.velocity;
            Vector3 rayDir = -transform.up;

            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = hit.rigidbody;
            if (hitBody != null)
            {
                otherVel = hitBody.velocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);
            float relVel = rayDirVel - otherDirVel;

            float x = hit.distance - defaultHeight;

            float springForce = (x * springStrength) - (relVel * springDampening);
            if (!isJumping)
            {
                
                rb.AddForce(rayDir * springForce);

                if (hitBody != null)
                {
                    hitBody.AddForceAtPosition(rayDir * -springForce, hit.point);
                }
            }
        }
        else
        {
            isGrounded = false;
            
        }

        if (isGrounded)
        {
            Vector3 unitVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            float velDot = Vector3.Dot(unitGoalVelocity, unitVelocity);
            float accel = acceleration * AccelerationFactorFromDot.Evaluate(velDot);
            Vector3 _goalVelocity = unitGoalVelocity * maxSpeed * speedFactor;

            goalVelocity = Vector3.MoveTowards(goalVelocity, _goalVelocity, accel * Time.fixedDeltaTime);
            Vector3 neededAccel = (goalVelocity - rb.velocity) / Time.fixedDeltaTime;

            float maxAccel = maxAccelForce * MaxAccelerationFactorFromDot.Evaluate(velDot) * maxAccelForceFactor;

            neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
            Debug.Log("Ground force: " + Vector3.Scale(neededAccel * rb.mass, forceScale));
            rb.AddForce(Vector3.Scale(neededAccel * rb.mass, forceScale));
            
        }
        //UpdateUprightForce();
        HandleKickSwing();
        //LegStiffness();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.T))
        {
            rb.AddForceAtPosition(new Vector3(0, 0, 5f), transform.position + new Vector3(0f, 60f, 0f));
        }

        
        unitGoalVelocity = new Vector3(horizontalInput, 0f, verticalInput);
        unitGoalVelocity = new Vector3(unitGoalVelocity.x, 0f, unitGoalVelocity.z).normalized;
        Debug.DrawRay(transform.position, unitGoalVelocity * 5f, Color.green);
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.black);
        //unitGoalVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        //Debug.Log("Input: (" + horizontalInput + "," + verticalInput + ")");
        if (horizontalInput != 0 || verticalInput != 0)
        {
            goalRotation = Quaternion.LookRotation(unitGoalVelocity, Vector3.up);
            Vector3 motionDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
            Quaternion toRotation = Quaternion.LookRotation(motionDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 straight = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            goalRotation = Quaternion.LookRotation(straight, Vector3.up);
            Vector3 motionDirection = new Vector3(rb.velocity.x, 0f, rb.velocity.z).normalized;
            //transform.rotation = Quaternion.LookRotation(straight, Vector3.up);
        }

        //goalRotation = Quaternion.LookRotation(unitGoalVelocity, Vector3.up);

    }

    public void UpdateUprightForce()
    {
        Quaternion characterCurrent = transform.rotation;
        Quaternion toGoal = ShortestRotation(goalRotation,characterCurrent);
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

    void LegStiffness()
    {
        if (!isDownswinging_L && !isBackswinging_L)
        {
            JointLimits limits = legJoint_L.limits;
            limits.max = 2f;
            limits.min = -2f;
            legJoint_L.limits = limits;

            //Vector3 vel = legRigidbody_L.velocity;
            //Vector3 rayDir;
            //if (legJoint_L.angle > 0)
            //{
            //    rayDir = legRigidbody_L.transform.forward;
            //}
            //else
            //{
            //    rayDir = -legRigidbody_L.transform.forward;
            //}

            //Vector3 otherVel = Vector3.zero;


            //float rayDirVel = Vector3.Dot(rayDir, vel);
            //float otherDirVel = Vector3.Dot(rayDir, otherVel);
            //float relVel = rayDirVel - otherDirVel;

            //float x = Mathf.Abs(legJoint_L.angle);

            //float springForce = (x * legSpringStrength) - (relVel * legSpringDampening);
            //legRigidbody_L.AddForce(rayDir * springForce);
        }

        if (!isDownswinging_R && !isBackswinging_R)
        {
            JointLimits limits = legJoint_R.limits;
            limits.max = 2f;
            limits.min = -2f;
            legJoint_R.limits = limits;

            //// Right Leg
            //Vector3 vel = legRigidbody_R.velocity;
            //Vector3 rayDir;
            //if (legJoint_R.angle > 0)
            //{
            //    rayDir = legRigidbody_R.transform.forward;
            //}
            //else
            //{
            //    rayDir = -legRigidbody_R.transform.forward;
            //}

            //Vector3 otherVel = Vector3.zero;


            //float rayDirVel = Vector3.Dot(rayDir, vel);
            //float otherDirVel = Vector3.Dot(rayDir, otherVel);
            //float relVel = rayDirVel - otherDirVel;

            //float x = Mathf.Abs(legJoint_R.angle);

            //float springForce = (x * legSpringStrength) - (relVel * legSpringDampening);
            //legRigidbody_R.AddForce(rayDir * springForce);
        }
    }

    void HandleKickSwing()
    {
        //Debug.Log("Current Angle: " + legJoint_L.angle + " max: " + legJoint_L.limits.max);
        if (isKicking)
        {
            
            if (isBackswinging_L)
            {
                legRigidbody_L.AddForce(-legRigidbody_L.transform.forward * backswingForce, ForceMode.Acceleration);
                
                if (legJoint_L.angle + 3f >= legJoint_L.limits.max)
                {
                    isBackswinging_L = false;
                    isDownswinging_L = true;
                }

            }
            else if (isDownswinging_L)
            {
                legRigidbody_L.AddForce(legRigidbody_L.transform.forward * downswingForce, ForceMode.Acceleration);
                if (legJoint_L.angle - 30f <= legJoint_L.limits.min)
                {
                    ResetDownswing();
                }
            }
            else if (isBackswinging_R)
            {
                legRigidbody_R.AddForce(-legRigidbody_R.transform.forward * backswingForce, ForceMode.Acceleration);

                if (legJoint_R.angle + 3f >= legJoint_R.limits.max)
                {
                    isBackswinging_R = false;
                    isDownswinging_R = true;
                }

            }
            else if (isDownswinging_R)
            {
                legRigidbody_R.AddForce(legRigidbody_R.transform.forward * downswingForce, ForceMode.Acceleration);
                if (legJoint_R.angle - 30f <= legJoint_R.limits.min)
                {
                    ResetDownswing();
                }
            }




        }
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        if (direction == null || direction == Vector2.zero)
        {
            verticalInput = 0f;
            horizontalInput = 0f;
        }
        verticalInput = direction.y;
        horizontalInput = direction.x;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            isGrounded = false;
            isJumping = true;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            StartCoroutine(JumpRoutine());
        }
    }

    public void KickBackswing_L(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            KickBackswing(true);
        }
        
    }
    public void KickBackswing_R(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            KickBackswing(false);
        }
    }
    public void KickDownswing_L(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            KickDownswing(true);
        }
    }
    public void KickDownswing_R(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            KickDownswing(false);
        }
    }

    void KickBackswing(bool isLeftFoot)
    {
        if (isKicking)
        {
            return;
        }
        if (isLeftFoot)
        {
            Debug.Log("Left foot");
            if (canKickLeft)
            {
                Debug.Log(" Left foot Backswing");
                legJoint_L.limits = unrestrictedLegLimits;
                isKicking = true;
                isBackswinging_L = true;
                canKickRight = false;
                canKickLeft = false;
            }

        }
        else
        {

            Debug.Log("Right foot");
            if (canKickRight)
            {
                Debug.Log("Right foot Backswing");
                legJoint_R.limits = unrestrictedLegLimits;
                isKicking = true;
                isBackswinging_R = true;
                canKickRight = false;
                canKickLeft = false;
            }

        }
    }


    void KickDownswing(bool isLeftFoot)
    {
        if (isLeftFoot)
        {
            if (isBackswinging_L == true)
            {
                Debug.Log("Downswing");
                isBackswinging_L = false;
                isDownswinging_L = true;
            }
        }
        else
        {
            if (isBackswinging_R == true)
            {
                Debug.Log("Downswing");
                isBackswinging_R = false;
                isDownswinging_R = true;
            }
        }


    }

    void ResetDownswing()
    {
        isKicking = false;
        if (isDownswinging_L)
        {
            canKickRight = true;
            isDownswinging_L = false;
            legRigidbody_L.AddForce(-legRigidbody_L.transform.forward * 900f, ForceMode.Acceleration);
            StartCoroutine(KickLeftRoutine());
        }
        else if (isDownswinging_R)
        {
            canKickLeft = true;
            isDownswinging_R = false;
            legRigidbody_R.AddForce(-legRigidbody_R.transform.forward * 900f, ForceMode.Acceleration);
            StartCoroutine(KickRightRoutine());
        }
        
    }

    IEnumerator KickLeftRoutine()
    {
        legRigidbody_L.GetComponentInChildren<Collider>().enabled = false;
        yield return new WaitForSeconds(kickDelay);
        legJoint_L.limits = restrictedLegLimits;
        legRigidbody_L.GetComponentInChildren<Collider>().enabled = true;
        canKickLeft = true;
    }

    IEnumerator KickRightRoutine()
    {
        legRigidbody_R.GetComponentInChildren<Collider>().enabled = false;
        yield return new WaitForSeconds(kickDelay);
        legJoint_R.limits = restrictedLegLimits;
        legRigidbody_R.GetComponentInChildren<Collider>().enabled = true;
        canKickRight = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }


    IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(1f);
        isJumping = false;
    }
}
