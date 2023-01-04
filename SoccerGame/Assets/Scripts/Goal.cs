using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public int teamIndex = 0;
    public bool isEnabled = true;
    protected Ball ball;
    protected AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        ball = FindObjectOfType<Ball>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnabled && other.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            GameplayManager.instance.GoalScored(teamIndex);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {


            if (ball.owner == null)
            {
                AudioManager.instance.Play("Crossbar");
            }
            else if (ball.owner != this)
            {
                ball.owner.BallStolen();
            }




        }
    }
}
