using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PitchGrid : MonoBehaviour
{

    public static PitchGrid instance;
    [SerializeField] Transform gridBottomLeftBound;
    [SerializeField] Transform gridUpperLeftBound;

    
    private float pitchWidth;
    private float pitchHeight;
    private float gridPointRadius;

    // grid updates per second
    [SerializeField] float updateRate = 5f;
    // how many grid cells per row and per column
    [SerializeField] float gridDensity = 10f;
    private float lastUpdateTime = 0f;
    List<Player> players;
    List<Player> team0Players;
    List<Player> team1Players;
    List<List<Vector3>> gridPoints = new List<List<Vector3>>();
    List<List<float>> gridScoresTeam0 = new List<List<float>>();
    List<List<Color>> gridColorsTeam0 = new List<List<Color>>();
    List<List<float>> gridScoresTeam1 = new List<List<float>>();
    List<List<Color>> gridColorsTeam1 = new List<List<Color>>();
    Ball ball;
    private float offsidesLineTeam0;
    private float offsidesLineTeam1;

    [Header("Debug")]
    [SerializeField] bool showTeam1 = false;
    [SerializeField] bool showGridColors = true;
    [SerializeField] bool showPointScores = true;
    [SerializeField] bool relativeColoring = true;
    // pointScore / maxColorThreshold = color of point (only matters if relativeColoring == false)
    [SerializeField] float maxColorThreshold = 100f;
    [SerializeField] Gradient gridColorGradient;
    // player whose grid we are calculating. If null then teammate check includes everyone on team
    [SerializeField] Player focusedPlayer;

    [Header("Scoring Factors")]
    // how important is it to stay onside 
    [SerializeField] float offsidesFactor = 60f;
    // how important is it for opponents to be far away 
    [SerializeField] float opponentDistanceFactor = 250f;
    // how important is it for teammates to be far away 
    [SerializeField] float teammateDistanceFactor = 60f;
    // how important is it to move up the pitch
    [SerializeField] float upfieldFactor = 0.75f;
    // how important is it to stay away from the sidelines
    [SerializeField] float infieldFactor = .25f;
    // how important is it to move towards opponent's Goal
    [SerializeField] float closeToGoalFactor = .25f;

    // how important is it that a teammate is open when choosing to pass
    [SerializeField] float passingOpenessFactor = 1f;
    // how important is it that a teammate is open when choosing to pass
    [SerializeField] float passingUpfieldFactor = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
        players = new List<Player>(FindObjectsOfType<Player>());
        ball = FindObjectOfType<Ball>();
        team0Players = new List<Player>();
        team1Players = new List<Player>();
        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            if (players[playerIndex].teamIndex == 0)
            {
                team0Players.Add(players[playerIndex]);
            }
            else
            {
                team1Players.Add(players[playerIndex]);
            }
        }
        pitchWidth = Mathf.Abs(gridBottomLeftBound.position.x) * 2f;
        pitchHeight = (gridUpperLeftBound.position - gridBottomLeftBound.position).magnitude;




        float currentYOffset = 0f;
        int gridYIndex = 0;
        gridPointRadius = 8f;// (pitchHeight / (gridDensity - 1f)) / 2f;
        Debug.Log("Grid point radius: " + gridPointRadius);
        while (currentYOffset <= pitchHeight)
        {
            float currentXOffset = 0f;
            gridPoints.Add(new List<Vector3>());
            gridScoresTeam0.Add(new List<float>());
            gridColorsTeam0.Add(new List<Color>());
            gridScoresTeam1.Add(new List<float>());
            gridColorsTeam1.Add(new List<Color>());
            while (currentXOffset <= pitchWidth)
            {
                gridPoints[gridYIndex].Add(new Vector3(gridBottomLeftBound.position.x + currentXOffset, 1f, gridBottomLeftBound.position.z + currentYOffset));
                gridScoresTeam0[gridYIndex].Add(0f);
                gridColorsTeam0[gridYIndex].Add(Color.white);
                gridScoresTeam1[gridYIndex].Add(0f);
                gridColorsTeam1[gridYIndex].Add(Color.white);
                currentXOffset += pitchWidth / (gridDensity - 1f);
            }
            gridYIndex += 1;
            currentYOffset += pitchHeight / (gridDensity - 1f);
        }


    }

    private void OnDrawGizmos()
    {
        //GUIStyle guiStyle = new GUIStyle();
        //guiStyle.fontSize = 10;
        //if (showTeam1)
        //{
        //    for (int rowIndex = 0; rowIndex < gridPoints.Count; rowIndex++)
        //    {
        //        for (int colIndex = 0; colIndex < gridPoints[rowIndex].Count; colIndex++)
        //        {
        //            if (showGridColors)
        //            {
        //                Gizmos.color = gridColorsTeam1[rowIndex][colIndex];
        //                Gizmos.DrawSphere(gridPoints[rowIndex][colIndex], 1f);
        //            }

        //            if (showPointScores)
        //            {
        //                Handles.Label(gridPoints[rowIndex][colIndex], ((int)gridScoresTeam1[rowIndex][colIndex]).ToString(), guiStyle);
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    for (int rowIndex = 0; rowIndex < gridPoints.Count; rowIndex++)
        //    {
        //        for (int colIndex = 0; colIndex < gridPoints[rowIndex].Count; colIndex++)
        //        {
        //            if (showGridColors)
        //            {
        //                Gizmos.color = gridColorsTeam0[rowIndex][colIndex];
        //                Gizmos.DrawSphere(gridPoints[rowIndex][colIndex], 1f);
        //            }

        //            if (showPointScores)
        //            {
        //                Handles.Label(gridPoints[rowIndex][colIndex], ((int)gridScoresTeam0[rowIndex][colIndex]).ToString(), guiStyle);
        //            }
        //        }
        //    }
        //}

    }

    // Update is called once per frame
    void Update()
    {

        
        if (Time.time - lastUpdateTime >= 1f/updateRate)
        {
            ResetGridScores();
            CalculateGridScores(1);
            CalculateGridScores(0);
            ColorGrid();
            lastUpdateTime = Time.time;
        }

    }

    public Player FindOptimalTeammateToPassTo(Player passer)
    {
        
        Player optimalTeammate = null;
        int passerRowIndex, passerColIndex;
        (passerRowIndex, passerColIndex) = GetGridSpaceFromPosition(passer.transform.position);

        float passerOpponentDistanceScore = CalculateDistanceFromOpponents(passerRowIndex, passerColIndex, passer.teamIndex);
        float passerUpfieldScore = CalculateDistanceFromOpponents(passerRowIndex, passerColIndex, passer.teamIndex);
        float bestOpponentDistanceScore = passerOpponentDistanceScore;
        float bestUpfieldScore = passerUpfieldScore;
        if (passer.teamIndex == 0)
        {
            // filter teammates who are not being blocked
            for (int i = 0; i < team0Players.Count; i++)
            {
                if (team0Players[i] == passer)
                {
                    continue;
                }
                int rowIndex, colIndex;
                (rowIndex, colIndex) = GetGridSpaceFromPosition(team0Players[i].transform.position);
                
                float opponentDistanceScore = CalculateDistanceFromOpponents(rowIndex, colIndex, passer.teamIndex);
                float upfieldScore = CalculateUpFieldEffect(rowIndex, colIndex, passer.teamIndex, passer.teamIndex == 0 ? offsidesLineTeam0 : offsidesLineTeam1);

                if (opponentDistanceScore * passingOpenessFactor < passerOpponentDistanceScore && upfieldScore * passingUpfieldFactor < passerUpfieldScore)
                {

                    if (opponentDistanceScore * passingOpenessFactor < bestOpponentDistanceScore && upfieldScore * passingUpfieldFactor < bestUpfieldScore)
                    {
                        bestOpponentDistanceScore = opponentDistanceScore * passingOpenessFactor;
                        bestUpfieldScore = upfieldScore * passingUpfieldFactor;
                        optimalTeammate = team0Players[i];
                    }
                        
                }
            }
            return optimalTeammate;
        }
        else
        {
            // filter teammates who are not being blocked
            for (int i = 0; i < team1Players.Count; i++)
            {
                if (team1Players[i] == passer)
                {
                    continue;
                }
                int rowIndex, colIndex;
                (rowIndex, colIndex) = GetGridSpaceFromPosition(team1Players[i].transform.position);

                float opponentDistanceScore = CalculateDistanceFromOpponents(rowIndex, colIndex, passer.teamIndex);
                float upfieldScore = CalculateUpFieldEffect(rowIndex, colIndex, passer.teamIndex, passer.teamIndex == 0 ? offsidesLineTeam0 : offsidesLineTeam1);

                if (opponentDistanceScore * passingOpenessFactor < passerOpponentDistanceScore && upfieldScore * passingUpfieldFactor < passerUpfieldScore)
                {

                    if (opponentDistanceScore * passingOpenessFactor < bestOpponentDistanceScore && upfieldScore * passingUpfieldFactor < bestUpfieldScore)
                    {
                        bestOpponentDistanceScore = opponentDistanceScore * passingOpenessFactor;
                        bestUpfieldScore = upfieldScore * passingUpfieldFactor;
                        optimalTeammate = team1Players[i];
                    }

                }
            }
            return optimalTeammate;
        }
    }

    private (int,int) GetGridSpaceFromPosition(Vector3 position)
    {
        int gridHeight = gridPoints.Count;
        int gridWidth = gridPoints[0].Count;
        float relativeZ = position.z - gridBottomLeftBound.position.z;
        float relativeX = position.x - gridBottomLeftBound.position.x;
        int row = (int)Mathf.Clamp(relativeZ / (pitchHeight / (gridDensity - 1f)), 0, gridHeight);
        int col = (int)Mathf.Clamp(relativeX / (pitchWidth / (gridDensity - 1f)), 0, gridWidth);
        return (row, col);
    }

    public Vector3 FindOptimalSpaceForPlayer(Player player)
    {
        // figure out which grid space this player is in
        for (int rowIndex = 0; rowIndex < gridPoints.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < gridPoints[rowIndex].Count; colIndex++)
            {
                if ((player.transform.position - gridPoints[rowIndex][colIndex]).magnitude <= gridPointRadius)
                {
                    int bestRowIndex = rowIndex;
                    int bestColIndex = colIndex;
                    float bestScore = player.teamIndex == 0 ? gridScoresTeam0[rowIndex][colIndex] : gridScoresTeam1[rowIndex][colIndex];
                    bestScore -= CalculateDistanceFromTeammates(rowIndex, colIndex, player.teamIndex, player);

                    // iterate over surounding grid spaces to find optimal
                    for (int adjacentRowIndex = rowIndex - 1; adjacentRowIndex <= rowIndex + 1; adjacentRowIndex++ )
                    {
                        // check if we are in a valid row
                        if (gridColorsTeam0.Count < adjacentRowIndex || adjacentRowIndex == -1)
                        {
                            continue;
                        }
                        for (int adjacentColIndex = colIndex - 1; adjacentColIndex <= colIndex + 1; adjacentColIndex++)
                        {
                            // check if we are in a valid column
                            if (gridScoresTeam1[adjacentRowIndex].Count < adjacentColIndex || adjacentColIndex == -1)
                            {
                                continue;
                            }

                            float pointScore;
                            if (player.teamIndex == 0)
                            {
                                pointScore = gridScoresTeam0[adjacentRowIndex][adjacentColIndex] - CalculateDistanceFromTeammates(adjacentRowIndex, adjacentColIndex, player.teamIndex, player);

                            }
                            else
                            {
                                pointScore = gridScoresTeam1[adjacentRowIndex][adjacentColIndex] - CalculateDistanceFromTeammates(adjacentRowIndex, adjacentColIndex, player.teamIndex, player);
                            }
                            if (pointScore > bestScore)
                            {
                                bestScore = pointScore;
                                bestRowIndex = adjacentRowIndex;
                                bestColIndex = adjacentColIndex;
                            }

                        }
                    }
                    return gridPoints[bestRowIndex][bestColIndex];
                }
            }
        }
        return player.transform.position;
    }

    void CalculateGridScores(int teamIndex)
    {

        float offsidesLine = CalculateOffsidesLine(teamIndex);
        if (teamIndex == 1)
        {
            offsidesLineTeam1 = offsidesLine;
        }
        else
        {
            offsidesLineTeam0 = offsidesLine;
        }
        for (int rowIndex = 0; rowIndex < gridPoints.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < gridPoints[rowIndex].Count; colIndex++)
            {

                float pointScore = 0f;
                pointScore -= CalculateOffsidesEffect(rowIndex, colIndex, teamIndex, offsidesLine);
                pointScore -= CalculateUpFieldEffect(rowIndex, colIndex, teamIndex,offsidesLine);
                pointScore -= CalculateInFieldEffect(rowIndex, colIndex);
                pointScore -= CalculateDistanceFromOpponents(rowIndex, colIndex, teamIndex);
                if (focusedPlayer)
                {
                    pointScore -= CalculateDistanceFromTeammates(rowIndex, colIndex, teamIndex,focusedPlayer);
                }
                else
                {
                    pointScore -= CalculateDistanceFromTeammates(rowIndex, colIndex, teamIndex);
                }
                


                if (teamIndex == 0)
                {
                    gridScoresTeam0[rowIndex][colIndex] += pointScore;
                }
                else
                {
                    gridScoresTeam1[rowIndex][colIndex] += pointScore;
                }
            }
        }



    }

    void ResetGridScores()
    {
        for (int rowIndex = 0; rowIndex < gridScoresTeam0.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < gridScoresTeam0[rowIndex].Count; colIndex++)
            {
                gridScoresTeam0[rowIndex][colIndex] = 0f;
            }
        }
        for (int rowIndex = 0; rowIndex < gridScoresTeam1.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < gridScoresTeam1[rowIndex].Count; colIndex++)
            {
                gridScoresTeam1[rowIndex][colIndex] = 0f;
            }
        }
    }

    float CalculateOffsidesLine(int teamIndex)
    {
        float opponentBackLine = 0f;
        if (teamIndex == 0)
        {

            for (int opponentPlayerIndex = 0; opponentPlayerIndex < team1Players.Count; opponentPlayerIndex++)
            {
                if (team1Players[opponentPlayerIndex].transform.position.z > opponentBackLine)
                {
                    opponentBackLine = team1Players[opponentPlayerIndex].transform.position.z;
                }
            }
            if (opponentBackLine < 0f)
            {
                opponentBackLine = 0f;
            }
            if (ball.transform.position.z > opponentBackLine)
            {
                opponentBackLine = ball.transform.position.z;
            }
        }
        else
        {
            for (int opponentPlayerIndex = 0; opponentPlayerIndex < team0Players.Count; opponentPlayerIndex++)
            {
                if (team0Players[opponentPlayerIndex].transform.position.z < opponentBackLine)
                {
                    opponentBackLine = team0Players[opponentPlayerIndex].transform.position.z;
                }
            }
            if (opponentBackLine > 0f)
            {
                opponentBackLine = 0f;
            }
            if (ball.transform.position.z < opponentBackLine)
            {
                opponentBackLine = ball.transform.position.z;
            }
        }
        if (teamIndex == 0)
        {
            Debug.DrawLine(new Vector3(gridBottomLeftBound.position.x, 1f, offsidesLineTeam0), new Vector3(Mathf.Abs(gridBottomLeftBound.position.x), 1f, offsidesLineTeam0), Color.blue);
        }
        else
        {
            Debug.DrawLine(new Vector3(gridBottomLeftBound.position.x, 1f, offsidesLineTeam1), new Vector3(Mathf.Abs(gridBottomLeftBound.position.x), 1f, offsidesLineTeam1), Color.red);
        }
        return opponentBackLine;
    }

    float CalculateOffsidesEffect(int rowIndex, int colIndex, int teamIndex, float offsidesLine)
    {
        float pointScore = 0f;
        if (teamIndex == 0)
        {
            // check if grid point is in an offsides position
            if (gridPoints[rowIndex][colIndex].z > offsidesLine)
            {
                pointScore += offsidesFactor;
            }
        }
        else
        {
            // check if grid point is in an offsides position
            if (gridPoints[rowIndex][colIndex].z < offsidesLine)
            {
                pointScore += offsidesFactor;
            }
        }

        return pointScore;
    }


    float CalculateDistanceFromOpponents(int rowIndex, int colIndex, int teamIndex)
    {
        float pointScore = 0;
        if (teamIndex == 0)
        {
            for (int opponentIndex = 0; opponentIndex < team1Players.Count; opponentIndex++)
            {
                float sqrMagnitude = (team1Players[opponentIndex].transform.position - gridPoints[rowIndex][colIndex]).sqrMagnitude;
                pointScore += opponentDistanceFactor / sqrMagnitude;
            }
        }
        else
        {
            for (int opponentIndex = 0; opponentIndex < team0Players.Count; opponentIndex++)
            {
                float sqrMagnitude = (team0Players[opponentIndex].transform.position - gridPoints[rowIndex][colIndex]).sqrMagnitude;
                pointScore += opponentDistanceFactor / sqrMagnitude;
            }
        }
        return pointScore;
    }

    float CalculateDistanceFromTeammates(int rowIndex, int colIndex, int teamIndex, Player player)
    {
        float pointScore = 0;
        if (teamIndex == 0)
        {
            for (int teammateIndex = 0; teammateIndex < team0Players.Count; teammateIndex++)
            {
                if (team0Players[teammateIndex] == player)
                {
                    continue;
                }
                float sqrMagnitude = (team0Players[teammateIndex].transform.position - gridPoints[rowIndex][colIndex]).sqrMagnitude;
                pointScore += teammateDistanceFactor / sqrMagnitude;
            }
        }
        else
        {
            for (int teammateIndex = 0; teammateIndex < team1Players.Count; teammateIndex++)
            {
                if (team1Players[teammateIndex] == player)
                {
                    continue;
                }
                float sqrMagnitude = (team1Players[teammateIndex].transform.position - gridPoints[rowIndex][colIndex]).sqrMagnitude;
                pointScore += teammateDistanceFactor / sqrMagnitude;
            }
        }
        return pointScore;
    }

    float CalculateDistanceFromTeammates(int rowIndex, int colIndex, int teamIndex)
    {
        float pointScore = 0;
        if (teamIndex == 0)
        {
            for (int teammateIndex = 0; teammateIndex < team0Players.Count; teammateIndex++)
            {
                float sqrMagnitude = (team0Players[teammateIndex].transform.position - gridPoints[rowIndex][colIndex]).sqrMagnitude;
                pointScore += teammateDistanceFactor / sqrMagnitude;
            }
        }
        else
        {
            for (int teammateIndex = 0; teammateIndex < team1Players.Count; teammateIndex++)
            {
                float sqrMagnitude = (team1Players[teammateIndex].transform.position - gridPoints[rowIndex][colIndex]).sqrMagnitude;
                pointScore += teammateDistanceFactor / sqrMagnitude;
            }
        }
        return pointScore;
    }

    float CalculateUpFieldEffect(int rowIndex, int colIndex, int teamIndex, float offsidesLine)
    {
        if (teamIndex == 0)
        {
            if (gridPoints[rowIndex][colIndex].z > offsidesLine)
            {
                // since we are offsides, favor moving backfield
                return Mathf.Abs(gridBottomLeftBound.position.z - gridPoints[rowIndex][colIndex].z) * upfieldFactor;
            }
            return Mathf.Abs(gridUpperLeftBound.position.z - gridPoints[rowIndex][colIndex].z) * upfieldFactor;
        }
        else
        {
            if (gridPoints[rowIndex][colIndex].z < offsidesLine)
            {
                // since we are offsides, favor moving backfield
                return Mathf.Abs(gridUpperLeftBound.position.z - gridPoints[rowIndex][colIndex].z) * upfieldFactor;
            }
            return Mathf.Abs(gridBottomLeftBound.position.z - gridPoints[rowIndex][colIndex].z) * upfieldFactor;
        }
    }

    float CalculateInFieldEffect(int rowIndex, int colIndex)
    {
        return Mathf.Max(Mathf.Abs(gridBottomLeftBound.position.x - gridPoints[rowIndex][colIndex].x), Mathf.Abs(Mathf.Abs(gridBottomLeftBound.position.x) - gridPoints[rowIndex][colIndex].x)) * infieldFactor;
    }

    void ColorGrid()
    {
        float bestTeam0Score = -1000f;
        float worstTeam0Score = 1000f;
        float bestTeam1Score = -1000f;
        float worstTeam1Score = 1000f;
        // find the best and worst scoring points
        for (int rowIndex = 0; rowIndex < gridColorsTeam0.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < gridColorsTeam0[rowIndex].Count; colIndex++)
            {
                if (gridScoresTeam0[rowIndex][colIndex] > bestTeam0Score)
                {
                    bestTeam0Score = gridScoresTeam0[rowIndex][colIndex];
                }
                if (gridScoresTeam0[rowIndex][colIndex] < worstTeam0Score)
                {
                    worstTeam0Score = gridScoresTeam0[rowIndex][colIndex];
                }

                if (gridScoresTeam1[rowIndex][colIndex] > bestTeam1Score)
                {
                    bestTeam1Score = gridScoresTeam1[rowIndex][colIndex];
                }
                if (gridScoresTeam1[rowIndex][colIndex] < worstTeam1Score)
                {
                    worstTeam1Score = gridScoresTeam1[rowIndex][colIndex];
                }
            }
        }

        // make things positive for lerping
        bestTeam0Score += Mathf.Abs(worstTeam0Score);
        bestTeam1Score += Mathf.Abs(worstTeam1Score);

        for (int rowIndex = 0; rowIndex < gridColorsTeam0.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < gridColorsTeam0[rowIndex].Count; colIndex++)
            {
                if (relativeColoring)
                {
                    float percent = (gridScoresTeam0[rowIndex][colIndex] + Mathf.Abs(worstTeam0Score)) / bestTeam0Score;
                    gridColorsTeam0[rowIndex][colIndex] = gridColorGradient.Evaluate(percent);
                }
                else
                {
                    gridColorsTeam0[rowIndex][colIndex] = gridColorGradient.Evaluate(1f - Mathf.Clamp01(gridScoresTeam0[rowIndex][colIndex] / -maxColorThreshold));
                }

            }
        }
        for (int rowIndex = 0; rowIndex < gridColorsTeam1.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < gridColorsTeam1[rowIndex].Count; colIndex++)
            {
                if (relativeColoring)
                {
                    float percent = (gridScoresTeam1[rowIndex][colIndex] + Mathf.Abs(worstTeam1Score)) / bestTeam1Score;
                    gridColorsTeam1[rowIndex][colIndex] = gridColorGradient.Evaluate(percent);
                }
                else
                {
                    gridColorsTeam1[rowIndex][colIndex] = gridColorGradient.Evaluate(1f - Mathf.Clamp01(gridScoresTeam1[rowIndex][colIndex] / -maxColorThreshold));

                }
            }
        }
    }



}
