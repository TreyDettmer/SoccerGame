using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MinimapController : MonoBehaviour
{

    Transform ballTransform;

    [SerializeField]
    private Transform ballCanvas;
    [SerializeField]
    private float maxBallCanvasScale = 2.5f;

    public static MinimapController instance;

    [SerializeField]
    private GameObject playerCanvasPrefab;

    private List<Transform> playerControllerTransforms = new List<Transform>();
    private List<Transform> playerCanvasTransforms = new List<Transform>();

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
        ballTransform = FindObjectOfType<Ball>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        ballCanvas.position = new Vector3(ballTransform.position.x, ballCanvas.position.y, ballTransform.position.z);
        float ballCanvasScale = Mathf.Lerp(1f, maxBallCanvasScale, ballTransform.position.y / 20f);
        ballCanvas.localScale = new Vector3(ballCanvasScale, ballCanvasScale, 1f);

        for (int i = 0; i < playerCanvasTransforms.Count; i++)
        {
            playerCanvasTransforms[i].position = new Vector3(playerControllerTransforms[i].position.x, playerCanvasTransforms[i].position.y, playerControllerTransforms[i].position.z);
        }
    }

    public void CreatePlayerCanvasPrefab(PlayerController playerController, Color color)
    {
        playerControllerTransforms.Add(playerController.transform);
        GameObject instancedPlayerCanvas = Instantiate(playerCanvasPrefab, new Vector3(playerController.transform.position.x, 60f, playerController.transform.position.z), ballCanvas.rotation,transform);
        playerCanvasTransforms.Add(instancedPlayerCanvas.transform);
        instancedPlayerCanvas.GetComponentInChildren<RawImage>().color = color;
    }
}
