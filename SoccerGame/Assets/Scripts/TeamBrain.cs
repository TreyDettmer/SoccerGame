using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamBrain : MonoBehaviour
{
    [SerializeField] private int teamIndex = -1;
    public int maxPlayersDefendingBall = 2;
    public List<Player> players = new List<Player>();
    public List<Player> playersDefendingBall = new List<Player>();
    public List<Player> opponentsBeingDefended = new List<Player>();
    public int numberOfPlayersDefendingBall = 0;
    public Transform offenseFormationCentralTransform;
    public List<Transform> offenseFormationTransforms;
    public List<List<Player>> playersInEachFormationSpot;
    private List<bool> offenseFormationSpotAvailability;
    private List<Vector3> offenseFormationOffsets = new List<Vector3>();
    private Ball ball;
    private float formationRightBoundOffset = 0f;
    private float formationLeftBoundOffset = 0f;
    private float formationBottomBoundOffset = 0f;
    private float formationTopBoundOffset = 0f;
    private List<FormationSpot> formationSpots = new List<FormationSpot>();

    class FormationSpot
    {
        public Transform transform;
        public bool isAvailable;

        public FormationSpot(Transform _transform, bool _isAvailable)
        {
            transform = _transform;
            isAvailable = _isAvailable;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < offenseFormationTransforms.Count; i++)
        {

            offenseFormationOffsets.Add(offenseFormationCentralTransform.position - offenseFormationTransforms[i].position);
            formationSpots.Add(new FormationSpot(offenseFormationTransforms[i], true));
        }
        for (int i = 0; i < offenseFormationTransforms.Count; i++)
        {
            // get left bound
            if (offenseFormationCentralTransform.position.x - offenseFormationTransforms[i].position.x < formationLeftBoundOffset)
            {
                formationLeftBoundOffset = offenseFormationCentralTransform.position.x - offenseFormationTransforms[i].position.x;
            }
            // get right bound
            if (offenseFormationTransforms[i].position.x - offenseFormationCentralTransform.position.x > formationRightBoundOffset)
            {
                formationRightBoundOffset = offenseFormationTransforms[i].position.x - offenseFormationCentralTransform.position.x;
            }
            // get Top bound
            if (offenseFormationTransforms[i].position.z - offenseFormationCentralTransform.position.z > formationTopBoundOffset)
            {
                formationTopBoundOffset = offenseFormationTransforms[i].position.z - offenseFormationCentralTransform.position.z;
            }
            // get bottom bound
            if (offenseFormationTransforms[i].position.z - offenseFormationCentralTransform.position.z < formationBottomBoundOffset)
            {
                formationBottomBoundOffset = offenseFormationTransforms[i].position.z - offenseFormationCentralTransform.position.z;
            }
        }
        if (teamIndex == 0)
        {
            float temp = formationBottomBoundOffset;
            formationBottomBoundOffset = formationTopBoundOffset;
            formationTopBoundOffset = temp;
        }
        
        ball = FindObjectOfType<Ball>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.teamWithBall == teamIndex)
        {
            offenseFormationCentralTransform.position = ball.transform.position;

            float xOffset = 0f;
            float zOffset = 0f;
            if (offenseFormationCentralTransform.position.x + formationRightBoundOffset > 35f)
            {
                xOffset = 35 - (offenseFormationCentralTransform.position.x + formationRightBoundOffset);
            }
            else if (offenseFormationCentralTransform.position.x + formationLeftBoundOffset < -35f)
            {
                xOffset = -35 - (offenseFormationCentralTransform.position.x + formationLeftBoundOffset);
            }

            if (offenseFormationCentralTransform.position.z + formationTopBoundOffset > 55f)
            {
                zOffset = 55 - (offenseFormationCentralTransform.position.z + formationTopBoundOffset);
            }
            else if (offenseFormationCentralTransform.position.z + formationTopBoundOffset < -55f)
            {
                zOffset = -55 - (offenseFormationCentralTransform.position.z + formationTopBoundOffset);
            }
            for (int i = 0; i < offenseFormationOffsets.Count; i++)
            {

                offenseFormationTransforms[i].position = offenseFormationCentralTransform.position - offenseFormationOffsets[i] + new Vector3(xOffset, 0f, zOffset);

            }
        }
    }

    public Transform GetAssignedFormationSpot(Player player)
    {
        
        for (int i = 0; i < formationSpots.Count; i++)
        {
            if (formationSpots[i].isAvailable)
            {
                formationSpots[i].isAvailable = false;
                return formationSpots[i].transform;
            }
        }
        return formationSpots[0].transform;
    }

    public bool IsFormationSpotTaken(Transform position)
    {
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < offenseFormationTransforms.Count; i++)
        {
            Gizmos.DrawSphere(offenseFormationTransforms[i].position, 1f);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(offenseFormationCentralTransform.position, 1f);
    }

    public void PlayerStartedDefendingBall(Player player)
    {
        playersDefendingBall.Add(player);
        numberOfPlayersDefendingBall++;
    }

    public void PlayerStoppedDefendingBall(Player player)
    {
        if (playersDefendingBall.Contains(player))
        {
            playersDefendingBall.Remove(player);
            numberOfPlayersDefendingBall--;
        }
    }

    public bool IsOpponentBeingDefended(Player opponent)
    {
        if (opponentsBeingDefended.Contains(opponent))
        {
            return true;
        }
        return false;
    }

    public bool StartedDefendingOpponent(Player opponent)
    {
        if (opponentsBeingDefended.Contains(opponent))
        {
            return false;
        }
        opponentsBeingDefended.Add(opponent);
        return true;
    }

    public void StoppedDefendingOpponent(Player opponent)
    {
        if (opponent == null)
        {
            return;
        }
        if (opponentsBeingDefended.Contains(opponent))
        {
            opponentsBeingDefended.Remove(opponent);
        }
    }
}
