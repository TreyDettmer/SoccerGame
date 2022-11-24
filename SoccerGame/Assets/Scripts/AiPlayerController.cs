using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayerController : Player
{

    public enum AIState
    {
        Penalized,
        Waiting,
        Playing
    }

    protected override void Start()
    {
        base.Start();
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
                            if (canDribble)
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

    }
}
