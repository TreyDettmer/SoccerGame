using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdSimulation : MonoBehaviour
{
    public int crowdSize = 200;
    public float spaceBetweenFans = 1f;
    public List<Transform> rowEndpoints;

    public GameObject[] fanPrefabs;
    [SerializeField]
    private List<Vector3[]> rows = new List<Vector3[]>();
    private List<Vector3> fansPositions = new List<Vector3>();
    private List<float> fansMovementSpeed = new List<float>();
    private List<float> fansMovementOffset = new List<float>();
    private List<GameObject> fans = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (rowEndpoints.Count >= 4)
        {
            for (int i = 0; i < rowEndpoints.Count - 3; i += 4)
            {
                Vector3 endpoint1 = rowEndpoints[i].position;
                Vector3 endpoint2 = rowEndpoints[i + 1].position;
                Vector3 endpoint3 = rowEndpoints[i + 2].position;
                Vector3 endpoint4 = rowEndpoints[i + 3].position;
                rows.Add(new Vector3[]{ endpoint1, endpoint2, endpoint3, endpoint4});

            }
        }
        GenerateCrowd();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < fans.Count; i++)
        {
            float translation = Mathf.Sin((Time.time * fansMovementSpeed[i]) + fansMovementOffset[i]) * 0.3f;
            fans[i].transform.position = fansPositions[i] + new Vector3(0f, translation + 1f, 0f);
        }
    }

    void GenerateCrowd()
    {
        int fansRemaining = crowdSize;
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            Vector3[] row = rows[rowIndex];
            float rowLength = GetRowLength(row);
            int numberOfFansOnThisRow = Mathf.FloorToInt(rowLength / spaceBetweenFans);
            for (int currentSection = 1; currentSection <= 4; currentSection++)
            {
                if (fansRemaining == 0)
                {
                    return;
                }
                float spaceInSection = (row[currentSection % 4] - row[(currentSection - 1) % 4]).magnitude;
                Vector3 directionOfSection = (row[currentSection % 4] - row[(currentSection - 1) % 4]).normalized;
                int numberOfFansInThisSection = Mathf.FloorToInt(spaceInSection / spaceBetweenFans);
                for (int i = 0; i < numberOfFansInThisSection; i++)
                {
                    if (fansRemaining == 0)
                    {
                        return;
                    }
                    Vector3 spawnPosition = row[(currentSection - 1) % 4] + (directionOfSection * spaceBetweenFans * i);

                    int fanPrefabIndex = Random.Range(0, fanPrefabs.Length);
                    Vector3 lookDirection = -spawnPosition;
                    lookDirection.y = 0;
                    GameObject fan = Instantiate(fanPrefabs[fanPrefabIndex], spawnPosition, Quaternion.LookRotation(lookDirection, Vector3.up),transform);
                    fansPositions.Add(spawnPosition);
                    fansMovementSpeed.Add(Random.Range(2.5f, 6f));
                    fansMovementOffset.Add(Random.Range(1f, 6f));
                    fans.Add(fan);
                    fansRemaining -= 1;
                }
            }
            if (fansRemaining == 0)
            {
                return;
            }

        }
    }

    float GetRowLength(Vector3[] row)
    {
        float length = 0f;
        length += (row[1] - row[0]).magnitude;
        length += (row[2] - row[1]).magnitude;
        length += (row[3] - row[2]).magnitude;
        length += (row[0] - row[3]).magnitude;
        return length;
    }

    float TotalLengthOfRows()
    {
        float totalLength = 0f;
        //for (int i = 1; i < rowEndpoints.Count; i++)
        //{
        //    totalLength += (rowEndpoints[i].position - rowEndpoints[i - 1].position).magnitude;
        //}
        return totalLength;
    }
}
