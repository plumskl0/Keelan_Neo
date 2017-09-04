using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateTurbine : MonoBehaviour {
    Animation animation;
	// Use this for initialization
	void Start () {
        //animation.Play

        animation = GetComponent<Animation>();
        //  .animation.Play()
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.B))
        {
            animation.Play("Fluegel|startRotation");
            Debug.Log("weiter");
            /*while (animation.IsPlaying("Fluegel|startRotation"))
            {
                Debug.Log("starteFluegel");
            }*/
            animation.Play("Fluegel|continuesRotation");
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("starte Rotation");
            animation.Play("Fluegel|startRotation");
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("führe Rotation fort");
            animation.Play("Fluegel|continuesRotation");
        }
		
	}
}
