using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferBallToTarget : MonoBehaviour {

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

    // Distanzabweichung am Ziel damit der Ball losgelassen wird
    private const float minDistance = 0.0f;

    void Start()
    {
        targetPos = toPosition.position;
    }

    void Update()
    {
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
                ball.isKinematic = true;
            }
        }
    }
}


 //   public Transform toPosition;

 //   private Vector3 startPos;
 //   private Vector3 targetPos;

 //   private float startTime;
 //   private float stayTime = 2f;
 //   private float timeToReachTarget = 2f;
 //   private float t;
 //   private bool ballReadyToTransfer = false;


 //   private Rigidbody ball;

	//// Use this for initialization
	//void Start () {
 //       targetPos = toPosition.position;
	//}
	
	//// Update is called once per frame
	//void Update () {
 //       if (targetPos != null && ballReadyToTransfer)
 //       {
 //           transferBall();
 //       } 
 //   }

 //   private void transferBall()
 //   {
 //       if (!isBallAtDest())
 //       {
 //           t += Time.deltaTime / timeToReachTarget;
 //           ball.position = Vector3.Lerp(startPos, targetPos, t);
 //       } else
 //       {
 //           ballReadyToTransfer = false;
 //       }
 //   }

 //   // Distanzabweichung am Ziel damit der Ball losgelassen wird
 //   private const float minDistance = 0.0f;

 //   private bool isBallAtDest()
 //   {
 //       return (ball.position - targetPos).sqrMagnitude <= minDistance;
 //   }

 //   private void prepareBallForTransfer()
 //   {
 //       startPos = ball.position;
 //       float radius = ball.GetComponent<SphereCollider>().radius;

 //       targetPos.Set(targetPos.x, targetPos.y + radius, targetPos.z);

 //       startTime = 0;

 //       ball.velocity = Vector3.zero;
 //       ball.isKinematic = true;
 //   }

 //   private void OnTriggerStay(Collider other)
 //   {
 //       if (other.CompareTag("Ball"))
 //       {
 //           startTime += Time.deltaTime;

 //           if (startTime >= stayTime)
 //           {
 //               ballReadyToTransfer = true;
 //               ball = other.attachedRigidbody;
 //           }
 //       }
 //   }



