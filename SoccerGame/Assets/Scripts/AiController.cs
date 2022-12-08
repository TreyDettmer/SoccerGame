using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : MonoBehaviour
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

    private Player opponentImDefending = null;
    private List<Vector3> shotPlacements = new List<Vector3>();



    public enum AIState
    {
        Idling,
        Dribbling,
        Shooting,
        Attacking,
        Defending
    }


    Player _;

    private void Awake()
    {
        _ = GetComponent<Player>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnGameplayStart()
    {
        float currentYOffset = 1f;
        while (currentYOffset <= (shotPlacementMaxYHeight + 1f) - ((shotPlacementMaxYHeight) / (shotPlacementStep + 1f)))
        {
            currentYOffset += (shotPlacementMaxYHeight) / (shotPlacementStep + 1f);
            float currentXOffset = -shotPlacementMaxXOffset + (2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f);
            while (currentXOffset <= shotPlacementMaxXOffset - ((2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f)))
            {

                shotPlacements.Add(_.opponentsGoalsPosition + new Vector3(currentXOffset, currentYOffset, 0f));
                currentXOffset += (2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f);

            }

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        _.horizontalInput = 0f;
        _.verticalInput = 0f;
        GetInput();
    }


    private void GetInput()
    {
        Vector3 desiredPosition = CalculateDesiredPosition();
        Vector3 position = transform.position;
        position.y = 0;
        desiredPosition.y = 0;
        Vector3 relativeDirection = transform.parent.InverseTransformDirection(position - desiredPosition);
        Debug.DrawRay(desiredPosition, Vector3.up * 5f, Color.black);
        Debug.DrawLine(transform.position, desiredPosition, Color.magenta);
        Debug.DrawLine(_.ball.transform.position, _.myGoalsPosition, Color.blue);

        _.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 5f) * -Mathf.Sign(relativeDirection.x);
        _.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 5f) * -Mathf.Sign(relativeDirection.z);

    }



    private Vector3 CalculateDesiredPosition()
    {

        if (aiState == AIState.Dribbling)
        {
            if ((_.opponentsGoalsPosition - transform.position).sqrMagnitude < 1200)
            {
                if (!_.isKicking && !isLiningUpShot)
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
            return _.opponentsGoalsPosition;
        }
        else if (aiState == AIState.Defending)
        {
            CheckForOpponentToDefend();
            if (opponentImDefending != null)
            {
                if (_.ball.owner == opponentImDefending)
                {
                    // point between my opponent and our goal
                    Vector3 nearestPoint = FindNearestPointOnLine(opponentImDefending.transform.position, _.myGoalsPosition, transform.position);
                    if ((transform.position - nearestPoint).sqrMagnitude < 10f)
                    {
                        // if we are between the oppponent and our goal, move towards the opponent
                        return _.ball.transform.position;
                    }
                    else
                    {
                        return nearestPoint;
                    }

                }
                else
                {
                    // move to center point of triangle created by ball, goal, and opponent
                    return FindCentroid(_.ball.transform.position, _.myGoalsPosition, opponentImDefending.transform.position);
                }
            }
            return FindNearestPointOnLine(_.ball.transform.position, _.myGoalsPosition, transform.position);
        }
        else if (aiState == AIState.Shooting)
        {
            return _.opponentsGoalsPosition;
        }
        else if (aiState == AIState.Idling)
        {

            if (Player.teamWithBall == -1)
            {
                return _.ball.transform.position;
            }
            return transform.position;
        }
        else if (aiState == AIState.Attacking)
        {
            return new Vector3(transform.position.x, 0f, _.opponentsGoalsPosition.z);
        }

        return transform.position;

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

    private void CheckForOpponentToDefend()
    {
        if (_.opponents.Count > 0)
        {
            float nearestOpponentSquareDistance = 1000f;
            int nearestOpponentIndex = 0;
            for (int opponentIndex = 0; opponentIndex < _.opponents.Count; opponentIndex++)
            {
                float squareDistance = (transform.position - _.opponents[opponentIndex].transform.position).sqrMagnitude;
                if (squareDistance < nearestOpponentSquareDistance)
                {
                    nearestOpponentSquareDistance = squareDistance;
                    nearestOpponentIndex = opponentIndex;
                }
            }

            if (nearestOpponentSquareDistance <= detectionRadius * detectionRadius)
            {
                if ((transform.position - _.ball.transform.position).sqrMagnitude < nearestOpponentSquareDistance)
                {
                    opponentImDefending = null;
                }
                else
                {
                    opponentImDefending = _.opponents[nearestOpponentIndex];
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
        RaycastHit hit;
        for (int i = 0; i < shotPlacements.Count; i++)
        {
            if (!Physics.SphereCast(transform.position, shotClearanceNeeded, (shotPlacements[shotPlacementIndex] - transform.position).normalized, out hit, (shotPlacements[i] - transform.position).magnitude, _.playerLayerMask))
            {
                return shotPlacements[shotPlacementIndex];
            }
            shotPlacementIndex += 1;
            shotPlacementIndex = shotPlacementIndex % shotPlacements.Count;
        }
        // all shot paths are blocked so don't shoot
        return Vector3.zero;
    }

    // Gives the player time to rotate towards where they want to shoot it
    private IEnumerator LineUpShotRoutine(float delay)
    {

        yield return new WaitForSeconds(delay);
        _.StartKick();
        StartCoroutine(ShotPowerRoutine(CalculateShotPower()));
        isLiningUpShot = false;
    }

    public Vector3 FindCentroid(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float x = (p1.x + p2.x + p3.x) / 3f;
        float z = (p1.z + p2.z + p3.z) / 3f;
        return new Vector3(x, 0f, z);
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
        _.EndKick();
    }

}
