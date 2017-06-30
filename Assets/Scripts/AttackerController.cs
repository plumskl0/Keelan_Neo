using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerController : MonoBehaviour {

    [Tooltip("Das Objekt, das den Angreifer darstellen soll.")]
    public GameObject attacker;

    [Tooltip("Wie schnell soll der Angreifer sein.")]
    public float attackerSpeed = 2f;

    [Tooltip("Bereich in dem der Angreifer den Ball versucht zu holen.")]
    public BallInZoneCheck attackZone;

    [Tooltip("Ort an dem der Ball vom Angreifer hingebracht werden soll.")]
    public Transform ballDropZone;

    // Initialer Ort der Angreifers
    private Vector3 atkInitPos;

    // Angreifer transform
    private Transform atkTransform;

    private Rigidbody ballRb;
    private float ballRadius;

    private float minDistance = 0.1f;

    private bool ballCatched;

    public void Start()
    {
        atkTransform = attacker.GetComponent<Transform>();
        atkInitPos = atkTransform.position;
    }

    public void Update()
    {
        if (attackZone.isBallInZone && !ballCatched)
        {
            Debug.Log("Ball is in Zone");
            // Wenn der Ball sich in der Zone befindet,
            // dann versucht der Angreifer den Ball zu holen
            if (ballRb == null)
            {
                ballRb = attackZone.ballRb;

                ballRadius = ballRb.GetComponent<SphereCollider>().radius;
            }

            float step = attackerSpeed * Time.deltaTime;

            Vector3 ballPos = GetBallPositionWithRadius(ballRb.position);

            atkTransform.position = Vector3.MoveTowards(atkTransform.position, ballPos, step);

            // Prüfen ob der Ball erreicht wurde
            ballCatched = isAtDest(atkTransform.position, ballPos);
        }
        else if (ballCatched)
        {
            Debug.Log("Caught the Ball and moving it");
            // Wenn der Ball gefangen wurde soll der am Zielort 
            // abgelegt werden
            if (ballRb.transform.parent == null)
            {
                ballRb.transform.parent = atkTransform;
                ballRb.isKinematic = true;
            }

            float step = attackerSpeed * Time.deltaTime;
            atkTransform.position = Vector3.MoveTowards(atkTransform.position, ballDropZone.position, step);

            if (isAtDest(atkTransform.position, ballDropZone.position))
            {
                ballCatched = false;
                atkTransform.DetachChildren();
                ballRb.isKinematic = false;
            }
        }
        else if (!isAtDest(atkTransform.position, atkInitPos))
        {
            // Wenn der Ball nicht in der Zone ist, 
            // und der Angreifer nicht an seinem Platz,
            // dann soll der Angreifer zurück zu seinem Ursprungsort
            float step = attackerSpeed * Time.deltaTime;
            atkTransform.position = Vector3.MoveTowards(atkTransform.position, atkInitPos, step);
        }
    }

    private bool isAtDest(Vector3 currentPos, Vector3 targetPos)
    {
        return (currentPos - targetPos).sqrMagnitude <= minDistance;
    }

    private Vector3 GetBallPositionWithRadius(Vector3 currentBallPos)
    {
        currentBallPos.y = currentBallPos.y + ballRadius*2;
        return currentBallPos;
    }
}