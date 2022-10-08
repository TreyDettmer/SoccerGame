using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public Transform player;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    private Vector3 velocity;
    public bool lookInOppositeDirection = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        
        Vector3 modifiedOffset = offset;
        float dot = Vector3.Dot(new Vector3(player.forward.x, 0f, player.forward.z), new Vector3(transform.forward.x, 0f, transform.forward.z));
        if (dot < -.4f)
        {
        }
        if (lookInOppositeDirection)
        {
            modifiedOffset.z *= -1;
        }
        Vector3 desiredPosition = player.position + modifiedOffset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
