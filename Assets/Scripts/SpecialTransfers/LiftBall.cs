using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftBall : MonoBehaviour { 

    [Tooltip("Zielort wo der Ball hinbewegt werden soll.")]
    public Transform toPosition;

    [Tooltip("Zeit in der sich der Ball an den Zielort bewegen soll.")]
    public float timeToReachTarget;

    [Tooltip("Wie lange soll der Ball im Bereich bleiben bis er angezogen wird.")]
    public float stayTime = 2f;

    private float startTime = 0;

    private bool transferingBall;

    private float t;
    private Vector3 startPos;
    private Vector3 targetPos;

    private Rigidbody ball;

    private CoinBooth coinBooth;

    // Distanzabweichung am Ziel damit der Ball losgelassen wird
    private const float minDistance = 0.0f;

    void Start()
    {
        targetPos = toPosition.position;
        coinBooth = GetComponent<CoinBooth>();
        if (coinBooth != null)
            coinBooth.ManualBooth = true;
    }

    void Update()
    {
        if (ball != null)
        {
            if (coinBooth != null && coinBooth.ManualBooth)
            {
                if (coinBooth.isBoothAffordable())
                    coinBooth.payManualBooth();
            }

            if (coinBooth != null && !coinBooth.BoothUsedOnce && coinBooth.WasPayed)
            {
                moveBallToDestWithBooth();
            }
            else if (coinBooth == null)
            {
                moveBallToDest();
            }
        }
    }

    private void moveBallToDestWithBooth()
    {
        moveBallToDest();
        if (isBallAtDest())
            coinBooth.usedManualBooth();
    }

    private void moveBallToDest()
    {
        if (targetPos != null && transferingBall)
        {

            if (!ball.isKinematic)
                ball.isKinematic = true;

            if (!isBallAtDest())
            {
                t += Time.deltaTime / timeToReachTarget;
                ball.position = Vector3.Lerp(startPos, targetPos, t);
            }
            else
            {
                if (ball.isKinematic)
                    ball.isKinematic = false;
                transferingBall = false;
                targetPos = toPosition.position;
                t = 0;
            }
        }
    }

    private bool isBallAtDest()
    {
        return (ball.position - targetPos).sqrMagnitude <= minDistance;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ball") && !transferingBall)
        {
            startTime += Time.deltaTime;

            if (startTime >= stayTime)
            {
                ball = other.attachedRigidbody;

                startPos = ball.position;
                float radius = ball.GetComponent<SphereCollider>().radius;

                targetPos.Set(targetPos.x, targetPos.y + radius, targetPos.z);

                transferingBall = true;
                startTime = 0;

                ball.velocity = Vector3.zero;
            }
        }
    }

}
