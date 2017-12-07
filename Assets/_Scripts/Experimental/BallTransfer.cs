using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTransfer : MonoBehaviour {


    [Header("Eingangspunkte")]
    public Transform liftPoint;
    public Transform startPoint;

    [Header("Zielpunkte")]
    public Transform endPoint;
    public Transform dropPoint;

    [Header("Eigenschaften")]
    [Tooltip("Zeit in der sich der Ball an den Zielort bewegen soll.")]
    public float timeToReachTarget = 2;

    [Tooltip("Wie lange soll der Ball im Bereich bleiben bis er angezogen wird.")]
    public float stayTime = 2;
    
    private float startTime = 0;

    private bool transferingBall;

    private float t;
    private Vector3 startPos;
    private Vector3 targetPos;

    private Rigidbody ball;

    // Distanzabweichung am Ziel damit der Ball losgelassen wird
    private const float minDistance = 0.0f;

    private BallInZoneCheck liftPointCheck;

    private bool liftBall = false;
    private bool transferBall = false;
    private bool idleBall = false;
    private bool dropBall = false;

    // Use this for initialization
    void Start () {
        targetPos = startPoint.position;
        liftPointCheck = liftPoint.GetComponent<BallInZoneCheck>();
    }
	
	// Update is called once per frame
	void Update () {

        //liftPointCheck.isBallInZone;

        if (targetPos != null && transferingBall)
        {
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
                targetPos = startPoint.position;
                t = 0;
            }
        }
    }

    private bool isBallAtDest()
    {
        return (ball.position - targetPos).sqrMagnitude <= minDistance;
    }

    //private void liftBall()
    //{
    //    if (other.CompareTag("Ball") && !transferingBall)
    //    {
    //        startTime += Time.deltaTime;

    //        if (startTime >= stayTime)
    //        {
    //            ball = other.attachedRigidbody;

    //            startPos = ball.position;
    //            float radius = ball.GetComponent<SphereCollider>().radius;

    //            targetPos.Set(targetPos.x, targetPos.y + radius, targetPos.z);

    //            transferingBall = true;
    //            startTime = 0;

    //            ball.velocity = Vector3.zero;
    //            ball.isKinematic = true;
    //        }
    //    }
    //}

}
