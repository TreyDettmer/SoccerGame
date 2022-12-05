using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayerController : Player
{
    [SerializeField]
    private AIState aiState = AIState.Idling;

    public float detectionRadius = 20f;

    public float shotPlacementMaxXOffset = 4f;
    public float shotPlacementMaxYHeight = 2f;
    public float shotPlacementStep = 3f;
    // the radius of the spheres used during the shot path spherecasts
    public float shotClearanceNeeded = .5f;
    private Vector3 currentShotPlacement = Vector3.zero;
    private bool isLiningUpShot = false;

    private Vector3 desiredPosition = Vector3.zero;
    private Vector3 myGoalsPosition = Vector3.zero;
    private Vector3 opponentsGoalsPosition = Vector3.zero;
    private Player opponentImDefending = null;
    private List<Vector3> shotPlacements = new List<Vector3>();
    private LayerMask playerLayerMask;

    public enum AIState
    {
        Idling,
        Dribbling,
        Shooting,
        Attacking,
        Defending
    }

    protected override void Start()
    {
        base.Start();
        myGoalsPosition = GameplayManager.instance.GetGoalPositionForTeam(teamIndex);
        opponentsGoalsPosition = teamIndex == 0 ? GameplayManager.instance.GetGoalPositionForTeam(1) : GameplayManager.instance.GetGoalPositionForTeam(0);
        playerLayerMask = LayerMask.NameToLayer("Player");
        float currentYOffset = 1f;
        while (currentYOffset <= (shotPlacementMaxYHeight + 1f) - ((shotPlacementMaxYHeight) / (shotPlacementStep + 1f)))
        {
            currentYOffset += (shotPlacementMaxYHeight) / (shotPlacementStep + 1f);
            float currentXOffset = -shotPlacementMaxXOffset + (2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f);
            while (currentXOffset <= shotPlacementMaxXOffset - ((2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f)))
            {
                
                shotPlacements.Add(opponentsGoalsPosition + new Vector3(currentXOffset, currentYOffset, 0f));
                currentXOffset += (2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f);

            }
            
        }
        

    }
    protected override void FixedUpdate()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        

        CalculateMovementInput();

        base.FixedUpdate();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;
        for (int i = 0; i < shotPlacements.Count;i++)
        {
            Gizmos.DrawWireSphere(shotPlacements[i], shotClearanceNeeded);
        }
    }

    protected Vector3 GetDirectionOfMovement(float _horizontalInput, float _verticalInput)
    {
        Vector3 forward = transform.parent.forward;
        Vector3 right = transform.parent.right;
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;
        Vector3 forwardRelativeVerticalInput = _verticalInput * forward;
        Vector3 rightRelativeHorizontalInput = _horizontalInput * right;


        return forwardRelativeVerticalInput + rightRelativeHorizontalInput;


    }
    protected void GetInput()
    {

        if (isKicking)
        {
            unitGoalVelocity = GetDirectionOfMovement(0f, verticalInput).normalized;
        }
        else
        {
            unitGoalVelocity = GetDirectionOfMovement(horizontalInput, verticalInput).normalized;
        }



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
                            if (CanDribble)
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
            //cameraPlayerGui.UpdatePowerMeter(kickBackswingElapsedTime / maxKickBackswingTime);
            if (kickBackswingElapsedTime >= maxKickBackswingTime)
            {
                EndKick();
            }
        }

        if (teamWithBall == teamIndex)
        {

        }
        else if (teamWithBall == -1)
        {
            // move towards ball
        }
        else
        {
            // move to defending position

        }

        


    }

    private void UpdateAiState(AIState _aiState)
    {
        if (aiState == _aiState)
        {
            // ignore since we're already in this state
            return;
        }
        if (_aiState == AIState.Dribbling)
        {
            aiState = AIState.Dribbling;
        }
        else if (_aiState == AIState.Defending)
        {
            aiState = AIState.Defending;
        }
        else if (_aiState == AIState.Shooting)
        {
            aiState = AIState.Shooting;
        }
        else if (_aiState == AIState.Idling)
        {
            aiState = AIState.Idling;
        }
        else if (_aiState == AIState.Attacking)
        {
            aiState = AIState.Attacking;
        }    
    }

    private void CalculateMovementInput()
    {
        Vector3 desiredPosition = CalculateDesiredPosition();
        Vector3 position = transform.position;
        position.y = 0;
        desiredPosition.y = 0;
        Vector3 relativeDirection = transform.parent.InverseTransformDirection(position - desiredPosition);
        Debug.DrawRay(desiredPosition, Vector3.up * 5f, Color.black);
        Debug.DrawLine(transform.position, desiredPosition, Color.magenta);
        Debug.DrawLine(ball.transform.position, myGoalsPosition, Color.blue);

        horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 5f) * -Mathf.Sign(relativeDirection.x);
        verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 5f) * -Mathf.Sign(relativeDirection.z);
    }

    private void CheckForOpponentToDefend()
    {
        if (opponents.Count > 0)
        {
            float nearestOpponentSquareDistance = 1000f;
            int nearestOpponentIndex = 0;
            for (int opponentIndex = 0; opponentIndex < opponents.Count; opponentIndex++)
            {
                float squareDistance = (transform.position - opponents[opponentIndex].transform.position).sqrMagnitude;
                if (squareDistance < nearestOpponentSquareDistance)
                {
                    nearestOpponentSquareDistance = squareDistance;
                    nearestOpponentIndex = opponentIndex;
                }
            }

            if (nearestOpponentSquareDistance <= detectionRadius * detectionRadius)
            {
                if ((transform.position - ball.transform.position).sqrMagnitude < nearestOpponentSquareDistance)
                {
                    opponentImDefending = null;
                }
                else
                {
                    opponentImDefending = opponents[nearestOpponentIndex];
                }
                
            }
            else
            {
                opponentImDefending = null;
            }
        }
    }

    // check if we have an open look at goal. Return a shot location if one exists
    private Vector3 CheckForShotPath()
    {

        int shotPlacementIndex = Random.Range(0, shotPlacements.Count - 1);
        for (int i = 0; i < shotPlacements.Count; i++)
        {
            if (!Physics.SphereCast(transform.position, shotClearanceNeeded, (shotPlacements[shotPlacementIndex] - transform.position).normalized, out _, (shotPlacements[i] - transform.position).magnitude, playerLayerMask))
            {
                return shotPlacements[shotPlacementIndex];
            }
            shotPlacementIndex += 1;
            shotPlacementIndex = shotPlacementIndex % shotPlacements.Count;
        }
        // all shot paths are blocked so don't shoot
        return Vector3.zero;
    }

    // determines how long to hold the shoot button down
    private float CalculateShotPower()
    {
        float squareDistance = (currentShotPlacement - transform.position).sqrMagnitude;
        if (squareDistance < 250f)
        {
            return .3f;
        }
        else if (squareDistance < 810f)
        {
            return .4f;
        }
        else
        {
            return .5f;
        }
    }

    // simulates holding down the kick button
    private IEnumerator ShotPowerRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndKick();
    }


    // Gives the player time to rotate towards where they want to shoot it
    private IEnumerator LineUpShotRoutine(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        StartKick();
        StartCoroutine(ShotPowerRoutine(CalculateShotPower()));
        isLiningUpShot = false;
    }
        

    private Vector3 CalculateDesiredPosition()
    {

        if (aiState == AIState.Dribbling)
        {
            if ((opponentsGoalsPosition - transform.position).sqrMagnitude < 1200)
            {
                if (!isKicking && !isLiningUpShot)
                {
                    Vector3 potentialShotPlacement = CheckForShotPath();
                    if (potentialShotPlacement != Vector3.zero)
                    {
                        currentShotPlacement = potentialShotPlacement;
                        isLiningUpShot = true;
                        StartCoroutine(LineUpShotRoutine(1f));
                        return currentShotPlacement;
                    }
                }
                if (currentShotPlacement != Vector3.zero)
                {
                    return currentShotPlacement;
                }
            }
            return opponentsGoalsPosition;
        }
        else if (aiState == AIState.Defending)
        {
            CheckForOpponentToDefend();
            if (opponentImDefending != null)
            {
                if (ball.owner == opponentImDefending)
                {
                    // point between my opponent and our goal
                    Vector3 nearestPoint = FindNearestPointOnLine(opponentImDefending.transform.position, myGoalsPosition, transform.position);
                    if ((transform.position - nearestPoint).sqrMagnitude < 10f)
                    {
                        // if we are between the oppponent and our goal, move towards the opponent
                        return ball.transform.position;
                    }
                    else
                    {
                        return nearestPoint;
                    }

                }
                else
                {
                    // move to center point of triangle created by ball, goal, and opponent
                    return FindCentroid(ball.transform.position, myGoalsPosition, opponentImDefending.transform.position);
                }
            }
            return FindNearestPointOnLine(ball.transform.position, myGoalsPosition, transform.position);
        }
        else if (aiState == AIState.Shooting)
        {
            return opponentsGoalsPosition;
        }
        else if (aiState == AIState.Idling)
        {
            
            if (teamWithBall == -1)
            {
                return ball.transform.position;
            }
            return transform.position;
        }
        else if (aiState == AIState.Attacking)
        {
            return new Vector3(transform.position.x,0f, opponentsGoalsPosition.z);
        }

        return transform.position;



    }

    public Vector3 FindCentroid(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float x = (p1.x + p2.x + p3.x) / 3f;
        float z = (p1.z + p2.z + p3.z) / 3f;
        return new Vector3(x, 0f, z);
    }

    public Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 end, Vector3 point)
    {
        // ignore y difference
        origin.y = 0;
        end.y = 0;
        point.y = 0;

        Vector3 lineDir = (end - origin).normalized;
        Vector3 dirToPoint = (point - origin).normalized;
        float dot = Vector3.Dot(lineDir, dirToPoint);
        if (dot < 0)
        {
            return origin;
        }
        //Debug.Log("Dot: " + dot);
        float length = (origin - point).magnitude;
        float dotLength = length * dot;
        return origin + lineDir * dotLength;
    }

    public override void BallEvent(Player playerWithBall)
    {
        base.BallEvent(playerWithBall);
        if (teamWithBall == -1)
        {
            UpdateAiState(AIState.Idling);
        }
        else if (teamWithBall == teamIndex)
        {
            if (playerWithBall == this)
            {
                UpdateAiState(AIState.Dribbling);
            }
            else
            {
                UpdateAiState(AIState.Attacking);
            }
        }
        else
        {
            UpdateAiState(AIState.Defending);
        }
    }

    public override void EndKick()
    {
        base.EndKick();
        currentShotPlacement = Vector3.zero;

    }

}
